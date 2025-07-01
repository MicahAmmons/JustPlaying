using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Managers.CombatMan.ActionLibrary
{
    public class AiAction
    {
        public AiActionType Action { get; set; }
        public AttackName? Attack { get; set; }
        public ActionTarget Target { get; set; }
        public MovementAmount? MovementAmount { get; set; }
    }
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
    RandomEnemy
}

public enum MovementAmount
{
    FullMP,
    HalfMP,
    Fixed
}