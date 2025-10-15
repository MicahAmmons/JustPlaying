using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.AnimationFolder.EntityCloudFX
{
    public class FXEntityCloud
    {
        public List<FXEntityCloudSpecific> ListOfSpecificEntityClouds { get; set; } = new List<FXEntityCloudSpecific>();


    }
    public class FXEntityCloudSpecific
    {
        public List<Rectangle> MaskFrames = new List<Rectangle>();
        public int MaskRow { get; set; }
        public Vector2 ScrollSpeed { get; set; }
        public Vector4 OverlayColor { get; set; }
        public Texture2D SpriteSheet { get; set; }
        public Texture2D OverlayTexture { get; set; }

        public Rectangle GetMask(int localIdx)
        {
            return MaskFrames[localIdx];
        }
        public Rectangle GetNextFrame(int index)
        {
            return MaskFrames[index];
        }
    }
}
