using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Data.SaveData;
using PlayingAround.Entities.Player;
using PlayingAround.Managers.Assets;
using System.Collections.Generic;

namespace PlayingAround.Managers
{
    public static class PlayerManager
    {
        private static Player _currentPlayer;
        public static Player CurrentPlayer => _currentPlayer;

        public static void LoadFromSave(PlayerSaveData data)
        {
            _currentPlayer = Player.LoadFromSave(data);
        }

        public static void LoadDefault()
        {
            var texture = AssetManager.GetTexture("Hero_Blonde");
            _currentPlayer = new Player(texture, new Vector2(100, 100), new List<SummonsSaveData>(), 200f);
        }

        public static void Update(GameTime gameTime)
        {
            _currentPlayer?.Update(gameTime);
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            _currentPlayer?.Draw(spriteBatch);
        }

        public static void DrawDebug(SpriteBatch spriteBatch, Texture2D debugPixel)
        {
            _currentPlayer?.DrawDebugPath(spriteBatch, debugPixel);
        }

        public static Vector2 GetFeetCenter()
        {
            return _currentPlayer?.GetFeetCenter() ?? Vector2.Zero;
        }

        public static PlayerSaveData Save()
        {
            return _currentPlayer?.Save();
        }

        public static void SetNewTilePosition()
        {
            _currentPlayer?.SetNewTilePosition();
        }
    }
}
