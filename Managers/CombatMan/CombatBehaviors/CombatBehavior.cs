using PlayingAround.Entities.Monster.CombatMonsters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Managers.CombatMan.CombatBehavior
{
    public class CombatBehavior
    {
        //public 

    }
}

public enum TurnActionOptions
{
      MoveTowardsClosestEnemy,
      AttackClosestEnemy
}
public enum AttackPriority
{
    UseShortestRangeAttack,
}