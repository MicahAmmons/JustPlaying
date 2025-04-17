using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
        public static void LoadContent(Player player)
        {
            _mainFont = AssetManager.GetFont("mainFont");
            _fightBackground = AssetManager.GetTexture("fightBackground");
            ProximityManager.OnPlayerNearMonster += HandleFightPrompt;
            ProximityManager.OnPlayerLeaveMonster += HandlePlayerExit;
            _player = player;
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
                    CombatManager.BeginCombat(_playMonsters, _player);
                }
            }
        }
    }
}
