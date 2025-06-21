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
            _currentPlayer.AnimationController.Update(gameTime);


        }
        public static void AllowPlayerMovement(bool permission)
        {
            if (!permission) 
            { 
                _currentPlayer.AllowedToMove = false;
                if (_currentPlayer.MovementPath.Count > 0)
                {
                    _currentPlayer.CurrentPos = _currentPlayer.MovementPath[0];
                }
                _currentPlayer.MovementPath.Clear();
                return;
            }
            else if (permission) { _currentPlayer.AllowedToMove = true; }
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
            if (!_currentPlayer.AllowedToMove)  return;
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
            switch (SceneManager.CurrentState)
            {
                case SceneState.Play:
                    DrawPlayer(spriteBatch);
                    break;
                case SceneState.Dialogue:
                    DrawPlayer(spriteBatch);
                    break;

            }
          
        }
        private static void DrawPlayer(SpriteBatch spriteBatch)
        {
            Player player = CurrentPlayer;
            Texture2D texture = player.SpriteSheet;
            Vector2 currentPos = player.CurrentPos;
            Vector2 drawOffset = TileManager.OffSetFromCenterOfDiamond(currentPos, player.DrawSpecifics.Width, player.DrawSpecifics.Height);

           
            Vector2 position = player.CurrentPos;
            Rectangle destination = new Rectangle
             (
                                  (int)drawOffset.X,
                                  (int)drawOffset.Y - (player.DrawSpecifics.Width / 2),
                                       player.DrawSpecifics.Width,
                                       player.DrawSpecifics.Height
            );
            Rectangle source = player.AnimationController.GetCurrentFrame();
            spriteBatch.Draw(texture, destination, source, Color.White);
        }
        
        
        public static PlayerSaveData SavePlayer()
        {
            var data = _playerData;

            _currentPlayer.Save(data);
           
            return data;
        }

        public static void UpdateAllMovement(GameTime gameTime)
        {
            if (!_currentPlayer.AllowedToMove || _currentPlayer == null || _currentPlayer.MovementPath.Count <= 0 || _currentPlayer.MovementPath == null)
                return;
            {
                _currentPlayer.UpdateMovement(gameTime);
            }
        }
    }
}
