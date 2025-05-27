using PlayingAround.Entities.Monster.PlayMonsters;
using PlayingAround.Entities.Player;
using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
