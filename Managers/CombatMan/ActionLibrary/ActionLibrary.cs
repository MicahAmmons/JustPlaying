using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Managers.CombatMan.ActionLibrary;
using System.Collections.Generic;
using System;
using static CombatStateMachine;

public static class ActionLibrary
{
    public static readonly Dictionary<AiActionType, Func<AiAction, CombatMonster, bool>> Executors =
        new()
        {
            { AiActionType.Attack, ExecuteAttack },
            { AiActionType.Move, ExecuteMove }
        };
    public static readonly Dictionary<AiActionType, AITurnState> ActionStates =
        new()
        {
            { AiActionType.Attack, AITurnState.ExecutingAttack },
            { AiActionType.Move, AITurnState.ExecutingMove }
        };

    private static readonly Dictionary<ActionTarget, Func<CombatMonster, AttackName, bool>> AttackExecutors =
        new()
        {
            { ActionTarget.ClosestEnemy, (monster, attack) => monster.AttackClosestEnemy(attack) },
            //{ ActionTarget.Self,         (monster, attack) => monster.AttackSelf(attack) },
            //{ ActionTarget.RandomEnemy,  (monster, attack) => monster.AttackRandomEnemy(attack) }
        };

    private static readonly Dictionary<MovementAmount, Func<CombatMonster, ActionTarget, bool>> MoveExecutors =
        new()
        {
            { MovementAmount.FullMP, (monster, target) => monster.MoveUpToFullMP(target) },
           // { MovementAmount.HalfMP, (monster, target) => monster.MoveTowardTargetHalfMP(target) },
           // { MovementAmount.Fixed,  (monster, target) => monster.MoveTowardTargetFixed(target, 3) }
        };
    
    private static bool ExecuteAttack(AiAction action, CombatMonster monster)
    {
        
        var executor = AttackExecutors[action.Target];
        return executor(monster, action.Attack.Value);
    }

    private static bool ExecuteMove(AiAction action, CombatMonster monster)
    {
        var executor = MoveExecutors[action.MovementAmount.Value];
        return executor(monster, action.Target);
    }
}
