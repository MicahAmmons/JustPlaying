using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PlayingAround.Data.SaveData;
using PlayingAround.Entities.Player;
using PlayingAround.Game.Map;
using PlayingAround.Game.Pathfinding;
using PlayingAround.Manager;
using PlayingAround.Managers.Assets;
using System.Collections.Generic;

namespace PlayingAround.Managers
{
    public static class PlayerManager
    {
        private static Player _currentPlayer;
        public static Player CurrentPlayer => _currentPlayer;
        public static PlayerSaveData _playerData;
        public static float deltaTime;


        public static void LoadContent(PlayerSaveData data)
        {
            _playerData = data;
            _currentPlayer = Player.LoadFromSave(data);
        }

        public static void Update(GameTime gameTime)
        {
            deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _currentPlayer.Update(gameTime);
            UpdatePlayerInput(gameTime);

        }

        public static void UpdatePlayerInput(GameTime gameTime)
        {

             switch (SceneManager.CurrentState)
            {
                case SceneManager.SceneState.Play:
                    MovePlayerInput(gameTime);
                    break;
            }

        }

        public static void MovePlayerInput(GameTime gameTime)
        {
            if (!_currentPlayer.AllowedToMove)  return;
            {
                if (InputManager.IsRightClick())
                {
                    Vector2 target = new Vector2(InputManager.MouseX , InputManager.MouseY);
                    TileCell cell = TileManager.GetCell(target);
                    if (TileManager.IsCellWalkable(cell.X,cell.Y))
                    {
                        _currentPlayer.UpdatePlayerEndPoint(target);
                    }
      
                }
                if (InputManager.IsKeyPressed(Keys.Space))
                {
                    _currentPlayer.ClearMovementPath();
                }
            }
            
        }
        public static void Draw(SpriteBatch spriteBatch)
        {
            _currentPlayer?.Draw(spriteBatch);
        }

        public static void DrawDebug(SpriteBatch spriteBatch, Texture2D debugPixel)
        {
            _currentPlayer?.DrawDebugPath(spriteBatch, debugPixel);
        }


        public static PlayerSaveData Save()
        {
            return _currentPlayer.Save();
        }


    }
}
