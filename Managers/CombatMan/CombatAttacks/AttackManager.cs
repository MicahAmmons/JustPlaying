using PlayingAround.Entities.Monster.CombatMonsters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Managers.CombatMan.CombatAttacks
{
    public static class AttackManager
    {


        public static void PerformAttack(SingleAttack att, CombatMonster attacker, CombatMonster target)
        {
            if (AttackHits(att, attacker, target))
            {
                target.Health -= att.Damage;
            }
        }

        public static bool AttackHits(SingleAttack att, CombatMonster attacker, CombatMonster target)
        {
            return true;
        }
        public static CombatMonster ChooseTarget(List<CombatMonster> mons, SingleAttack att)
        {
            if (att.Name == "slam")
            {
                foreach (var mon in mons)
                {
                   if (mon.isPlayerControled)
                    {
                        return mon;
                    }
                }
            }
            return mons[1];
        }
    }
}
