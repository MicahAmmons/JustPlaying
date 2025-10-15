using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.AnimationFolder.EntityCloudFX
{
    public class FXEntityCloudData
    {
        public int MaskRow { get; set; }
        public string OverlayTextureName { get; set; }
        public ColorData OverlayColor { get; set; }
        public Vec2Data ScrollSpeed { get; set; }

    }
}
public class ColorData
{
    public float R { get; set; }
    public float G { get; set; }
    public float B { get; set; }
    public float A { get; set; }

    public Vector4 ToVector4() => new Vector4(R, G, B, A);
}
public struct Vec2Data
{
    public float X { get; set; }
    public float Y { get; set; }
    public Vector2 ToXna() => new Vector2(X, Y);
}