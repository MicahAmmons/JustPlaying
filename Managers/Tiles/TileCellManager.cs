using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.AnimationFolder;
using PlayingAround.Game.Map;
using System;
using System.Collections.Generic;

namespace PlayingAround.Managers.Tiles
{ 
    public static class TileCellManager
    {
        private static List<TileCell> _activeVisualEffectCells = new List<TileCell>();
        private static List<TileCell> _activeAnimation = new List<TileCell>();
        public static void Update(GameTime gameTime)
        {
            UpdateActiveAnimation(gameTime);
        }
        public static void UpdateActiveAnimation(GameTime gameTime)
        {
            if (_activeAnimation.Count == 0) return;
            _activeAnimation.RemoveAll(c => c.AnimationManager == null);

            // Update remaining
            foreach (var cell in _activeAnimation)
                cell.Update(gameTime);

            // Drop anything that died during Update
            _activeAnimation.RemoveAll(c => c.AnimationManager == null);
        }
        public static void AddActiveAnimationCell(TileCell cell)
        {
            if (!_activeAnimation.Contains(cell))
            _activeAnimation.Add(cell);
        }
        public static void Draw(SpriteBatch spriteBatch)
        {
            DrawCellAnimations(spriteBatch);
        }
        public static void DrawCellAnimations(SpriteBatch spriteBatch)
        {
            if (_activeAnimation.Count <= 0) return;
            foreach (var cell in _activeAnimation)
            {
                cell.DrawAnimation(spriteBatch);
            }
        }
        public static void UpdateCellVisualEffects(float delta)
        {
            foreach (var cell in _activeVisualEffectCells)
            {
                cell.VEManager.Update(delta);
                if (!cell.VEManager.HasActiveEffects)
                {
                    _activeVisualEffectCells.Remove(cell);
                }
            }
        }
        public static void AddCellVE(TileCell tileCell)
        {
            _activeVisualEffectCells.Add(tileCell);
        }
    }

}