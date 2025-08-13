using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Managers.Tiles;
using System.Collections.Generic;

namespace PlayingAround.AnimationFolder.GlowTex
{
    public static class GlowTextureController
    {
        private static float _globalTime; // shared clock for all glow textures
        private static List<GlowTexture> _glowTextures => TileManager.CurrentMapTile.BehindGlowTextures;

        public static void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            // advance the shared time once per frame
            _globalTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            // draw each glow with the same synced time
            foreach (var glow in _glowTextures)
            {
                glow.DrawGlowTexture(spriteBatch, _globalTime);
            }
        }

        // optional helpers if you ever need them:
        public static void ResetTime() => _globalTime = 0f;
        public static float GetTime() => _globalTime;
    }
}
