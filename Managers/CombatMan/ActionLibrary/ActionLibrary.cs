using PlayingAround.Entities.Monster.CombatMonsters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CombatStateMachine;

namespace PlayingAround.Managers.CombatMan.ActionLibrary
{
    public static class ActionLibrary
    {
        public static readonly Dictionary<MonsterActionOrder, Func<CombatMonster, bool>> Executors =
            new()
            {
            { MonsterActionOrder.MoveTowardsClosestEnemy, monster => monster.GetMovementCellPathToClosestEnemy() },
            { MonsterActionOrder.AttackClosestEnemy, monster => monster.AttackClosestEnemy() },

            };

        public static readonly Dictionary<MonsterActionOrder, AITurnState> StateMap =
            new()
            {
            { MonsterActionOrder.MoveTowardsClosestEnemy, AITurnState.ExecutingMove },
            { MonsterActionOrder.AttackClosestEnemy, AITurnState.ExecutingAttack },
            };
    }
}