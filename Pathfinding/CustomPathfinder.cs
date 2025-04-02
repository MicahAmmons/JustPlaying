using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using PlayingAround.Game.Map;
using PlayingAround.Manager;

namespace PlayingAround.Game.Pathfinding
{
    public static class CustomPathfinder
    {
        public static List<Vector2> BuildPixelPath(Vector2 start, Vector2 end, int playerWidth, int playerHeight)
        { // Start is the FeetCenter
            Vector2 offset = new Vector2(playerWidth / 2f, playerHeight);
            end -= offset;
            var tile = TileManager.CurrentMapTile;
            if (tile == null)
                return new List<Vector2>();

            List<Vector2> path = new();
            float moveStep = MapTile.TileWidth/3; // movement in pixels (same as cell width)
            float closeEnough = moveStep / 2f + 1;
            Vector2 current = start;
            Vector2? previousDirection = null;
            int maxSteps = 500;
            int stepCount = 0;

            while (Vector2.Distance(current, end) > closeEnough && stepCount++ < maxSteps)
            {
                Vector2 direction = Vector2.Normalize(end - current);
                Vector2 nextStep = (current + direction * moveStep) ;
                Vector2 feetLandingPos = nextStep - offset;
                int cellX = (int)(feetLandingPos.X / MapTile.TileWidth);
                int cellY = (int)(feetLandingPos.Y / MapTile.TileHeight);

                if (IsWalkable(cellX, cellY))
                {
                    path.Add(nextStep);   // this is the FeetCenter target
                    previousDirection = direction;
                    current = nextStep;
                    continue;
                }

                // Diagonal case handling
                List<Vector2> fallbacks = GetFallbackDirections(direction, previousDirection);

                bool moved = false;
                foreach (var fallback in fallbacks)
                {
                    Vector2 candidate = current + fallback * moveStep;
                    int fallbackX = (int)(candidate.X / MapTile.TileWidth);
                    int fallbackY = (int)(candidate.Y / MapTile.TileHeight);

                    if (IsWalkable(fallbackX, fallbackY))
                    {
                        path.Add(candidate);
                        previousDirection = fallback;
                        current = candidate;
                        moved = true;
                        break;
                    }
                }

                if (!moved)
                    break; // blocked entirely
            }

            if (Vector2.Distance(current, end) <= closeEnough)
            {
                if (path.Count > 0)
                    path[path.Count - 1] = end; // Replace the last point
                else
                    path.Add(end); // Edge case: if path is empty

                return path;
            }


            return path;
        }
        public struct MovementInstruction
        {
            public Vector2 Direction;
            public float Distance;

            public MovementInstruction(Vector2 direction, float distance)
            {
                Direction = direction;
                Distance = distance;
            }
        }


        private static bool IsWalkable(int cellX, int cellY)
        {
            if (cellX < 0 || cellY < 0 || cellX >= MapTile.GridWidth || cellY >= MapTile.GridHeight)
                return false;

            return TileManager.CurrentMapTile.TileGrid[cellX, cellY].IsWalkable;
        }

        private static List<Vector2> GetFallbackDirections(Vector2 dir, Vector2? previousDir)
        {
            // Get the 4 cardinal and diagonal directions
            Vector2 north = new(0, -1);
            Vector2 south = new(0, 1);
            Vector2 east = new(1, 0);
            Vector2 west = new(-1, 0);
            Vector2 northeast = Vector2.Normalize(new Vector2(1, -1));
            Vector2 northwest = Vector2.Normalize(new Vector2(-1, -1));
            Vector2 southeast = Vector2.Normalize(new Vector2(1, 1));
            Vector2 southwest = Vector2.Normalize(new Vector2(-1, 1));

            // Compare direction to closest known
            List<Vector2> fallbackOrder = new();

            if (Vector2.Dot(dir, northeast) > 0.9f)
                fallbackOrder.AddRange(new[] { east, north, south, west });
            else if (Vector2.Dot(dir, northwest) > 0.9f)
                fallbackOrder.AddRange(new[] { west, north, south, east });
            else if (Vector2.Dot(dir, southeast) > 0.9f)
                fallbackOrder.AddRange(new[] { east, south, north, west });
            else if (Vector2.Dot(dir, southwest) > 0.9f)
                fallbackOrder.AddRange(new[] { west, south, north, east });
            else if (Vector2.Dot(dir, east) > 0.9f)
                fallbackOrder.AddRange(new[] { east, north, south, west });
            else if (Vector2.Dot(dir, west) > 0.9f)
                fallbackOrder.AddRange(new[] { west, north, south, east });
            else if (Vector2.Dot(dir, north) > 0.9f)
                fallbackOrder.AddRange(new[] { north, east, west, south });
            else if (Vector2.Dot(dir, south) > 0.9f)
                fallbackOrder.AddRange(new[] { south, east, west, north });

            if (previousDir.HasValue)
            {
                // Avoid reversing direction
                fallbackOrder.Remove(-previousDir.Value);
                fallbackOrder.Add(-previousDir.Value); // place it last
            }

            return fallbackOrder;
        }
    }
}
