using Microsoft.Xna.Framework;
using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Entities.Monster.PlayMonsters;
using PlayingAround.Game.Map;
using PlayingAround.Managers.Tiles;
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

        public static void GetPlayMonsterMovementPath(List<PlayMonsters> playMons, GameTime gameTime)
        {
            foreach (var mon in playMons)
            {
                if (mon.MovementPattern == "arc" || mon.MovementPattern == "idle")
                {
                    if (HandlePause(mon, gameTime))
                        continue;

                  
                }
            }
        }

        public static List<Vector2> MoveMonsters(CombatMonster mon, TileCell startingTile, TileCell endTile)
        {
            Vector2 start = startingTile.CenterPoint;
            Vector2 destination = endTile.CenterPoint;

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




        public static Vector2 FindEndPoint(Vector2 spawnPoint)
        {
            
            var tiles = TileManager.GetWalkableNeighbors(TileManager.GetCell(spawnPoint));
            if (tiles.Count > 0) return tiles[RandomHut.rng.Next(0, tiles.Count - 1)].CenterPoint;
            else return spawnPoint;
        }

     
        public static bool HandlePause(PlayMonsters mon, GameTime gameTime)
        {
            if (mon.IsPaused)
            {
                mon.PauseTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (mon.PauseTimer <= 0)
                {
                    mon.IsPaused = false;
                    SetCurrentPauseDuration(mon);
                    Vector2 end = FindEndPoint(mon.CurrentPos);
                    mon.MovePath = Movement.NPCMovement.ArcMovement(end, mon.CurrentPos);
                }

                return true; // Still paused this frame
            }

            // If movement path is empty, trigger a pause
            if (mon.MovePath == null || mon.MovePath.Count == 0)
            {
                mon.IsPaused = true;
                mon.PauseTimer = mon.CurrentPauseDuration;
                return true; // Pausing now
            }

            return false; // Not paused, movement can continue
        }

        private static void SetCurrentPauseDuration(PlayMonsters mon)
        {
            mon.CurrentPauseDuration = MathF.Round((float)(mon.PauseDurationMin + RandomHut.rng.NextDouble() * (mon.PauseDurationMax - mon.PauseDurationMin)),2);
        
    }
    }
}
