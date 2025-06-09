using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Entities.Monster.PlayMonsters;
using PlayingAround.Entities.Player;
using PlayingAround.Managers.Entities;
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
        public static CombatManager CurrentCombat {  get; set; }
        private static Player _currentPlayer => PlayerManager.CurrentPlayer;

        public static void CreateNewCombat(PlayMonsters playMonsters)
        {
            CurrentCombat = new CombatManager(playMonsters, _currentPlayer);
            SceneManager.SetState(SceneState.Combat);
        }

        public static void Update(GameTime gameTime)
        {
            if (CurrentCombat != null) 
            {
                if (CurrentCombat.StateCombat == CombatStateMachine.CombatState.ExitingCombat)
                {

                    return;
                }
                CurrentCombat.Update(gameTime);


            }
        }
        public static void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {

            if (CurrentCombat != null)
            {
                if (CurrentCombat.StateCombat == CombatStateMachine.CombatState.ExitingCombat)
                {

                    return;
                }
                CurrentCombat.Draw(spriteBatch, graphicsDevice);


            }
        }
    }
}
