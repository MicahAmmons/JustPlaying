using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

        public Animation(Texture2D texture, int row, int frameCount, int frameDuration = 1500, bool isLooping = true)
        {
            Frames = new List<Rectangle>();
            int frameWidth = 300;
            int frameHeight = 400;
            int gap = 50;

            for (int i = 0; i < frameCount; i++)
            {
                int x = i * (frameWidth + gap); // 0, 350, 700, ...
                int y = (row - 1) * (frameHeight + gap); // 0, 450, 900, ...
                Frames.Add(new Rectangle(x, y, frameWidth, frameHeight));
            }

            FrameDuration = frameDuration / 1000f; // Convert ms to seconds
            IsLooping = isLooping;
        }
        public Animation(Animation originalAnimation)
        {
            Frames = originalAnimation.Frames;
            FrameDuration = originalAnimation.FrameDuration;
            IsLooping = originalAnimation.IsLooping;
        }



        public int FrameCount => Frames.Count;

        public Rectangle GetFrame(int index)
        {
            if (index < 0 || index >= Frames.Count)
                return Frames[0]; // fallback
            return Frames[index];
        }
    }

}
