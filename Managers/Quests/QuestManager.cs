using PlayingAround.Data.SaveData;
using PlayingAround.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Managers.Quests
{
    public static class QuestManager
    {
        private static Dictionary<string,QuestSaveData> _questSaveData;


        public static void LoadContent(Dictionary<string, QuestSaveData> data)
        {
            _questSaveData = data;
        }
        public static bool IsQuestComplete(string questID)
        {
            return _questSaveData[questID].stage == QuestStage.Completed;
        }

        internal static QuestStage GetStage(string questId)
        {
            return _questSaveData[questId].stage;
        }

        internal static bool IsObjectiveCompleted(string questId, string objectiveId)
        {
            return _questSaveData[questId].objectives[objectiveId].completed;
        }
    }
}

public enum QuestStage
{
    NotStarted,
    Accepted,
    ObjectiveCompleted,
    Completed,
    Failed,           // optional, in case you support quest failure
    Declined          // optional, for declined/repeatable quests
}