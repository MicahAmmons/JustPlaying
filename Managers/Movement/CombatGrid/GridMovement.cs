using Microsoft.Xna.Framework;
using PlayingAround.Game.Map;
using PlayingAround.Manager;
using PlayingAround.Managers.CombatMan;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlayingAround.Managers.Movement.CombatGrid
{
    public static class GridMovement
    {
        public static List<TileCell> FindClosestTargetPath(TileCell current, List<TileCell> targets, int maxSteps)
        {
            List<TileCell> bestPath = null;
            int shortestPathLength = int.MaxValue;

            foreach (var target in targets)
            {

                List<TileCell> path = FindPath(current, target, maxSteps);

                if (path.Count > 0 && path.Count < shortestPathLength)
                {
                    shortestPathLength = path.Count;
                    bestPath = path;
                }
            }

            return bestPath ?? new List<TileCell>(); // return empty if none found
        }



        public static List<TileCell> FindPath(TileCell startPos, TileCell endPos, int maxSteps)
        {
            TileCell startCell = startPos;
            TileCell endCell = endPos;

            var openSet = new PriorityQueue<TileCell>();
            var cameFrom = new Dictionary<TileCell, TileCell>();
            var gScore = new Dictionary<TileCell, int>();
            var fScore = new Dictionary<TileCell, float>();

            openSet.Enqueue(startCell, 0);
            gScore[startCell] = 0;
            fScore[startCell] = Heuristic(startCell, endCell);

            while (openSet.Count > 0)
            {
                TileCell current = openSet.Dequeue();

                if (current.X == endCell.X && current.Y == endCell.Y)
                {
                    endCell.BlockedByMonster = true;
                    return ReconstructPath(cameFrom, current, maxSteps); 
                }
                foreach (TileCell neighbor in TileManager.GetWalkableNeighbors(current, endCell))
                {
                    int tentativeG = gScore[current] + 1;

                    if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeG;
                        fScore[neighbor] = tentativeG + Heuristic(neighbor, endCell);

                        if (!openSet.Contains(neighbor))
                            openSet.Enqueue(neighbor, fScore[neighbor]);

                    }
                }
            }
            return new List<TileCell>();
            // No path found
        }


        private static List<TileCell> ReconstructPath(Dictionary<TileCell, TileCell> cameFrom, TileCell current, int maxSteps)
        {
            List<TileCell> path = new();
            path.Add(current);

            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Add(current);
            }

            path.Reverse();

            if (path.Count > 0 )
                path.RemoveAt(path.Count - 1); // optional redundancy check, can be removed too

            return path.Take(maxSteps).ToList();
        }
        public static int CheckManhattanDistance(TileCell origin, TileCell destination)
        {
            if (origin == null || destination == null)
                throw new ArgumentNullException("One or both TileCells are null.");

            int dx = Math.Abs(origin.X - destination.X);
            int dy = Math.Abs(origin.Y - destination.Y);

            return dx + dy;
        }


        private static float Heuristic(TileCell a, TileCell b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y); // Manhattan distance
        }
        public class PriorityQueue<T>
        {
            private readonly List<(T item, float priority)> elements = new();

            public int Count => elements.Count;

            public void Enqueue(T item, float priority)
            {
                elements.Add((item, priority));
            }

            public T Dequeue()
            {
                int bestIndex = 0;
                float bestPriority = elements[0].priority;

                for (int i = 1; i < elements.Count; i++)
                {
                    if (elements[i].priority < bestPriority)
                    {
                        bestPriority = elements[i].priority;
                        bestIndex = i;
                    }
                }

                T bestItem = elements[bestIndex].item;
                elements.RemoveAt(bestIndex);
                return bestItem;
            }

            public bool Contains(T item)
            {
                return elements.Any(e => EqualityComparer<T>.Default.Equals(e.item, item));
            }
        }



    }
}
