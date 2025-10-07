using PlayingAround.Managers.Dialogue;
using PlayingAround.Managers.Quests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.ConditionsAndEffects.ConditionFolder
{
    public static class ConditionManager
    {


        public static bool ConditionsAreMet(List<Condition> conditions)
        {
            // return false if anything fails, otherwise true if it makes it all teh way through 
            foreach (var condition in conditions)
            {
                switch (condition.Type)
                {
                    case ConditionType.QuestStage:
                        if (QuestManager.GetStage(condition.QuestId) != condition.QuestStage)
                            return false;
                        break;

                    case ConditionType.ObjectiveProgress:
                        if (!QuestManager.ObjectiveProgressIs(condition.QuestId, condition.ObjectiveId, condition.ProgressionStateId))
                            return false;
                        break;
                }
            }

            return true; // all conditions passed
        }

    }
}
