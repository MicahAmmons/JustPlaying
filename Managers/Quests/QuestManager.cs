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
        internal static void UpdateQuestStageTo(string questID,  QuestStage stage)
        {
            _questSaveData[questID].stage = stage;
        }
        internal static void CompleteQuest(string questID)
        {
            _questSaveData[questID].stage = QuestStage.Completed;
        }
        internal static void StartQuest(string questID)
        {
            _questSaveData[questID].stage = QuestStage.Accepted;
        }
        internal static void UpdateKillCounts(string monsterName, int count)
        {
            var matchesForMonster = QuestLibrary.GetKillObjectivesFor(monsterName);
            
            foreach (var (questId, objective) in matchesForMonster)
            {
                var saveData = _questSaveData[questId];
                var progress = saveData.objectives[objective.id];

                if (saveData.stage is QuestStage.NotStarted or QuestStage.Completed or QuestStage.Declined)
                    continue;
                if (!ObjectiveIsActiveForStage(saveData.stage, objective.activationStage))
                    continue;
                if (saveData.objectives[objective.id].completed) continue;

                progress.progress += count;
                if (progress.progress >= objective.requiredCount)
                    progress.completed = true;
                if (progress.completed)
                    UpdateQuestStageTo(questId, QuestStage.ObjectiveCompleted);
            }
        }
        private static bool ObjectiveIsActiveForStage(QuestStage current, ObjectiveActivationStage required)
        {
            return required switch
            {
                ObjectiveActivationStage.Always => true,
                ObjectiveActivationStage.OnAccepted => current == QuestStage.Accepted,
                _ => false
            };
        }

        internal static Dictionary<string, QuestSaveData> SaveQuestData()
        {
            return _questSaveData;
        }
    }
}

public enum QuestStage
{
    NotStarted,
    Accepted,
    ObjectiveInProgress1,
    ObjectiveInProgress2,
    ObjectiveCompleted1,
    ObjectiveCompleted2,
    Completed,
    Failed,           // optional, in case you support quest failure
    Declined          // optional, for declined/repeatable quests
}