using Microsoft.Xna.Framework.Input;
using PlayingAround.Managers.CombatMan.Aspects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.ConditionsAndEffects.ConditionFolder
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

    }

}
public enum ConditionType
{
    None,
    QuestStage,
    ObjectiveProgress,
    AspectObtained,
    KeyPressed
}
