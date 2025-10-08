using Microsoft.Xna.Framework.Input;
using PlayingAround.Entities.Monster.PlayMonsters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Triggers.EffectFolder
{
    public class Outcome
    {
        public OutcomeType Type { get; set; }
        public string QuestId { get; set; }      // used by most effects
        public QuestObjectiveProgressionState ProgressionStateId { get; set; }
        public string ObjectiveId { get; set; }
        public QuestStage QuestStage { get; set; }
        public string NotificationText { get; set; }
        public string Key { get; set; }
        public int Level { get; set; }
    }

}
public enum OutcomeType
{
    SetQuestStage,
    CompleteQuest,
    StartQuest,
    SetObjectiveProgressState,
    NotificationText,
    AdvanceLevels,
    StartCombat
}