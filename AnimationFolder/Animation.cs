using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Managers.Assets;
using PlayingAround.Managers.CombatMan.CombatAttacks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.AnimationFolder
{
    public class Animation
    {
        public List<Rectangle> Frames { get; private set; }
        public float FrameDuration { get; private set; } // In seconds
        public bool IsIndefinite { get; private set; }
        public bool IsLooping { get; private set; } 
        public Texture2D SpriteSheet { get;  set; }
        public int FrameCount { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public bool FadeEffect { get; private set; }
        public bool SmokeEffect { get; private set; }
        public bool PingPong { get; private set; }
        public int EndCyclePause { get; private set; }
        public Direction DefaultDirection { get; private set; }
        public int StartCyclePause { get; private set; }
        public VEDrawLocation? IsDrawPointOverride { get; private set; } = null;
        public bool HasStartingPause { get; private set; } = false;
        public bool RotatesTowardDirection { get; private set; } = false;
        public bool OverrideDiamondDrawPoint { get; private set; } = false;
        public OriginPoint OriginPoint { get; private set; }
        public int YOffset { get; private set; }
        public bool HoldUntilAllFinished { get; private set; }


        public Vector2? DrawPointOverride;
        public Vector2 DestinationPoint;
        public Vector2 AnimationMovementDirection;
        public List<Vector2> OverrideTravelPath = null;



        public Animation(SpecificAnimationData data)
        {
            Frames = new List<Rectangle>(data.FrameCount);
            SpriteSheet = AssetManager.GetTexture(data.SpriteSheetName);
            DefaultDirection = data.DefaultDirection;
            // Max numb of frames per sheet width
            int framesPerRow = Math.Max(1, SpriteSheet.Width / data.FrameWidth);

            // 0-based starting row index in the sheet
            int startRowIndex = Math.Max(0, data.Row - 1);

            for (int i = 0; i < data.FrameCount; i++)
            {
                int col = i % framesPerRow;        // 0..framesPerRow-1
                int rowOffset = i / framesPerRow;  // 0,1,2...

                int x = col * data.FrameWidth;
                int y = (startRowIndex + rowOffset) * data.FrameHeight;

                // stop ifoutside the texture
                if (y + data.FrameHeight > SpriteSheet.Height)
                    break; 

                Frames.Add(new Rectangle(x, y, data.FrameWidth, data.FrameHeight));
            }

            Width = data.FrameWidth;
            Height = data.FrameHeight;
            FrameDuration = data.FrameDurationMs / 1000f; // ms -> seconds
            IsIndefinite = data.IsIndefinite;
            FrameCount = Frames.Count; 
            FadeEffect = data.FadeEffect;
            SmokeEffect = data.SmokeEffect;
            PingPong = data.PingPong;
            EndCyclePause = data.EndCyclePause;
            StartCyclePause = data.StartCyclePause;
            IsLooping = data.IsLooping;
            YOffset = data.YOffset;
            RotatesTowardDirection = data.RotatesTowardsDirection;
            if (RotatesTowardDirection) OverrideDiamondDrawPoint = true;
            HoldUntilAllFinished = data.HoldUntilFinished;
            OriginPoint = data.OriginPoint ?? OriginPoint.TopLeft;
            if (StartCyclePause > 0) { HasStartingPause = true; }
            if (data.IsDrawPointOverride != null) 
            { 
                IsDrawPointOverride = (VEDrawLocation)data.IsDrawPointOverride; 
            }

        }

        public Rectangle GetFrame(int index)
        {
            if (index < 0 || index >= Frames.Count)
                return Frames[Frames.Count - 1]; // fallback
            return Frames[index];
        }
        public void SetDrawPointOverride(Vector2 centerPoint)
        {
           DestinationPoint = centerPoint;
        }
        public void SetDrawPointPathOverride(List<Vector2> path)
        {
            OverrideTravelPath = new List<Vector2>(path);
            AnimationMovementDirection = OverrideTravelPath[OverrideTravelPath.Count - 1] - OverrideTravelPath[0];
        }
        internal void FrameDurationOverride(float movementQuicknessOverride)
        {
           FrameDuration = movementQuicknessOverride;
        }
        internal void ResetOverridePath()
        {
            OverrideTravelPath = null;
        }
        internal float GetRotation()
        {
            if (RotatesTowardDirection)
                return MathF.Atan2(AnimationMovementDirection.Y, AnimationMovementDirection.X);
            else
                return 0f;
        }

        internal Vector2 GetOrigin()
        {
            return GetOriginPoint();
        }
        private Vector2 GetOriginPoint()
        {
            float w = Width;
            float h = Height;
                switch (OriginPoint)
                {
                    case OriginPoint.TopLeft:
                        return new Vector2(0, 0);

                    case OriginPoint.TopMiddle:
                        return new Vector2(w / 2f, 0);

                    case OriginPoint.TopRight:
                        return new Vector2(w, 0);

                    case OriginPoint.MiddleLeft:
                        return new Vector2(0, h / 2f);

                    case OriginPoint.Middle:
                        return new Vector2(w / 2f, h / 2f);

                    case OriginPoint.MiddleRight:
                        return new Vector2(w, h / 2f);

                    case OriginPoint.BottomLeft:
                        return new Vector2(0, h);

                    case OriginPoint.BottomMiddle:
                        return new Vector2(w / 2f, h);

                    case OriginPoint.BottomRight:
                        return new Vector2(w, h);

                    default:
                        return Vector2.Zero;
                }
            }
        

    }

}


public enum OriginPoint
{
    TopLeft,
    TopMiddle,
    TopRight,
    MiddleLeft,
    Middle,
    MiddleRight,
    BottomLeft,
    BottomMiddle,
    BottomRight
}