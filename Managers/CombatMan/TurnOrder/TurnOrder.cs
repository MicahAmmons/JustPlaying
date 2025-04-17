using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Entities.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Managers.CombatMan.TurnOrder
{
    public class TurnOrder
    {
        public Player Player;
        public List<CombatMonster> Monsters;


        public TurnOrder(Player player, List<CombatMonster> mons)
        {
            Player = player;

        }
    }
}
