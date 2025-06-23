using Microsoft.Xna.Framework;
using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Entities.Monster.PlayMonsters;
using PlayingAround.Game.Map;
using PlayingAround.Interfaces;
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


        public static List<Vector2> GetMovementPatternVector2List(MovementPatternType pattern, TileCell startingTile, TileCell endTile)
        {
            Vector2 start = startingTile.CenterPoint;
            Vector2 destination = endTile.CenterPoint;

            return pattern switch
            {
                MovementPatternType.Arc  => ArcMovement(destination, start),
                MovementPatternType.Straight => StraightMovement(destination, start),
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
     
      

       
    }
}
public enum MovementPatternType
{
    Arc,
    Straight,
    None

}