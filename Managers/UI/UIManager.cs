using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PlayingAround.Data;
using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Entities.Monster.PlayMonsters;
using PlayingAround.Entities.Player;
using PlayingAround.Manager;
using PlayingAround.Managers.Assets;
using PlayingAround.Managers.CombatMan;
using PlayingAround.Managers.Proximity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Managers.UI
{

    public static class UIManager
    {
        private static SpriteFont _mainFont;
        private static bool _fightBox = false;
        private static PlayMonsters _playMonsters;
        private static Texture2D _fightBackground;
        private static Player _player;
        private static CombatMonster _playerMonster;
        private static CombatMonster _standInMonster;


        private static string _playerStats;
            
        public static void LoadContent(Player player)
        {
            _mainFont = AssetManager.GetFont("mainFont");
            _fightBackground = AssetManager.GetTexture("fightBackground");
            ProximityManager.OnPlayerNearMonster += HandleFightPrompt;
            ProximityManager.OnPlayerLeaveMonster += HandlePlayerExit;
            _player = player;


        }

        public static void Update(GameTime _gameTime)
        {
            UpdatePlayer();
        }
        public static void HandlePlayerExit()
        {
            _fightBox = false;
            _playMonsters = null;
        }
        public static void HandleFightPrompt(PlayMonsters mon)
        {
            _fightBox = true;
            _playMonsters = mon;
        }
        public static void Draw(SpriteBatch _spriteBatch, GraphicsDevice graphicsDevice)
        { 
            if (_fightBox && SceneManager.CurrentState == SceneManager.SceneState.Play)
            {
                _spriteBatch.Draw(_fightBackground, new Vector2(960,15), Color.Black);
                if (InputManager.IsKeyPressed(Keys.Enter))
                {
                    SceneManager.SetState(SceneManager.SceneState.Combat);
                    CombatManager.BeginCombat(_playMonsters, _player);
                }
            }
            DrawPlayerStatsUI(_spriteBatch);
           


        }
        private static void DrawPlayerStatsUI(SpriteBatch _spriteBatch)
        {
            if (_playerStats != null )
            {
                Rectangle backgroundRect = new Rectangle(1600, 880, 200, 100); // width/height based on how much space you need
                _spriteBatch.Draw(_fightBackground, backgroundRect, Color.Aqua);
                Vector2 textSize = _mainFont.MeasureString(_playerStats);

                // Center the text within the backgroundRect
                Vector2 textPosition = new Vector2(
                    backgroundRect.X + (backgroundRect.Width - textSize.X) / 2,
                    backgroundRect.Y + (backgroundRect.Height - textSize.Y) / 2
                );

                _spriteBatch.DrawString(_mainFont, _playerStats, textPosition, Color.White);
            }
        }

        private static void UpdatePlayer()
        {
            if (SceneManager.CurrentState == SceneManager.SceneState.Play)
            {
                _playerStats = $"Health: {_player.stats.CurrentHealth} / {_player.stats.CurrentHealth}\n" +
                               $"Mana: {_player.stats.CurrentMana} / {_player.stats.CurrentMana}";
            }
            if (SceneManager.CurrentState == SceneManager.SceneState.Combat)
            {
                _playerMonster = CombatManager.GetPlayerMonster();
                _standInMonster = CombatManager.GetStandInMonster();
                _playerStats = $"Health: {_playerMonster.CurrentHealth} / {_playerMonster.MaxHealth}\n" +
                               $"Mana: {_playerMonster.CurrentMana} / {_playerMonster.CurrentMana}\n" +
                               $"Speed: {_standInMonster.Speed}";
            }
        }
    }
}
