using Microsoft.Xna.Framework;
using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Entities.Monster.PlayMonsters;
using PlayingAround.Game.Map;
using PlayingAround.Manager;
using System;
using System.Collections;
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

        public static void MoveMonsters(GameTime gameTime, List<PlayMonsters> playMons)
        {
            foreach (var mon in playMons)
            {
                if (mon.MovementPattern == "arc" || mon.MovementPattern == "idle")
                {
                    if (Movement.NPCMovement.HandlePause(mon, gameTime))
                        continue;

                    Movement.NPCMovement.MoveTowardsNextPathPoint(mon, gameTime);
                }
            }
        }
        public static List<Vector2> MoveMonsters(CombatMonster mon, TileCell startingTile, TileCell endTile)
        {
            Vector2 start = TileManager.GetCellCords(startingTile);
            Vector2 destination = TileManager.GetCellCords(endTile);

            return mon.MovementPattern switch
            {
                "arc" or "idle" => ArcMovement(destination, start),
                "straight" => StraightMovement(destination, start),
                _ => StraightMovement(destination, start) // Fallback to straight if unknown pattern
            };
        }


        public static List<Vector2> StraightMovement(Vector2 endPoint, Vector2 start)
        {
            int steps = 15; // More steps = smoother straight line
            var path = new List<Vector2>();

            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;

                Vector2 point = Vector2.Lerp(start, endPoint, t); // Simple linear interpolation
                path.Add(point);
            }

            return path;
        }

        public static List<Vector2> ArcMovement(Vector2 endPoint, Vector2 start)
        {
            Vector2 end = endPoint;
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
            if (attempts == maxAttempts) 
            { 
                return spawnPoint; 
            }
            if (walkable == true)
            {
                return end;
            }
            else return spawnPoint;
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
        public static void MoveTowardsNextPathPoint(CombatMonster mon, List<Vector2> MovePath, GameTime gameTime)
        {
            if (MovePath == null || MovePath.Count == 0)
                return;

            Vector2 nextPoint = MovePath[0];
            float speed = mon.MovementQuickness * (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 direction = nextPoint - mon.currentPos;
            float distance = direction.Length();

            if (distance <= speed)
            {
                mon.currentPos = nextPoint;
                MovePath.RemoveAt(0);
            }
            else
            {
                direction.Normalize();
                mon.currentPos += direction * speed;
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
                    Vector2 end = FindEndPoint(mon.PacingBoundary, mon.CurrentPos);
                    mon.MovePath = Movement.NPCMovement.ArcMovement(end, mon.CurrentPos);
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
