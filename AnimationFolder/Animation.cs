using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.AnimationFolder.EntityCloudFX;
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
        public FXEntityCloud FXEntityCloud { get; private set; }
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
            Width = data.FrameWidth;
            Height = data.FrameHeight;
            FrameDuration = data.FrameDurationMs / 1000f; // ms -> seconds
            IsIndefinite = data.IsIndefinite;
            FadeEffect = data.FadeEffect;
            SmokeEffect = data.SmokeEffect;
            PingPong = data.IsPingPong;
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
            Frames = new List<Rectangle>();
            var frames = new List<Rectangle>(data.FrameCount);
            SpriteSheet = AssetManager.GetTexture(data.SpriteSheetName);
            DefaultDirection = data.DefaultDirection;
            if (data.FXEntityCloudData != null)
            {
                FXEntityCloud = new FXEntityCloud();
                foreach (var d in data.FXEntityCloudData)
                {
                    var c = d.OverlayColor;
                    FXEntityCloud.ListOfSpecificEntityClouds.Add(new FXEntityCloudSpecific()
                    {
                        ScrollSpeed = new Vector2(d.ScrollSpeed.X, d.ScrollSpeed.Y),
                        SpriteSheet = SpriteSheet,
                        MaskRow = d.MaskRow,
                        OverlayTexture = AssetManager.GetTexture(d.OverlayTextureName),

                        OverlayColor = new Vector4(c.R, c.G, c.B, c.A)
                    });

                }
               
            }


            int framesPerRow = Math.Max(1, SpriteSheet.Width / data.FrameWidth);
            int startRowIndex = Math.Max(0, data.Row - 1);


            for (int i = 0; i < data.FrameCount; i++)
            {
                int col = i % framesPerRow;
                int rowOffset = i / framesPerRow;

                int x = col * data.FrameWidth;
                int y = (startRowIndex + rowOffset) * data.FrameHeight;

                if (y + data.FrameHeight > SpriteSheet.Height)
                    break;


                // main overlay frame
                frames.Add(new Rectangle(x, y, data.FrameWidth, data.FrameHeight));

                // per-FX mask frames
                if (FXEntityCloud?.ListOfSpecificEntityClouds != null)
                {
                    foreach (var fxSpec in FXEntityCloud.ListOfSpecificEntityClouds)
                    {
                        int maskBaseRow = (fxSpec.MaskRow > 0) ? fxSpec.MaskRow : data.Row;
                        int maskStartRowIndex = Math.Max(0, maskBaseRow - 1);
                        int maskY = (maskStartRowIndex + rowOffset) * data.FrameHeight;

                        if (maskY + data.FrameHeight <= SpriteSheet.Height)
                        {
                            fxSpec.MaskFrames.Add(new Rectangle(x, maskY, data.FrameWidth, data.FrameHeight));
                        }
                    }
                }
            }
            Frames.AddRange(FinalizeRectangleList(frames));
            FrameCount = Frames.Count;
            //PingPong
            //EndDelay
            //StartDelay



        }

        private List<Rectangle> FinalizeRectangleList(List<Rectangle> frames)
        {
            if (EndCyclePause > 0)
            {
                for (int i = 0; i < EndCyclePause ; i++)
                {
                    var dupedFrame = frames[frames.Count - 1];
                    var frame = new Rectangle()
                    {
                        X = dupedFrame.X,
                        Y = dupedFrame.Y,
                        Width = dupedFrame.Width,
                        Height = dupedFrame.Height
                    };
                    frames.Add(frame);
                }
            }
            if (PingPong)
            {
                for (int i = frames.Count - 1; i >= 0; i--)
                {
                    var currentFrame = frames[i];
                    var newFrame = new Rectangle()
                    {
                        X = currentFrame.X,
                        Y = currentFrame.Y,
                        Width = currentFrame.Width,
                        Height = currentFrame.Height
                    };
                    frames.Add(newFrame);
                }
            }
            if (StartCyclePause > 0)
            {
                for (int i = 0; i < StartCyclePause; i++)
                {
                    Frames.Add(new Rectangle());
                }
            }
            return frames;
        }

        public Rectangle GetFrame(int index)
        {
            if (index < 0 || index >= Frames.Count)
                return Frames[Frames.Count - 1]; // fallback
            return Frames[index];
        }
        public void SetDrawPointOverride(Vector2 centerPoint)
        {
           DrawPointOverride = centerPoint;
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