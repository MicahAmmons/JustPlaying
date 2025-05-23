﻿//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using Microsoft.VisualBasic;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Input;
//using PlayingAround.Game.Map;
//using PlayingAround.Manager;

//namespace PlayingAround.Game.Pathfinding
//{
//    public static class CustomPathfinder
//    {
//        private static bool straightLine = true;
//        public static List<Vector2> BuildPixelPath(Rectangle start, Vector2 end)
//        {
//            List<Vector2> path = new();
//            var tile = TileManager.CurrentMapTile;
//            if (tile == null)
//                return new List<Vector2>();
//            if (!IsCornerWalkable((int)end.X, (int)end.Y, tile))
//            {
//                return path = null;
//            }
//            // Step 1: Define foot hitbox corners
//            Vector2 topLeftFootHitbox = new Vector2(start.Left, start.Top);
//            Vector2 topRightFootHitbox = new Vector2(start.Right, start.Top);
//            Vector2 bottomLeftFootHitbox = new Vector2(start.Left, start.Bottom);
//            Vector2 bottomRightFootHitbox = new Vector2(start.Right, start.Bottom);
//            Vector2 feetBoxCenter = new Vector2(start.Left + start.Width / 2f, bottomLeftFootHitbox.Y);


//            float moveStep = start.Height;
//            float closeEnough = (start.Height / 2) + 1; // if the character lands just over halfway the distancec
//            Vector2 currentTopLeft = topLeftFootHitbox;
//            Vector2 currentTopRight = topRightFootHitbox;
//            Vector2 currentBottomLeft = bottomLeftFootHitbox;
//            Vector2 currentBottomRight = bottomRightFootHitbox;
//            Vector2? previousTopLeft = null;
//            Vector2? previousTopRight = null;
//            Vector2? previousBottomLeft = null;
//            Vector2? previousBottomRight = null;
//            Vector2? previousFeetBoxCenter = null;

//            Vector2 offset = new Vector2(-start.Width / 2f, -start.Height * 3);

//            int maxSteps = 500;
//            int stepCount = 0;

//            while (Vector2.Distance(feetBoxCenter, end) > closeEnough && stepCount++ < maxSteps)
//            {
//                Vector2 direction = end - feetBoxCenter;
//                if (direction.Length() > 0)
//                    direction.Normalize();

//                Vector2 moveVector = direction * moveStep;

//                previousTopLeft = currentTopLeft;
//                previousTopRight = currentTopRight;
//                previousBottomLeft = currentBottomLeft;
//                previousBottomRight = currentBottomRight;
//                previousFeetBoxCenter = feetBoxCenter;

//                currentTopLeft += moveVector;
//                currentTopRight += moveVector;
//                currentBottomLeft += moveVector;
//                currentBottomRight += moveVector;
//                feetBoxCenter += moveVector;

//                if (IsCornerWalkable((int)currentTopLeft.X, (int)currentTopLeft.Y, tile) &&
//                    IsCornerWalkable((int)currentTopRight.X - 1, (int)currentTopRight.Y, tile) &&
//                    IsCornerWalkable((int)currentBottomLeft.X, (int)currentBottomLeft.Y - 1, tile) &&
//                    IsCornerWalkable((int)currentBottomRight.X - 1, (int)currentBottomRight.Y - 1, tile))

//                {
//                    path.Add(feetBoxCenter);
//                }
//                else
//                {
//                    straightLine = false;
//                    for (int i = 1; i <= moveStep; i++)
//                    {
//                        Vector2 trialVector = direction * (moveStep - i);

//                        Vector2 testTopLeft = previousTopLeft.Value + trialVector;
//                        Vector2 testTopRight = previousTopRight.Value + trialVector;
//                        Vector2 testBottomLeft = previousBottomLeft.Value + trialVector;
//                        Vector2 testBottomRight = previousBottomRight.Value + trialVector;
//                        Vector2 testCenter = previousFeetBoxCenter.Value + trialVector;

//                        if (IsCornerWalkable((int)testTopLeft.X, (int)testTopLeft.Y, tile) &&
//                            IsCornerWalkable((int)testTopRight.X - 1, (int)testTopRight.Y, tile) &&
//                            IsCornerWalkable((int)testBottomLeft.X, (int)testBottomLeft.Y - 1, tile) &&
//                            IsCornerWalkable((int)testBottomRight.X - 1, (int)testBottomRight.Y - 1, tile))
//                        {
//                            path.Add(testCenter);
//                            currentTopLeft = testTopLeft;
//                            currentTopRight = testTopRight;
//                            currentBottomLeft = testBottomLeft;
//                            currentBottomRight = testBottomRight;
//                            feetBoxCenter = testCenter;
//                            break;
//                        }
//                    }
//                    direction = end - feetBoxCenter;

//                    Vector2[] verticalPointToTry = FindFirstOpeningToResume(direction, moveStep, feetBoxCenter);
//                    path.Add(verticalPointToTry[0]);
//                    path.Add(verticalPointToTry[1]);
//                    previousTopLeft = currentTopLeft;
//                    previousTopRight = currentTopRight;
//                    previousBottomLeft = currentBottomLeft;
//                    previousBottomRight = currentBottomRight;
//                    previousFeetBoxCenter = feetBoxCenter;

//                    // Update the current positions to reflect the new points
//                    currentTopLeft = verticalPointToTry[0];
//                    currentTopRight = verticalPointToTry[1]; // Assuming verticalPointToTry[1] aligns with the right corner
//                    currentBottomLeft = verticalPointToTry[0]; // Align the bottom corners similarly
//                    currentBottomRight = verticalPointToTry[1]; // Same for the bottom-right corner
//                    feetBoxCenter = (verticalPointToTry[0] + verticalPointToTry[1]) / 2; // Assuming the center is midway between the two points

//                }

//            }

//            // Add final destination if it's close enough
//            if (Vector2.Distance(feetBoxCenter, end) <= closeEnough)
//            {
//                path.Add(end);
//            }


//            // Apply offset to each point
//            for (int i = 0; i < path.Count; i++)
//            {
//                path[i] += offset;
//            }

//            if (!straightLine)
//            {
//                var finalPath = MakePathEfficient(path);
//                return finalPath;
//            }
//            return path;

//        }
//        private static List<Vector2> MakePathEfficient(List<Vector2> path)
//        {
//            if (path == null || path.Count < 2)
//                return path;

//            List<Vector2> efficientPath = new List<Vector2>();

//            efficientPath.Add(path[0]);

//            for (int i = 1; i < path.Count - 1; i++)
//            {
//                Vector2 prev = path[i - 1];
//                Vector2 current = path[i];
//                Vector2 next = path[i + 1];

//                // If current shares X with both neighbors, it's part of a vertical run
//                bool sameXRun = prev.X == current.X && current.X == next.X;

//                // If current shares Y with both neighbors, it's part of a horizontal run
//                bool sameYRun = prev.Y == current.Y && current.Y == next.Y;

//                // If not part of a flat run, keep it
//                if (!sameXRun && !sameYRun)
//                {
//                    efficientPath.Add(current);
//                }
//            }

//            efficientPath.Add(path[^1]); // Add the last point

//            return efficientPath;
//        }






//        private static bool CheckPathForBlockage(Vector2 start, Vector2 end)
//        {
//            var tile = TileManager.CurrentMapTile;
//            if (tile == null)
//                return false;

//            Vector2 direction = Vector2.Normalize(end - start);
//            float distance = Vector2.Distance(start, end);
//            int steps = (int)(distance / 32); // You can tweak the precision here

//            for (int i = 1; i < steps; i++)
//            {
//                Vector2 point = start + direction * (i * 32);
//                return false;
//            }

//            return true;
//        }


//        private static Vector2[] FindFirstOpeningToResume(Vector2 direction, float moveStep, Vector2 currentFeetPos)
//        {
//            // Proper x/y variables for movement directions
//            Vector2 primaryVertical = new Vector2(0, 0);    // Vertical primary movement direction (e.g., North/South)
//            Vector2 secondaryVertical = new Vector2(0, 0);  // Vertical secondary movement direction (opposite of primary)
//            Vector2 primaryHorizontal = new Vector2(0, 0);   // Horizontal primary movement direction (e.g., East/West)
//            Vector2 secondaryHorizontal = new Vector2(0, 0); // Horizontal secondary movement direction (opposite of primary)


//            if (direction.X < 0 && direction.Y < 0) // Northwest
//            {
//                primaryVertical.Y = -1;  // North
//                secondaryVertical.Y = 1; // South
//                primaryHorizontal.X = -1; // West
//                secondaryHorizontal.X = 1; // East
//            }
//            else if (direction.X < 0 && direction.Y > 0) // Southwest
//            {
//                primaryVertical.Y = 1;   // South
//                secondaryVertical.Y = -1; // North
//                primaryHorizontal.X = -1; // West
//                secondaryHorizontal.X = 1; // East
//            }
//            else if (direction.X > 0 && direction.Y < 0) // Northeast
//            {
//                primaryVertical.Y = -1;  // North
//                secondaryVertical.Y = 1; // South
//                primaryHorizontal.X = 1; // East
//                secondaryHorizontal.X = -1; // West
//            }
//            else if (direction.X > 0 && direction.Y > 0) // Southeast
//            {
//                primaryVertical.Y = 1;   // South
//                secondaryVertical.Y = -1; // North
//                primaryHorizontal.X = 1; // East
//                secondaryHorizontal.X = -1; // West
//            }

//            Vector2 originalPosition = currentFeetPos; // Save original position to reset if needed
//            Vector2 verticalMovement = Vector2.Zero;
//            Vector2 horizontalMovement = Vector2.Zero;
//            bool primaryFailedFirstTry = false;

//            for (int i = 1; i <= 500; i++)
//            {
//                // Calculate the vertical step for this iteration (moveStep * i)
//                Vector2 verticalPos = originalPosition + primaryVertical * (moveStep * i);

//                if (IsCornerWalkable((int)verticalPos.X, (int)verticalPos.Y, TileManager.CurrentMapTile))
//                {
//                    verticalMovement = verticalPos - originalPosition;  // Log vertical movement
//                    currentFeetPos = verticalPos; // Update position after vertical move

//                    // Now try the horizontal direction (East/West)
//                    Vector2 horizontalPos = currentFeetPos + primaryHorizontal * moveStep;
//                    if (IsCornerWalkable((int)horizontalPos.X, (int)horizontalPos.Y, TileManager.CurrentMapTile) &&
//IsCornerWalkable((int)horizontalPos.X - 32, (int)horizontalPos.Y - 64 / 3, TileManager.CurrentMapTile) &&
//IsCornerWalkable((int)horizontalPos.X - 32, (int)horizontalPos.Y, TileManager.CurrentMapTile) &&
//IsCornerWalkable((int)horizontalPos.X + 32, (int)horizontalPos.Y - 64 / 3, TileManager.CurrentMapTile) &&
//IsCornerWalkable((int)horizontalPos.X + 32, (int)horizontalPos.Y, TileManager.CurrentMapTile))

//                    {
//                        horizontalMovement = horizontalPos - currentFeetPos;  // Log horizontal movement
//                        return new Vector2[] { originalPosition + verticalMovement, originalPosition + horizontalMovement + verticalMovement };  // Return both movements
//                    }

//                    // If horizontal movement fails, continue with the next iteration and increase the vertical step
//                }
//                else
//                {
//                    // If vertical movement fails, we stop trying this primary vertical direction
//                    if (i == 2 || i == 1)
//                    {
//                        primaryFailedFirstTry = true;
//                    }
//                    break;
//                }
//            }

//            // If primary vertical movement failed, try the secondary vertical direction (North/South)
//            if (!primaryFailedFirstTry)
//            {
//                for (int i = 2; i <= 500; i++)
//                {
//                    // Calculate the vertical step for this iteration (moveStep * i)
//                    Vector2 secondaryVerticalPos = originalPosition + secondaryVertical * (moveStep * i);

//                    if (IsCornerWalkable((int)secondaryVerticalPos.X, (int)secondaryVerticalPos.Y, TileManager.CurrentMapTile) &&
//    IsCornerWalkable((int)secondaryVerticalPos.X - 32, (int)secondaryVerticalPos.Y - 64 / 3, TileManager.CurrentMapTile) &&
//    IsCornerWalkable((int)secondaryVerticalPos.X - 32, (int)secondaryVerticalPos.Y, TileManager.CurrentMapTile) &&
//    IsCornerWalkable((int)secondaryVerticalPos.X + 32, (int)secondaryVerticalPos.Y - 64 / 3, TileManager.CurrentMapTile) &&
//    IsCornerWalkable((int)secondaryVerticalPos.X + 32, (int)secondaryVerticalPos.Y, TileManager.CurrentMapTile))

//                    {
//                        verticalMovement = secondaryVerticalPos - originalPosition;  // Log secondary vertical movement
//                        currentFeetPos = secondaryVerticalPos; // Update position after secondary vertical move


//                        // Now try the horizontal direction (East/West)
//                        Vector2 secondaryHorizontalPos = currentFeetPos + primaryHorizontal * moveStep;
//                        if (IsCornerWalkable((int)secondaryHorizontalPos.X, (int)secondaryHorizontalPos.Y, TileManager.CurrentMapTile) &&
//    IsCornerWalkable((int)secondaryHorizontalPos.X - 32, (int)secondaryHorizontalPos.Y - 64 / 3, TileManager.CurrentMapTile) &&
//    IsCornerWalkable((int)secondaryHorizontalPos.X - 32, (int)secondaryHorizontalPos.Y, TileManager.CurrentMapTile) &&
//    IsCornerWalkable((int)secondaryHorizontalPos.X + 32, (int)secondaryHorizontalPos.Y - 64 / 3, TileManager.CurrentMapTile) &&
//    IsCornerWalkable((int)secondaryHorizontalPos.X + 32, (int)secondaryHorizontalPos.Y, TileManager.CurrentMapTile))
//                        {
//                            horizontalMovement = secondaryHorizontalPos - currentFeetPos; // Log horizontal movement
//                            return new Vector2[] { originalPosition + verticalMovement, originalPosition + horizontalMovement + verticalMovement }; // Return both movements
//                        }

//                        // If horizontal movement fails, continue with the next iteration and increase the vertical step
//                    }
//                    else
//                    {
//                        // If secondary vertical movement fails, we stop trying this secondary vertical direction
//                        break;
//                    }
//                }
//            }





//            if (primaryFailedFirstTry)
//            {
//                // Try horizontal movement first
//                for (int i = 1; i <= 500; i++)
//                {
//                    // Calculate the horizontal step for this iteration (moveStep * i)
//                    Vector2 horizontalPos = originalPosition + primaryHorizontal * (moveStep * i) + new Vector2(0, -20);

//                    if (IsCornerWalkable((int)horizontalPos.X, (int)horizontalPos.Y, TileManager.CurrentMapTile) &&
//    IsCornerWalkable((int)horizontalPos.X - 32, (int)horizontalPos.Y, TileManager.CurrentMapTile) &&
//    IsCornerWalkable((int)horizontalPos.X + 32, (int)horizontalPos.Y, TileManager.CurrentMapTile))

//                    {
//                        horizontalMovement = horizontalPos - originalPosition;  // Log horizontal movement
//                        currentFeetPos = horizontalPos; // Update position after horizontal move

//                        // Now try the vertical direction (North/South)
//                        Vector2 verticalPos = currentFeetPos + primaryVertical * moveStep;
//                        if (IsCornerWalkable((int)verticalPos.X, (int)verticalPos.Y, TileManager.CurrentMapTile) &&
//    IsCornerWalkable((int)verticalPos.X - 32, (int)verticalPos.Y - 64 / 3, TileManager.CurrentMapTile) &&
//    IsCornerWalkable((int)verticalPos.X - 32, (int)verticalPos.Y, TileManager.CurrentMapTile) &&
//    IsCornerWalkable((int)verticalPos.X + 32, (int)verticalPos.Y - 64 / 3, TileManager.CurrentMapTile) &&
//    IsCornerWalkable((int)verticalPos.X + 32, (int)verticalPos.Y, TileManager.CurrentMapTile))
//                        {
//                            verticalMovement = verticalPos - currentFeetPos;  // Log vertical movement
//                            return new Vector2[] { originalPosition + horizontalMovement - new Vector2(0, -20), originalPosition + horizontalMovement + verticalMovement }; // Return both movements
//                        }

//                        // If vertical movement fails, continue with the next iteration and increase the horizontal step
//                    }
//                    else
//                    {
//                        // If horizontal movement fails, we stop trying this primary horizontal direction
//                        break;
//                    }
//                }

//                // If primary horizontal movement failed, try the secondary horizontal direction (East/West)


//                for (int i = 2; i <= 500; i++)
//                {
//                    // Calculate the horizontal step for this iteration (moveStep * i)
//                    Vector2 secondaryHorizontalPos = originalPosition + secondaryHorizontal * (moveStep * i);

//                    if (IsCornerWalkable((int)secondaryHorizontalPos.X, (int)secondaryHorizontalPos.Y, TileManager.CurrentMapTile) &&
//    IsCornerWalkable((int)secondaryHorizontalPos.X - 32, (int)secondaryHorizontalPos.Y - 64 / 3, TileManager.CurrentMapTile) &&
//    IsCornerWalkable((int)secondaryHorizontalPos.X - 32, (int)secondaryHorizontalPos.Y, TileManager.CurrentMapTile) &&
//    IsCornerWalkable((int)secondaryHorizontalPos.X + 32, (int)secondaryHorizontalPos.Y - 64 / 3, TileManager.CurrentMapTile) &&
//    IsCornerWalkable((int)secondaryHorizontalPos.X + 32, (int)secondaryHorizontalPos.Y, TileManager.CurrentMapTile))
//                    {
//                        horizontalMovement = secondaryHorizontalPos - originalPosition;  // Log secondary horizontal movement
//                        currentFeetPos = secondaryHorizontalPos; // Update position after secondary horizontal move

//                        // Now try the vertical direction (North/South)
//                        Vector2 secondaryVerticalPos = currentFeetPos + primaryVertical * moveStep;
//                        if (IsCornerWalkable((int)secondaryVerticalPos.X, (int)secondaryVerticalPos.Y, TileManager.CurrentMapTile) &&
//    IsCornerWalkable((int)secondaryVerticalPos.X - 32, (int)secondaryVerticalPos.Y - 64 / 3, TileManager.CurrentMapTile) &&
//    IsCornerWalkable((int)secondaryVerticalPos.X - 32, (int)secondaryVerticalPos.Y, TileManager.CurrentMapTile) &&
//    IsCornerWalkable((int)secondaryVerticalPos.X + 32, (int)secondaryVerticalPos.Y - 64 / 3, TileManager.CurrentMapTile) &&
//    IsCornerWalkable((int)secondaryVerticalPos.X + 32, (int)secondaryVerticalPos.Y, TileManager.CurrentMapTile))
//                        {
//                            verticalMovement = secondaryVerticalPos - currentFeetPos;  // Log vertical movement
//                            return new Vector2[] { originalPosition + horizontalMovement, originalPosition + horizontalMovement + verticalMovement }; // Return both movements
//                        }

//                        // If vertical movement fails, continue with the next iteration and increase the horizontal step
//                    }
//                    else
//                    {
//                        // If secondary horizontal movement fails, we stop trying this secondary horizontal direction
//                        break;
//                    }
//                }

//            }
//            // If all directions are blocked or we reach the border, break out and stop trying
//            Debug.WriteLine("Blocked in all directions or reached the screen border, breaking...");
//            return new Vector2[] { Vector2.Zero, Vector2.Zero };  // Return zero vectors as a failure result
//        }

//        private static bool IsCornerWalkable(int pixelX, int pixelY, MapTile tile)
//        {
//            int cellX = pixelX / MapTile.TileWidth;
//            int cellY = pixelY / MapTile.TileHeight;

//            return IsCellWalkable(cellX, cellY, tile);
//        }

//        private static bool IsCellWalkable(int x, int y, MapTile tile)
//        {
//            if (x < 0 || y < 0 || x >= MapTile.GridWidth || y >= MapTile.GridHeight)
//                return false;

//            return tile.TileGrid[x, y].IsWalkable;
//        }








//    }
//}
