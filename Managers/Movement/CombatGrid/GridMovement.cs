using Microsoft.Xna.Framework;
using PlayingAround.Game.Map;
using PlayingAround.Manager;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlayingAround.Managers.Movement.CombatGrid
{
    public static class GridMovement
    {
        public static TileCell GetMonsterMovePosition(TileCell current, List<TileCell> targets)
        {
            TileCell closest = null;
            int bestDistance = int.MaxValue;

            foreach (var target in targets)
            {
                int distance = Math.Abs(current.X - target.X) + Math.Abs(current.Y - target.Y); // Manhattan distance

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    closest = target;
                }
            }

            return closest;
        }
        public static List<TileCell> FindPath(Vector2 startPos, TileCell endPos, int maxSteps)
        {
            TileCell startCell = TileManager.GetCell(startPos);
            TileCell endCell = endPos;
            endCell.BlockedByMonster = false;

            if (startCell == null || endCell == null || !endCell.IsWalkable)
                return new List<TileCell>();

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


                foreach (TileCell neighbor in TileManager.GetWalkableNeighbors(current))
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

        private static float Heuristic(TileCell a, TileCell b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y); // Manhattan distance
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

            if (path.Count > 0)
                path.RemoveAt(0); // Remove the starting cell (already on it)

            if (path.Count > 0 && path[^1] == path.Last())
                path.RemoveAt(path.Count - 1); // Remove last if it's redundant

            // 🔥 Stop at first blocked cell
            List<TileCell> result = new();
            foreach (var cell in path.Take(maxSteps))
            {
                if (cell.BlockedByMonster)
                    break;
                result.Add(cell);
            }

            return result;
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
