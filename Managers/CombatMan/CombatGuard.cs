using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Entities.Monster.PlayMonsters;
using PlayingAround.Entities.Player;
using PlayingAround.Managers.Entities;
using PlayingAround.Managers.Quests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Managers.CombatMan
{
     public static class CombatGuard
    {
        private static CombatManager _currentCombat { get; set; }
        public static CombatManager CurrentCombat => _currentCombat;
        private static CombatManager _previousCombat {  get; set; }

        public static void CreateNewCombat(PlayMonsters playMonsters)
        {

            _currentCombat = new CombatManager(playMonsters);

        }

        public static void Update(GameTime gameTime)
        {
            if (_currentCombat != null) 
            {
                _currentCombat.Update(gameTime);
            }
        }
        public static void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            if (SceneManager.CurrentState != SceneState.Combat) return;
            if (_currentCombat != null)
            {
                _currentCombat.Draw(spriteBatch, graphicsDevice);
            }
        }
        public static void EndCombat()
        {
            switch (_currentCombat.TheWinner)
            {
                case WhoWon.Player:
                    PlayMonsterManager.RemovePlayMonster(_currentCombat.PlayMonsters);
                    foreach (var kvp in _currentCombat.defeatedMonsters) 
                    {
                        string monName = kvp.Key;
                        int count = kvp.Value;
                        QuestManager.UpdateKillCounts(monName, count);
                    }
                    break;
                case WhoWon.Monster:

                    break;
            }
            _currentCombat.ClearEntityMaps();
            PlayerManager.ClearAllPlayerAspects();
            _previousCombat = _currentCombat;
            _currentCombat= null;
            var player = PlayerManager.CurrentPlayer.MovementController;

            player.SetCurrentPos((Vector2)player.CachedPosition);
            player.ClearCachPos();
            player.ToggleAllowedToBeDrawn(true);
            SceneManager.SetState(SceneState.Play);

        }
    }
}
