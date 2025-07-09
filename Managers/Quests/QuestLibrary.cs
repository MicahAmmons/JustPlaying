using PlayingAround.Entities.Monster;
using PlayingAround.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Managers.Quests
{

    public static class QuestLibrary
    {

        private static Dictionary<string, QuestData> _questsData;
         private static Dictionary<string, List<(string questId, QuestObjective objective)>> _killObjectiveIndex = new(); // string is the name of the monster, i.e. training_dummy
        public static void LoadContent()
        {
            _questsData = JsonLoader.LoadQuests();

            foreach (var (questId, questData) in _questsData)
            {
                foreach (var objective in questData.objectives)
                {
                    if (objective.objectiveType == QuestObjectiveType.KillCount)
                    {
                        string key = objective.killTargetId;

                        if (!_killObjectiveIndex.ContainsKey(key))
                            _killObjectiveIndex[key] = new();

                        _killObjectiveIndex[key].Add((questId, objective));
                    }
                }
            }
        }

        public static QuestData GetQuestData(string questId)
        {
            return _questsData.TryGetValue(questId, out var data) ? data : null;
        }

        public static List<(string questId, QuestObjective objective)> GetKillObjectivesFor(string monsterId)
        {
            return _killObjectiveIndex.TryGetValue(monsterId, out var list) ? list : new();
        }
    }
}
