using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Managers.Quests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PlayingAround.Triggers.TriggerManager;

namespace PlayingAround.Triggers.EffectFolder
{
    public class OutcomeExecutor
    {

        public void HandleOutcomes(params Outcome[] outcomes)
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
                        case OutcomeType.NotificationText:

                }
            }
        }

        internal void Execute(FiredNode firedNode)
        {
            var trig = firedNode.Trigger;
            var node = trig.TriggerNodes[firedNode.NodeIndex];

            foreach (var group in node.Outcomes)
                HandleOutcomes(group);
        }
    }
}
