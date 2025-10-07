using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Managers.Quests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.ConditionsAndEffects.EffectFolder
{
    public static class OutcomeManager
    {
        public static void HandleOutcomes(params Outcome[] outcomes)
        {
            foreach (var outcome in outcomes)
            {
                switch (outcome.Type)
                {
                    case OutcomeType.SetQuestStage:
                        QuestManager.UpdateQuestStageTo(outcome.QuestId, outcome.QuestStage);
                        break;
                    case OutcomeType.CompleteQuest:
                        QuestManager.CompleteQuest(outcome.QuestId);
                        break;
                    case OutcomeType.StartQuest:
                        QuestManager.StartQuest(outcome.QuestId);
                        break;
                    case OutcomeType.SetObjectiveProgressState:
                        QuestManager.SetObjectiveProgress(outcome.ProgressionStateId, outcome.QuestId, outcome.ObjectiveId);
                        break;
                }
            }
        }
    }
}
