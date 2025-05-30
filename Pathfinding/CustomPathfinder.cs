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
        private static float bufferDistance = 5f;
        public static List<Vector2> BuildPixelPath(Rectangle start, Vector2? endd)
        {
            List<Vector2> path = new();
            Vector2 end = (Vector2)endd;
            float moveStep = start.Height;
            float closeEnough = (start.Height / 2f) + 1;

            Vector2 feetBoxCenter = new Vector2(start.Left + start.Width / 2f, start.Bottom);
            Vector2 offset = new Vector2(-start.Width / 2f, -start.Height * 3);

            Rectangle currentRect = start;
            Rectangle previousRect = start;
            Vector2 previousCenter = feetBoxCenter;

            int maxSteps = 500;
            int stepCount = 0;

            while (Vector2.Distance(feetBoxCenter, end) > closeEnough && stepCount++ < maxSteps)
            {
                Vector2 direction = end - feetBoxCenter;
                if (direction.Length() > 0)
                    direction.Normalize();

                Vector2 moveVector = direction * moveStep;

                previousRect = currentRect;
                previousCenter = feetBoxCenter;

                currentRect = new Rectangle(
                    (int)(currentRect.X + moveVector.X),
                    (int)(currentRect.Y + moveVector.Y),
                    currentRect.Width,
                    currentRect.Height
                );

                feetBoxCenter += moveVector;

                if (TileManager.IsCellWalkable(currentRect))
                {
                    path.Add(feetBoxCenter);
                }
                else
                {
                    // Try smaller steps backward until one fits
                    for (int i = 1; i <= moveStep; i++)
                    {
                        Vector2 trialVector = direction * (moveStep - i);
                        Rectangle trialRect = new Rectangle(
                            (int)(previousRect.X + trialVector.X),
                            (int)(previousRect.Y + trialVector.Y),
                            previousRect.Width,
                            previousRect.Height
                        );

                        Vector2 trialCenter = previousCenter + trialVector;

                        if (TileManager.IsCellWalkable(trialRect))
                        {
                            path.Add(trialCenter);
                            break;
                        }
                    }

                    break; // stop stepping forward
                }
            }

            // If final point is close enough, snap to it
            if (Vector2.Distance(feetBoxCenter, end) <= closeEnough)
            {
                path.Add(end);
            }

            // Offset all path points to match where cursor is
            for (int i = 0; i < path.Count; i++)
            {
                path[i] += offset;
            }

            return path;
        }

 



    }
}
