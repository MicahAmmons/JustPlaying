using PlayingAround.Managers.CombatMan.CombatAttacks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.ActFolder
{
    public class ActData
    {
        public List<SpecificActData> ActionOrder {  get; set; } = new List<SpecificActData>();
    }
    public class SpecificActData
    {
        public ActType Type { get; set; }
        public SingleAttackData AttackData { get; set; }
        public ActionTarget ActionTarget { get; set; }
        public MovementAmount MovementAmount { get; set; }
    }
}
public enum ActType
{
    Attack,
    Move,
    Summon,
    EndTurn

}
public enum AiActionType
{
    Attack,
    Move
}

public enum ActionTarget
{
    ClosestEnemy,
    Self,
    RandomEnemy,
    AwayFromEnemy,
    HighestHP,
    StayAtMaxAttackRange
}

public enum MovementAmount
{
    FullMP,
    HalfMP,
    Fixed,
}