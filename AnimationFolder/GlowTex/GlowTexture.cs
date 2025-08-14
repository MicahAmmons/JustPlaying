using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Managers.Assets;
using System;

namespace PlayingAround.AnimationFolder.GlowTex
{
    public class GlowTexture
    {
        public Texture2D Texture { get; set; }
        public Color TargetColor { get; set; }
        public float Duration { get; set; }
        public float StartDelay { get; set; }
        public float MinFade { get; set; } = 0f;
        public float MaxFade { get; set; } = 1f;

        private static Effect _colorReplaceEffect;

        public GlowTexture(GlowTextureData data)
        {
            Texture = AssetManager.GetTexture(data.name);
            TargetColor = ColorPalette.GetColor(data.color);
            Duration = data.speed;
            StartDelay = data.delay;

            MinFade = MathHelper.Clamp(data.minFade, 0f, 1f);
            MaxFade = MathHelper.Clamp(data.maxFade, 0f, 1f);
            if (MaxFade < MinFade) { var t = MinFade; MinFade = MaxFade; MaxFade = t; }

            if (_colorReplaceEffect == null)
                _colorReplaceEffect = AssetManager.GetEffect("ColorReplace");
        }

        // globalTime comes from controller
        public void DrawGlowTexture(SpriteBatch spriteBatch, float globalTime, Vector2? position = null)
        {
            Vector2 drawPos = position ?? Vector2.Zero;

            float t = globalTime - StartDelay;
            if (t < 0f)
            {
                // before delay: no recolor yet
                var fx0 = AssetManager.GetEffect("ColorReplace");
                fx0.Parameters["TargetColor"].SetValue(TargetColor.ToVector4());
                fx0.Parameters["Strength"].SetValue(0f);
                spriteBatch.Draw(Texture, drawPos, Color.White); // no alpha fade
                return;
            }

            float dur = Duration <= 0f ? 0.0001f : Duration;
            float cycles = t / dur;
            float phase = cycles % 2f;                 // 0..2
            float progress = phase <= 1f ? phase : 2f - phase;

            // Ease in/out using cosine smoothstep
            progress = 0.5f - 0.5f * (float)Math.Cos(progress * MathF.PI);


            // If you use Min/Max fade bounds:
            // float strength = MathHelper.Lerp(MinFade, MaxFade, progress);
            float strength = progress; // pure 0..1..0 if no min/max

            var fx = AssetManager.GetEffect("ColorReplace");
            fx.Parameters["TargetColor"].SetValue(TargetColor.ToVector4());
            fx.Parameters["Strength"].SetValue(strength);

            // IMPORTANT: draw fully opaque (no * progress)
            spriteBatch.Draw(Texture, drawPos, Color.White);
        }

    }
}
