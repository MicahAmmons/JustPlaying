using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PlayingAround.Interfaces;
using PlayingAround.Managers.CombatMan.Aspects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Triggers.ConditionFolder
{
    public class Condition
    {
        public ConditionType Type { get; set; }
        public QuestObjectiveProgressionState ProgressionStateId { get; set; } = QuestObjectiveProgressionState.Default;
        public string ObjectiveId { get; set; }
        public string QuestId { get; set; }
        public QuestStage QuestStage { get; set; }
        public string AspectName { get; set; }
        public Keys Key { get; set; }
        public int ProximityDistance { get; set; }
        public bool AllowedToFight { get; set; } 
        public IProximityTracked AnchorPoint { get; set; }

    }

}
public enum ConditionType
{
    None,
    QuestStage,
    ObjectiveProgress,
    AspectObtained,
    KeyPressed,
    Proximity,
    AllowedToFight
}
