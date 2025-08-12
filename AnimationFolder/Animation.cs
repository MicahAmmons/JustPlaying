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
        public float EndOfCyclePause { get; private set; }



        public Animation(int frameCount, int frameWidth, int frameHeight, int frameDurationMs, int row, bool isLooping, string textureKey, float endPause)
        {
            Frames = new List<Rectangle>(frameCount);
            SpriteSheet = AssetManager.GetTexture(textureKey);

            // Max numb of frames per sheet width
            int framesPerRow = Math.Max(1, SpriteSheet.Width / frameWidth);

            // 0-based starting row index in the sheet
            int startRowIndex = Math.Max(0, row - 1);

            for (int i = 0; i < frameCount; i++)
            {
                int col = i % framesPerRow;        // 0..framesPerRow-1
                int rowOffset = i / framesPerRow;  // 0,1,2...

                int x = col * frameWidth;
                int y = (startRowIndex + rowOffset) * frameHeight;

                // stop ifoutside the texture
                if (y + frameHeight > SpriteSheet.Height)
                    break; 

                Frames.Add(new Rectangle(x, y, frameWidth, frameHeight));
            }

            Width = frameWidth;
            Height = frameHeight;
            FrameDuration = frameDurationMs / 1000f; // ms -> seconds
            IsLooping = isLooping;
            FrameCount = Frames.Count; // in case we broke early
            EndOfCyclePause = endPause;
        }


        public Animation(Animation other)
        {
            Frames = new List<Rectangle>(other.Frames);
            FrameDuration = other.FrameDuration;
            IsLooping = other.IsLooping;
            SpriteSheet = other.SpriteSheet;
            FrameCount = other.FrameCount;
        }
        public Rectangle GetFrame(int index)
        {
            if (index < 0 || index >= Frames.Count)
                return Frames[0]; // fallback
            return Frames[index];
        }
    }

}
