using Microsoft.Xna.Framework;
using PlayingAround.Entities.Monster.PlayMonsters;
using PlayingAround.Game.Map;
using PlayingAround.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Managers.Movement
{
    public static class NPCMovement
    {
        public const float ArcHeight = 50f;
        public static List<Vector2> ArcMovement(Rectangle pacingBoundary, Vector2 start)
        {
            Vector2 end = FindEndPoint(pacingBoundary, start);

            // Create control point for arc — adjust the arc height (e.g., 50) for steeper arcs
            Vector2 control = new Vector2((start.X + end.X) / 2, MathF.Min(start.Y, end.Y) - ArcHeight);

            int steps = 15; // More steps = smoother arc
            var path = new List<Vector2>();

            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;

                Vector2 point = Vector2.Lerp(
                    Vector2.Lerp(start, control, t),
                    Vector2.Lerp(control, end, t),
                    t);

                path.Add(point);
            }

            return path;
        }




        public static Vector2 FindEndPoint(Rectangle bound, Vector2 spawnPoint)
        {
            Vector2 end = new Vector2(0, 0);
            Random rand = new Random();
            bool walkable = false;

            int halfWidth = bound.Width / 2;
            int halfHeight = bound.Height / 2;

            int minX = (int)spawnPoint.X - halfWidth;
            int maxX = (int)spawnPoint.X + halfWidth;

            int minY = (int)spawnPoint.Y - halfHeight;
            int maxY = (int)spawnPoint.Y + halfHeight;

            float minDistance = bound.Width / 5f;

            int attempts = 0;
            const int maxAttempts = 100;

            do
            {
                float x = rand.Next(minX, maxX);
                float y = rand.Next(minY, maxY);

                end = new Vector2(x, y);
                float distance = Vector2.Distance(spawnPoint, end);
                TileCell cell = TileManager.GetCell(end) ;
                if (distance >= minDistance &&
                    TileManager.IsCellWalkable(cell.X, cell.Y) &&
                    ViewportManager.IsPointWithinScreen(end, 50))
                {
                    walkable = true;
                }

                attempts++;
            } while (!walkable && attempts < maxAttempts);
            if (attempts == maxAttempts) { return spawnPoint; }
            else return end;
        }
        public static void MoveTowardsNextPathPoint(PlayMonsters mon, GameTime gameTime)
        {
            if (mon.MovePath == null || mon.MovePath.Count == 0)
                return;

            Vector2 nextPoint = mon.MovePath[0];
            float speed = mon.MovementSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 direction = nextPoint - mon.CurrentPos;
            float distance = direction.Length();

            if (distance <= speed)
            {
                mon.CurrentPos = nextPoint;
                mon.MovePath.RemoveAt(0);
            }
            else
            {
                direction.Normalize();
                mon.CurrentPos += direction * speed;
            }
        }
        public static bool HandlePause(PlayMonsters mon, GameTime gameTime)
        {
            if (mon.IsPaused)
            {
                mon.PauseTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (mon.PauseTimer <= 0)
                {
                    mon.IsPaused = false;
                    mon.MovePath = Movement.NPCMovement.ArcMovement(mon.PacingBoundary, mon.CurrentPos);
                }

                return true; // Still paused this frame
            }

            // If movement path is empty, trigger a pause
            if (mon.MovePath == null || mon.MovePath.Count == 0)
            {
                mon.IsPaused = true;
                mon.PauseTimer = mon.PauseDuration;
                return true; // Pausing now
            }

            return false; // Not paused, movement can continue
        }






    }
}
