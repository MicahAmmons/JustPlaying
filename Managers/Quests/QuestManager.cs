using PlayingAround.Data.SaveData;
using PlayingAround.Managers.Dialogue;
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
        internal static bool ObjectiveProgressIs(string questId, string objectiveId, QuestObjectiveProgressionState progressId)
        {
            return _questSaveData[questId].Objectives[objectiveId].ProgressState == progressId;
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
        internal static void SetObjectiveProgress(QuestObjectiveProgressionState progressionState, string questId, string objectiveId)
        {
            _questSaveData[questId].Objectives[objectiveId].ProgressState = progressionState;
        }
        internal static void UpdateKillCounts(string monsterName, int count)
        {
            var matchesForMonster = QuestLibrary.GetKillObjectivesFor(monsterName);
            
            // Looks through each quest that has MONSTER as its objective kill count
            foreach (var (questId, objective) in matchesForMonster)
            {
                //This is looking at the SAVED DATA and the specific Objective of that data 
                QuestSaveData saveData = _questSaveData[questId];
                SavedQuestObjective currentProgress = saveData.Objectives[objective.id];

                // Skip if the quest isn't active or relevant
                if (saveData.stage is QuestStage.NotStarted or QuestStage.Completed or QuestStage.Declined)
                    continue;
                // Skip if the objective isn't in progress
                if (currentProgress.ProgressState != QuestObjectiveProgressionState.InProgress)
                    continue;

                currentProgress.ProgressCount += count;
                if (currentProgress.ProgressCount >= objective.requiredCount)
                    currentProgress.ProgressState = QuestObjectiveProgressionState.Completed;

            }
        }
        //private static bool ObjectiveIsActiveForStage(QuestSaveData data, QuestObjective objective)
        //{
        //    QuestStage current = data.stage;
        //    ObjectiveActivationStage required = objective.activationStage;
        //    return required switch
        //    {
        //        ObjectiveActivationStage.Always => true,
        //        ObjectiveActivationStage.OnAccepted => current == QuestStage.Accepted,
        //        ObjectiveActivationStage.PreviousConditionCompleted => IsPreviousObjectiveCompleted(data, objective),
        //        _ => false
        //    };
        //}
        //private static bool IsPreviousObjectiveCompleted(QuestSaveData data, QuestObjective objective)
        //{
        //    string prev = objective.PreviousId;
        //    if (data.objectives[prev].ProgressState == ObjectiveProgressionState.Completed == true && data.objectives[objective.id].ProgressState != ObjectiveProgressionState.Completed) return true;
        //    return false;

        //}
        internal static Dictionary<string, QuestSaveData> SaveQuestData()
        {
            return _questSaveData;
        }


    }
}

