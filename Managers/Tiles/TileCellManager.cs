using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Game.Map;
using System;
using System.Collections.Generic;

namespace PlayingAround.Managers.Tiles
{ 
    public static class TileCellManager
    {
        private static List<TileCell> _activeVisualEffectCells = new List<TileCell>();

        public static void Update(float delta)
        {
            UpdateCellVisualEffects(delta);
        }
        public static void Draw(SpriteBatch spriteBatch)
        {
            foreach (var cell in _activeVisualEffectCells)
            {
                cell.VEManager.Draw(spriteBatch);
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