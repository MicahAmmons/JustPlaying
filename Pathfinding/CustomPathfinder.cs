using System;
using System.Collections.Generic;
using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using PlayingAround.Game.Map;
using PlayingAround.Manager;

namespace PlayingAround.Game.Pathfinding
{
    public static class CustomPathfinder
    {
        public static List<Vector2> BuildPixelPath(Rectangle start, Vector2 end)
        {
            List<Vector2> path = new();

            // Step 1: Define foot hitbox corners
            Vector2 topLeftFootHitbox = new Vector2(start.Left, start.Top);
            Vector2 topRightFootHitbox = new Vector2(start.Right, start.Top);
            Vector2 bottomLeftFootHitbox = new Vector2(start.Left, start.Bottom);
            Vector2 bottomRightFootHitbox = new Vector2(start.Right, start.Bottom);
            Vector2 feetBoxCenter = new Vector2(start.Left + start.Width / 2f, bottomLeftFootHitbox.Y);
            var tile = TileManager.CurrentMapTile;
            if (tile == null)
                return new List<Vector2>();

            float moveStep = start.Height;
            float closeEnough = (start.Height/2) + 1; // if the character lands just over halfway the distancec
            Vector2 currentTopLeft = topLeftFootHitbox;
            Vector2 currentTopRight = topRightFootHitbox;
            Vector2 currentBottomLeft = bottomLeftFootHitbox;
            Vector2 currentBottomRight = bottomRightFootHitbox;
            Vector2? previousTopLeft = null;
            Vector2? previousTopRight = null;
            Vector2? previousBottomLeft = null;
            Vector2? previousBottomRight = null;
            Vector2? previousFeetBoxCenter = null;

            Vector2 offset = new Vector2(-start.Width / 2f, -start.Height*3);

            int maxSteps = 500;
            int stepCount = 0;

            while (Vector2.Distance(feetBoxCenter, end) > closeEnough && stepCount++ < maxSteps)
            {
                Vector2 direction = end - feetBoxCenter;
                if (direction.Length() > 0)
                    direction.Normalize();

                Vector2 moveVector = direction * moveStep;

                previousTopLeft = currentTopLeft;
                previousTopRight = currentTopRight;
                previousBottomLeft = currentBottomLeft;
                previousBottomRight = currentBottomRight;
                previousFeetBoxCenter = feetBoxCenter;

                currentTopLeft += moveVector;
                currentTopRight += moveVector;
                currentBottomLeft += moveVector;
                currentBottomRight += moveVector;
                feetBoxCenter += moveVector;

                if (IsCornerWalkable((int)currentTopLeft.X, (int)currentTopLeft.Y, tile) &&
                    IsCornerWalkable((int)currentTopRight.X - 1, (int)currentTopRight.Y, tile) &&
                    IsCornerWalkable((int)currentBottomLeft.X, (int)currentBottomLeft.Y - 1, tile) &&
                    IsCornerWalkable((int)currentBottomRight.X - 1, (int)currentBottomRight.Y - 1, tile))
                {
                    path.Add(feetBoxCenter);
                }
                else 
                { 
                    for (int i = 1; i <= moveStep; i++)
                    {
                        Vector2 trialVector = direction * (moveStep - i);

                        Vector2 testTopLeft = previousTopLeft.Value + trialVector;
                        Vector2 testTopRight = previousTopRight.Value + trialVector;
                        Vector2 testBottomLeft = previousBottomLeft.Value + trialVector;
                        Vector2 testBottomRight = previousBottomRight.Value + trialVector;
                        Vector2 testCenter = previousFeetBoxCenter.Value + trialVector;

                        if (IsCornerWalkable((int)testTopLeft.X, (int)testTopLeft.Y, tile) &&
                            IsCornerWalkable((int)testTopRight.X - 1, (int)testTopRight.Y, tile) &&
                            IsCornerWalkable((int)testBottomLeft.X, (int)testBottomLeft.Y - 1, tile) &&
                            IsCornerWalkable((int)testBottomRight.X - 1, (int)testBottomRight.Y - 1, tile))
                        {
                            path.Add(testCenter);
                            break;
                        }
                    }
                break; // Stop pathing if the next step is not valid
                }
            }

            // Add final destination if it's close enough
            if (Vector2.Distance(feetBoxCenter, end) <= closeEnough)
            {
                path.Add(end);
            }


            // Apply offset to each point
            for (int i = 0; i < path.Count; i++)
            {
                path[i] += offset;
            }
            return path;
        }

        private static bool IsCornerWalkable(int pixelX, int pixelY, MapTile tile)
        {
            int cellX = pixelX / MapTile.TileWidth;
            int cellY = pixelY / MapTile.TileHeight;

            return IsCellWalkable(cellX, cellY, tile);
        }

        private static bool IsCellWalkable(int x, int y, MapTile tile)
        {
            if (x < 0 || y < 0 || x >= MapTile.GridWidth || y >= MapTile.GridHeight)
                return false;

            return tile.TileGrid[x, y].IsWalkable;
        }

        //// Start is the FeetCenter
        //Vector2 offset = new Vector2(playerWidth / 2f, playerHeight);
        //        end -= offset;
        //        int maxSteps = 500;
        //        int stepCount = 0;

        //        while (Vector2.Distance(current, end) > closeEnough && stepCount++ < maxSteps)
        //        {
        //            Vector2 direction = Vector2.Normalize(end - current);
        //            Vector2 nextStep = (current + direction * moveStep) ;
        //            Vector2 feetLandingPos = nextStep - offset;
        //            int cellX = (int)(feetLandingPos.X / MapTile.TileWidth);
        //            int cellY = (int)(feetLandingPos.Y / MapTile.TileHeight);

        //            if (IsWalkable(cellX, cellY))
        //            {
        //                path.Add(nextStep);   // this is the FeetCenter target
        //                previousDirection = direction;
        //                current = nextStep;
        //                continue;
        //            }

        //            // Diagonal case handling
        //            List<Vector2> fallbacks = GetFallbackDirections(direction, previousDirection);

        //            bool moved = false;
        //            foreach (var fallback in fallbacks)
        //            {
        //                Vector2 candidate = current + fallback * moveStep;
        //                int fallbackX = (int)(candidate.X / MapTile.TileWidth);
        //                int fallbackY = (int)(candidate.Y / MapTile.TileHeight);

        //                if (IsWalkable(fallbackX, fallbackY))
        //                {
        //                    path.Add(candidate);
        //                    previousDirection = fallback;
        //                    current = candidate;
        //                    moved = true;
        //                    break;
        //                }
        //            }

        //            if (!moved)
        //                break; // blocked entirely
        //        }

        //        if (Vector2.Distance(current, end) <= closeEnough)
        //        {
        //            if (path.Count > 0)
        //                path[path.Count - 1] = end; // Replace the last point
        //            else
        //                path.Add(end); // Edge case: if path is empty

        //            return path;
        //        }


        //        return path;







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
