using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PlayingAround.Game.Map;
using PlayingAround.Managers.Tiles;

namespace PlayingAround.Game.Pathfinding
{
    public static class CustomPathfinder
    {
        private static float moveStep = 5f;
        private static float closeEnough = 3f;
        private const int MaxSteps = 500;

        public static List<Vector2> BuildPixelPath(Vector2 start, Vector2? endd)
        {
            List<Vector2> path = new();
            if (endd == null) return path;

            Vector2 end = (Vector2)endd;
            Vector2 current = start;

            int steps = 0;
            while (Vector2.Distance(current, end) > closeEnough && steps++ < MaxSteps)
            {
                Vector2 direction = end - current;
                if (direction.Length() > 0)
                    direction.Normalize();

                Vector2 next = current + direction * moveStep;

                var cell = TileManager.GetCell(next);
                if (cell != null && TileManager.IsCellWalkable(cell.X, cell.Y))
                {
                    path.Add(next);
                    current = next;
                }
                else
                {
                    break; // first unwalkable cell, stop building path
                }
            }

            // If close enough and destination cell is walkable, add the end
            var endCell = TileManager.GetCell(end);
            if (Vector2.Distance(current, end) <= closeEnough &&
                endCell != null && TileManager.IsCellWalkable(endCell.X, endCell.Y))
            {
                path.Add(end);
            }

            return path;
        }
    }


}
