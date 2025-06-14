using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PlayingAround.Data.SaveData;
using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Entities.Player;
using PlayingAround.Game.Map;
using PlayingAround.Manager;
using PlayingAround.Managers.Resistances;
using PlayingAround.Managers.Tiles;

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
            ResistanceManager.GetPlayerResistances(_currentPlayer);
        }
        public static void Update(GameTime gameTime)
        {
            deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _currentPlayer.Update(gameTime);
            UpdatePlayerInput(gameTime);


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
        public static void UpdatePlayerStatsFromCombat(CombatMonster playerMonster)
        {
            _currentPlayer.stats.CurrentHealth = playerMonster.CurrentHealth;
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
            _currentPlayer?.Draw(spriteBatch);
        }

        
        public static PlayerSaveData SavePlayer()
        {
            var data = _playerData;

            _currentPlayer.Save(data);
            data.PlayerSummons = _currentPlayer.SavePlayerSummons();
            return data;
        }


    }
}
