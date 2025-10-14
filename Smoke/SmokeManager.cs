using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Managers;
using PlayingAround.Managers.Assets;
using PlayingAround.Managers.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace PlayingAround.Smoke
{
    public static class SmokeManager
    {
        private static SmokeTexture _smokeInfo => TileManager.CurrentMapTile.BackgroundSmokeTexture;
        private static Texture2D _singleCloud;
        private static readonly List<CloudPulseController> _pulseClouds = new List<CloudPulseController>();
        private static readonly Random _rng = new Random();


        private static readonly int[] _sizeOptions = {  150, 200, 250, 300, 350, 450, 550 }; 
        private static readonly Color[] _cloudPalette = new[]
        {
            ColorPalette.Acid,
            ColorPalette.Fire,
            ColorPalette.Earth,
        };
        public static void LoadContent()
        {
            _singleCloud = AssetManager.GetTexture("BGCloud_0_0_1");
        }
        public static void Update(GameTime gameTime)
        {
            if (!SceneManager.IsState(SceneState.Play)) return;

            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            UpdateTimers(delta);
            AddNewPulseCloud();
        }

        private static void UpdateTimers(float delta)
        {
            for (int i = _pulseClouds.Count - 1; i >= 0; i--)
            {
                var cloud = _pulseClouds[i];
                if (cloud == null)
                {
                    _pulseClouds.RemoveAt(i);
                    continue;
                }

                cloud.Timer -= delta;
                if (cloud.Timer <= 0)
                    _pulseClouds.RemoveAt(i);
            }

            foreach (var text in _smokeInfo.FadingBaseTextures)
            {
                float currentTimer = text.CurrentTimer;
                float max = text.FadeDuration;
                if (max > 0f)
                {
                    text.CurrentTimer += text.FadeDirection * delta;

                    if (text.CurrentTimer >= max)
                    {
                        text.CurrentTimer = max;
                        text.FadeDirection = -1;
                    }
                    else if (text.CurrentTimer <= 0f)
                    {
                        text.CurrentTimer = 0f;
                        text.FadeDirection = 1;
                    }
                }
            }
          

        }

        private static void AddNewPulseCloud()
        {
            if (!RoomForMore()) return;

            // pick random size
            int w = _sizeOptions[_rng.Next(_sizeOptions.Length)];
            int h = _sizeOptions[_rng.Next(_sizeOptions.Length)];

            // pick random color (you can weight/bias this if you want)
            Color color = _cloudPalette[_rng.Next(_cloudPalette.Length)];

            // try to find a valid spawn position with constraints
            if (TryFindSpawn(w, h, color, out Vector2 dest))
            {
                // You can randomize Duration/Timer if you want; here’s a fixed example:
                float duration = MathHelper.Lerp(2f, 10f, (float)_rng.NextDouble());


                _pulseClouds.Add(new CloudPulseController
                {
                    Color = color,
                    Destination = dest,
                    Width = w,
                    Height = h,
                    Duration = duration,
                    Timer = duration, // countdown handled in UpdateTimers
                    Text = _singleCloud // optional; controller sets a default too
                });
            }
            // else: no space available this frame — silently skip; can try again next Update
        }
        private static bool TryFindSpawn(int width, int height, Color color, out Vector2 center)
        {
            // Use your own viewport provider if you have one (e.g., ViewPortManager)
            int viewW = ViewportManager.ScreenWidth;   // assumes you have this in your project
            int viewH = ViewportManager.ScreenHeight;

            // If your clouds are drawn from center (as in your Draw), keep them fully on-screen:
            int halfW = width / 2;
            int halfH = height / 2;

            // Safety: avoid degenerate ranges.
            if (viewW <= width || viewH <= height)
            {
                // Fallback: allow edge-clamping spawn at center of the screen
                center = new Vector2(viewW * 0.5f, viewH * 0.5f);
                return IsValid(center, width, height, color);
            }

            const int MaxAttempts = 32;
            for (int attempt = 0; attempt < MaxAttempts; attempt++)
            {
                float x = _rng.Next(halfW, viewW - halfW);
                float y = _rng.Next(halfH, viewH - halfH);
                var candidate = new Vector2(x, y);

                if (IsValid(candidate, width, height, color))
                {
                    center = candidate;
                    return true;
                }
            }

            center = default;
            return false;
        }
        private static bool IsValid(Vector2 center, int width, int height, Color color)
        {
            Rectangle rect = CenteredRect(center, width, height);

            foreach (var c in _pulseClouds)
            {
                if (c == null) continue;

                // 1) No rectangle overlap
                if (rect.Intersects(CenteredRect(c.Destination, c.Width, c.Height)))
                    return false;

                // 2) Not within 100x100 of any cloud
                float dx = Math.Abs(center.X - c.Destination.X);
                float dy = Math.Abs(center.Y - c.Destination.Y);
                if (dx < 100f && dy < 100f)
                    return false;

                // 3) If same color, also enforce 100x100 spacing (redundant but explicit)
                if (c.Color == color && dx < 100f && dy < 100f)
                    return false;
            }

            return true;
        }
        private static Rectangle CenteredRect(Vector2 center, int width, int height)
        {
            int x = (int)MathF.Round(center.X - width * 0.5f);
            int y = (int)MathF.Round(center.Y - height * 0.5f);
            return new Rectangle(x, y, width, height);
        }
        private static bool RoomForMore()
        {
            if (_pulseClouds.Count <=  10) return true;
            return false;
        }
        public static void DrawPulseCloud(SpriteBatch sb)
        {
            foreach (var cloud in _pulseClouds)
                cloud?.Draw(sb);
        }

        internal static void DrawBackgroundSmoke(SpriteBatch spriteBatch, Effect fx)
        {
            var e = _smokeInfo;
            if (e.Opacity == null || e.Opacity == 0) return;
            fx.Parameters["Frequency"].SetValue(e.FrequencyVec);
            fx.Parameters["Speed"].SetValue(e.SpeedVec);
            fx.Parameters["DistortAmount"].SetValue(e.DistortAmount);
            fx.Parameters["Opacity"].SetValue(e.Opacity);
            var tex = e.SmokeFXTexture;

            var vp = spriteBatch.GraphicsDevice.Viewport;
            Vector2 screenCenter = new Vector2(vp.Width * 0.5f, vp.Height * 0.5f);
            Vector2 origin = new Vector2(tex.Width * 0.5f, tex.Height * 0.5f);

            spriteBatch.Draw(
    tex,
    position: screenCenter,
    sourceRectangle: null,
    color: Color.White,
    rotation: 0f,
    origin: origin,
    scale: 1f,
    effects: SpriteEffects.None,
    layerDepth: 0f
);
        }

        public static void DrawFadingBase(SpriteBatch spriteBatch)
        {
            var e = _smokeInfo;
            if (e == null) return;
            foreach (var t in e.FadingBaseTextures)
            {
                Texture2D text = t.Texture;
                if (text == null) return;

                float alpha = 1f;

                if (t.FadeDuration > 0f)
                {
                    float f = t.CurrentTimer / t.FadeDuration;
                    alpha = 1f - MathHelper.Clamp(f, 0f, 1f);
                }
                var width = ViewportManager.ScreenWidth;
                var height = ViewportManager.ScreenHeight;  
                Vector2 screenCenter = new Vector2(width * 0.5f, height * 0.5f);
                Vector2 origin = new Vector2(text.Width * 0.5f, text.Height * 0.5f);
                // Draw centered so the oversized texture overhangs evenly on all sides.
                spriteBatch.Draw(
                    text,
                    position: screenCenter,
                    sourceRectangle: null,
                    color: Color.White * alpha,
                    rotation: 0f,
                    origin: origin,
                    scale: 1f,
                    effects: SpriteEffects.None,
                    layerDepth: 0f
                );
            }
        }

        public static void DrawStaticBase(SpriteBatch sb)
        {
            var e = _smokeInfo;
            foreach (var text in e.StaticBaseTextures)
            {
                sb.Draw(text, Vector2.Zero, Color.White);
            }
        }
    }
}
