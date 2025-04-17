using PlayingAround.Entities.Monster.PlayMonsters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Managers.CombatMan.CombatAttacks
{
    public class ListOfAttacks
    {
        public List<SingleAttack> Attacks { get; private set; }


        public ListOfAttacks(List<SingleAttack> attacks)
        {
                Attacks = attacks ?? new List<SingleAttack>();

        }
    }
}
