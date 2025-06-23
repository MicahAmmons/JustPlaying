using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PlayingAround.AnimationFolder;
using PlayingAround.Data.SaveData;
using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Entities.Player;
using PlayingAround.Game.Map;
using PlayingAround.Manager;
using PlayingAround.Managers.Resistances;
using PlayingAround.Managers.Tiles;
using PlayingAround.Utils;
using System;

namespace PlayingAround.Managers.Entities
{
    public static class PlayerManager
    {

        private static Player _currentPlayer;
        public static Player CurrentPlayer => _currentPlayer;
        public static PlayerSaveData _playerData;
        public static void LoadContent(PlayerSaveData data)
        {
            _playerData = data;
            _currentPlayer = Player.LoadFromSave(data);
        }
        public static void Update(GameTime gameTime)
        {
            if (SceneManager.CurrentState == SceneState.TitleScreen) return;
            _currentPlayer.Update(gameTime);
            UpdatePlayerInput(gameTime);
        }
        public static void AllowPlayerMovement(bool permission)
        {
            if (!permission) 
            { 
                _currentPlayer.DrawSpecifics.AllowedToMove = false;
                if (_currentPlayer.CurrentStats.MovePath.Count > 0)
                {
                    _currentPlayer.CurrentPos = _currentPlayer.CurrentStats.MovePath[0];
                }
                _currentPlayer.CurrentStats.MovePath.Clear();
                return;
            }
            else if (permission) { _currentPlayer.DrawSpecifics.AllowedToMove = true; }
        }
        public static void UpdatePlayerInput(GameTime gameTime)
        {

             switch (SceneManager.CurrentState)
            {
                case SceneState.Play:
                    MovePlayerInput(gameTime);
                    break;
            }

        }
        public static void MovePlayerInput(GameTime gameTime)
        {
            if (!_currentPlayer.DrawSpecifics.AllowedToMove)  return;
            {
                if (InputManager.IsRightMouseDown())
                {
                    Vector2 target = new Vector2(InputManager.MouseX , InputManager.MouseY);
                    TileCell cell = TileManager.GetCell(target);

                        _currentPlayer.UpdatePlayerEndPoint(target);

      
                }
                if (InputManager.IsKeyPressed(Keys.Space))
                {
                    _currentPlayer.ClearMovementPath();
                }
            }
            
        }
        public static void Draw(SpriteBatch spriteBatch)
        {
            if (SceneManager.CurrentState == SceneState.Play || SceneManager.CurrentState == SceneState.Dialogue || SceneManager.CurrentState == SceneState.Combat)
            {
                _currentPlayer?.Draw(spriteBatch);
            }
        }

        
        
        public static PlayerSaveData SavePlayer()
        {
            var data = _playerData;

            _currentPlayer.Save(data);
           
            return data;
        }

        public static void UpdateAllMovement(GameTime gameTime)
        {
            if (!_currentPlayer.DrawSpecifics.AllowedToMove || _currentPlayer == null || _currentPlayer.CurrentStats.MovePath.Count <= 0 || _currentPlayer.CurrentStats.MovePath == null)
                return;
            {
                _currentPlayer.UpdateMovement(gameTime);
            }
        }
    }
}
