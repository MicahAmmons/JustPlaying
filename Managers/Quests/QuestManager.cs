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
        private static Dictionary<string, List<(string questId, string objectiveId)>> _killObjectiveIndex = new();


        public static void LoadContent(Dictionary<string, QuestSaveData> data)
        {
            _questSaveData = data;
            foreach (var (questId, questData) in data)
            {
                foreach (var (objectiveId, objective) in questData.objectives)
                {
                    if (objective.objectiveType == QuestObjectiveType.KillCount)
                    {
                        if (!_killObjectiveIndex.ContainsKey(objective.killId))
                            _killObjectiveIndex[objective.killId] = new();

                        _killObjectiveIndex[objective.killId].Add((questId, objectiveId));
                    }
                }
            }
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
            if (!_killObjectiveIndex.ContainsKey(monsterName)) return;
            
            foreach (var (questId, objectiveId) in _killObjectiveIndex[monsterName])
            {
                var quest = _questSaveData[questId];
                var objective = quest.objectives[objectiveId];
                if (quest.stage is QuestStage.NotStarted or QuestStage.Completed or QuestStage.Declined)
                    continue;
                if (!ObjectiveIsActiveForStage(quest.stage, objective.activationStage))
                    continue;
                if (objective.completed) continue;

                objective.progress += count;
                if (objective.progress >= objective.requiredAmount)
                    objective.completed = true;
                if (objective.completed)
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