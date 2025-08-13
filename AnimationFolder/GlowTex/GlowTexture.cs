using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Managers.Assets;

namespace PlayingAround.AnimationFolder.GlowTex
{
    public class GlowTexture
    {
        public Texture2D Texture { get; set; }
        public Color TargetColor { get; set; }
        public float Duration { get; set; }
        public float StartDelay { get; set; }

        // you can drop these now; they aren’t needed with a global clock
        // private bool _firstCycle = true;
        // private float _timer { get; set; }

        private static Effect _colorReplaceEffect;

        public GlowTexture(GlowTextureData data)
        {
            Texture = AssetManager.GetTexture(data.name);
            TargetColor = ColorPalette.GetColor(data.color);
            Duration = data.speed;
            StartDelay = data.delay;

            // grab the shared effect once
            if (_colorReplaceEffect == null)
                _colorReplaceEffect = AssetManager.GetEffect("ColorReplace");
        }

        // NOTE: now takes the synced globalTime instead of GameTime
        public void DrawGlowTexture(SpriteBatch spriteBatch, float globalTime, Vector2? position = null)
        {
            Vector2 drawPos = position ?? Vector2.Zero;

            // local time starts after the per-texture delay
            float t = globalTime - StartDelay;
            if (t < 0f)
            {
                // before delay hits, draw with zero intensity (no recolor)
                _colorReplaceEffect.Parameters["TargetColor"].SetValue(TargetColor.ToVector4());
                spriteBatch.Draw(Texture, drawPos, null, Color.White * 0f, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                return;
            }

            float dur = Duration <= 0f ? 0.0001f : Duration;

            // ping-pong progress (0→1→0) using the shared time
            float cycles = t / dur;           // how many durations have passed
            float phase = cycles % 2f;        // 0..2
            float progress = phase <= 1f ? phase : 2f - phase; // 0..1..0

            // set per-draw target color on the shared effect
            _colorReplaceEffect.Parameters["TargetColor"].SetValue(TargetColor.ToVector4());

            // use alpha as the recolor “strength” for this pixel (shader uses texture alpha for shape)
            spriteBatch.Draw(
                Texture,
                drawPos,
                null,
                Color.White * progress,
                0f,
                Vector2.Zero,
                1f,
                SpriteEffects.None,
                0f
            );
        }
    }
}
