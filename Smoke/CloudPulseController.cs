using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Managers.Assets;
using System;

namespace PlayingAround.Smoke
{
    public class CloudPulseController
    {
        public float Duration;
        public float Timer;
        public int Height;
        public int Width;
        public Vector2 Destination;
        public Color Color;
        public Texture2D Text;

        public CloudPulseController()
        {
            Text = AssetManager.GetTexture("BGCloud_0_0_1");

        }
        public void Draw(SpriteBatch sb)
        {
            if (Text == null || Duration <= 0f || Timer <= 0f) return;

            float total = Duration;                 // total lifetime
            float elapsed = total - MathF.Max(0f, Timer); // seconds since spawn
            if (elapsed < 0f || elapsed > total) return;

            // Alpha ramps 0→1 over first half, then 1→0 over second half
            float half = total * 0.5f;
            float alpha = (elapsed <= half)
                ? (elapsed / half)                  // fade in
                : ((total - elapsed) / half);       // fade out

            // Scale to desired Width/Height (fallback to 1:1 if not set)
            float sx = (Width > 0) ? Width / (float)Text.Width : 1f;
            float sy = (Height > 0) ? Height / (float)Text.Height : 1f;

            var origin = new Vector2(Text.Width * 0.5f, Text.Height * 0.5f);

            sb.Draw(
                Text,
                Destination,            // center on this point
                sourceRectangle: null,
                color: Color * alpha,   // fade by alpha
                rotation: 0f,
                origin: origin,         // draw from texture center
                scale: new Vector2(sx, sy),
                effects: SpriteEffects.None,
                layerDepth: 0f
            );
        }


    }
}
