using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using PlayingAround.Managers.Tiles;
using PlayingAround.Game.Map;

namespace PlayingAround.Game.Pathfinding
{
    public static class CustomPathfinder
    {
        private const int MaxSteps = 500;
        private const int ORTHOGONAL_COST = 10;
        private const int DIAGONAL_COST = 14;

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
                    if (neighbor.BlockedByCombatant) continue;
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
    }
}
