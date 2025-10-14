using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Managers;
using PlayingAround.Managers.CombatMan;
using PlayingAround.Managers.Quests;
using PlayingAround.Triggers.Notifications;
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
                        NotificationTextManager.AddNotificationBox(outcome.NotificationTextBox);
                        break;
                    case OutcomeType.AdvanceLevels:

                        break;
                    case OutcomeType.StartCombat:
                        SceneManager.SetState(SceneState.Combat);
                        CombatGuard.CreateNewCombat(outcome.PlayMonster);
                        break;
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
