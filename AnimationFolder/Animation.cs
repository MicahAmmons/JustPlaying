using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Managers.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.AnimationFolder
{
    public class Animation
    {
        public List<Rectangle> Frames { get; private set; }
        public float FrameDuration { get; private set; } // In seconds
        public bool IsLooping { get; private set; }
        public Texture2D SpriteSheet { get; private set; }
        public int FrameCount { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public bool FadeEffect { get; private set; }
        public bool SmokeEffect { get; private set; }
        public Direction DefaultDirection { get; private set; }



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
            IsLooping = data.IsLooping;
            FrameCount = Frames.Count; // in case we broke early
            FadeEffect = data.FadeEffect;
            SmokeEffect = data.SmokeEffect;
        }


        public Animation(Animation other)
        {
            Frames = new List<Rectangle>(other.Frames);
            FrameDuration = other.FrameDuration;
            IsLooping = other.IsLooping;
            SpriteSheet = other.SpriteSheet;
            Width = other.Width;
            Height = other.Height;
            FrameCount = other.FrameCount;
            SpriteSheet = other.SpriteSheet;
        }
        public Rectangle GetFrame(int index)
        {
            if (index < 0 || index >= Frames.Count)
                return Frames[0]; // fallback
            return Frames[index];
        }
    }

}
