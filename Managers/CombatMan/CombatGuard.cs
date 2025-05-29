using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Entities.Monster.PlayMonsters;
using PlayingAround.Entities.Player;
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

        public static void CreateNewCombat(PlayMonsters playMonsters, Player player)
        {
            CurrentCombat = new CombatManager(playMonsters, player);
            SceneManager.SetState(SceneManager.SceneState.Combat);
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
