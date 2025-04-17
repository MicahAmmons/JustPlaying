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
        public static Vector2 GetMonsterMovePosition(Vector2 monPos, List<Vector2> playerControlledPositions, int moveDist)
        {
            TileCell startCell = TileManager.GetCell(monPos);

            // Step 1: Find the closest target cell
            TileCell closestTarget = null;
            float closestDist = float.MaxValue;
            foreach (var playerPos in playerControlledPositions)
            {
                TileCell targetCell = TileManager.GetCell(playerPos);
                float dist = Vector2.Distance(new Vector2(startCell.X, startCell.Y), new Vector2(targetCell.X, targetCell.Y));
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestTarget = targetCell;
                }
            }

            if (closestTarget == null)
                return monPos; // No valid target

            // Step 2: Run A* from monster to target
            List<TileCell> fullPath = AStar(startCell, closestTarget);

            if (fullPath == null || fullPath.Count <= 1)
                return monPos;

            // Step 3: Trim the path to the max move distance (number of tiles)
            int stepsToTake = Math.Min(moveDist, fullPath.Count - 1); // -1 to skip current cell
            TileCell destination = fullPath[stepsToTake];

            // Step 4: Return world position of target cell
            return TileManager.GetCellCords(destination);
        }
    }
}
