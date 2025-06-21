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
        private static Player _currentPlayer => PlayerManager.CurrentPlayer;

        public static void CreateNewCombat(PlayMonsters playMonsters)
        {
            _currentCombat = new CombatManager(playMonsters);
            SceneManager.SetState(SceneState.Combat);
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
                    PlayMonsterManager.RemovePlayerMonster(_currentCombat.PlayMonsters);
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
            _previousCombat = _currentCombat;
            _currentCombat= null;
            PlayerManager.AllowPlayerMovement(true);
            SceneManager.SetState(SceneState.Play);

        }
    }
}
