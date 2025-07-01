using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Game.Map;
using PlayingAround.Managers.Assets;
using PlayingAround.Managers.CombatMan;
using PlayingAround.Managers.CombatMan.CombatAttacks;
using PlayingAround.Managers.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlayingAround.Managers.Movement
{
    public static class GridMovement
    {
        private const int MaxSteps = 500;
        private const int ORTHOGONAL_COST = 10;
        private const int DIAGONAL_COST = 14;
        public static List<TileCell> BestPathToClosestCell(TileCell current, List<TileCell> targets, int maxSteps)
        {
            List<TileCell> bestPath = null;
            int shortestPathLength = 30;

            foreach (var target in targets)
            {
                List<TileCell> path = GetCellToCellPath(current.CenterPoint, target.CenterPoint);
                if (path.Count > 0 && path.Count < shortestPathLength)
                {
                    shortestPathLength = path.Count;
                    bestPath = path;
                    if (bestPath.Contains(target)) bestPath.Remove(target);
                    if (bestPath.Contains(current)) bestPath.Remove(current);
                }
            }
            
            return bestPath.Count > maxSteps ? bestPath.Take(maxSteps).ToList() : bestPath;
        }
        public static List<TileCell> GetCellToCellPath(Vector2 startPixel, Vector2 endPixel)
        {
            TileCell startCell = TileManager.GetCell(startPixel);
            TileCell goalCell = TileManager.GetCell(endPixel);

            if (startCell == null || goalCell == null)
                return new();

            List<TileCell> cellPath = FindCellPath(startCell, goalCell);

            return cellPath.ToList();
        }
        private static List<TileCell> FindCellPath(TileCell start, TileCell goal)
        {
            var openSet = new PriorityQueue<TileCell, int>();
            var cameFrom = new Dictionary<TileCell, TileCell>();
            var gScore = new Dictionary<TileCell, int>();
            var fScore = new Dictionary<TileCell, int>();

            gScore[start] = 0;
            fScore[start] = Heuristic(start, goal);

            openSet.Enqueue(start, fScore[start]);

            int steps = 0;

            while (openSet.Count > 0 && steps++ < MaxSteps)
            {
                var current = openSet.Dequeue();

                if (current == goal)
                    return ReconstructPath(cameFrom, current);

                foreach (var neighbor in TileManager.GetWalkableNeighbors(current))
                {
                    if (neighbor.BlockedByMonster && neighbor != goal) 
                        continue;
                    int moveCost = IsDiagonal(current, neighbor) ? DIAGONAL_COST : ORTHOGONAL_COST;
                    int tentativeG = gScore[current] + moveCost;

                    if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeG;
                        fScore[neighbor] = tentativeG + Heuristic(neighbor, goal);
                        openSet.Enqueue(neighbor, fScore[neighbor]);
                    }
                }
            }

            return new(); // no path
        }
        private static List<TileCell> ReconstructPath(Dictionary<TileCell, TileCell> cameFrom, TileCell current)
        {
            var path = new List<TileCell> { current };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Insert(0, current);
            }
            return path;
        }
        private static int Heuristic(TileCell a, TileCell b)
        {
            int dx = Math.Abs(a.X - b.X);
            int dy = Math.Abs(a.Y - b.Y);
            return 10 * Math.Max(dx, dy); // Chebyshev distance
        }
        private static bool IsDiagonal(TileCell a, TileCell b)
        {
            return Math.Abs(a.X - b.X) == 1 && Math.Abs(a.Y - b.Y) == 1;
        }
        public static (List<Vector2>, List<Vector2>) SplitAttackPath(List<Vector2> attackPath)
        {
            List<Vector2> result1 = null;
            List<Vector2> result2 = null;

            result1 = new List<Vector2>();
            int half = attackPath.Count / 2;
            for (int i = 0; i < half; i++) 
            { 
              result1.Add(attackPath[i]);
              result2 = new List<Vector2>(result1);
              result2.Reverse(); 
            }
            return (result1, result2);
        }
       
        
       
        
    }
}
