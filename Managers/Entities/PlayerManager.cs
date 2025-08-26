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
using System.Collections.Generic;
using System.Threading;

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
        public static void Update(GameTime gameTime, float delta)
        {
            if (SceneManager.CurrentState == SceneState.TitleScreen) return;
            _currentPlayer.Update(gameTime, delta);
            UpdatePlayerInput(gameTime);
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
                if (InputManager.IsRightMouseDown())
                {
                    Vector2 target = new Vector2(InputManager.MouseX , InputManager.MouseY);
                    _currentPlayer.UpdatePlayerDestinationPoint(target);
                }
                if (InputManager.IsKeyPressed(Keys.Space))
                {
                    _currentPlayer.MovementController.ClearMovementPath();
                }
        }
        public static PlayerSaveData SavePlayer()
        {
            var data = _playerData;

            _currentPlayer.Save(data);
           
            return data;
        }
        public static void ClearAllPlayerAspects()
        {
            _currentPlayer.ClearAllAspects();
        }
        public static AnimationData GetIdleAnimationData()
        {
            return _playerData.AnimationData;
        }
        public static void DrawPlayer(SpriteBatch spriteBatch, Effect fx = null)
        {
            if (SceneManager.CurrentState == SceneState.Play || SceneManager.CurrentState == SceneState.Dialogue || SceneManager.IsState(SceneState.Combat))
            {
                _currentPlayer?.DrawTexture(spriteBatch, fx);
            }
        }
    }
}
