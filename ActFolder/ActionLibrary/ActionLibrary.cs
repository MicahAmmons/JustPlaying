//using PlayingAround.Entities.Monster.CombatMonsters;
//using System.Collections.Generic;
//using System;
//using static CombatStateMachine;
//using PlayingAround.ActFolder;

//public static class ActionLibrary
//{
//    public static readonly Dictionary<AiActionType, Func<ActType, CombatMonster, bool>> Executors =
//        new()
//        {
//            { ActType.Attack, ExecuteAttack },
//            { ActType.Move, ExecuteMove }
//        };
//    public static readonly Dictionary<ActType, CombatState> ActionStates =
//        new()
//        {
//            { ActType.Attack, CombatState.ExecutingAttack },
//            { ActType.Move, CombatState.ExecutingMove }
//        };

//    private static readonly Dictionary<ActionTarget, Func<CombatMonster, AttackName, bool>> AttackExecutors =
//        new()
//        {
//            { ActionTarget.ClosestEnemy, (monster, attack) => monster.AttackClosestEnemy(attack) },
//            //{ ActionTarget.Self,         (monster, attack) => monster.AttackSelf(attack) },
//            //{ ActionTarget.RandomEnemy,  (monster, attack) => monster.AttackRandomEnemy(attack) }
//        };

//    private static readonly Dictionary<MovementAmount, Func<CombatMonster, ActionTarget, bool>> MoveExecutors =
//        new()
//        {
//            { MovementAmount.FullMP, (monster, target) => monster.MoveUpToFullMP(target) },
//           // { MovementAmount.HalfMP, (monster, target) => monster.MoveTowardTargetHalfMP(target) },
//           // { MovementAmount.Fixed,  (monster, target) => monster.MoveTowardTargetFixed(target, 3) }
//        };
    
//    private static bool ExecuteAttack(Act action, CombatMonster monster)
//    {
        
//        var executor = AttackExecutors[action.Target];
//        return executor(monster, action.Attack.Value);
//    }

//    private static bool ExecuteMove(Act action, CombatMonster monster)
//    {
//        var executor = MoveExecutors[action.MovementAmount.Value];
//        return executor(monster, action.Target);
//    }
//}
