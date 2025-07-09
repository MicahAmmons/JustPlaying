using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Managers.Quests
{
    public class QuestData
    {
        public List<QuestObjective> objectives { get; set; }
    }

    public class QuestObjective
    {
        public string id { get; set; }
        public string killTargetId { get; set; }
        public int requiredCount { get; set; }
        public QuestObjectiveType objectiveType { get; set; }

    }
}
public enum QuestObjectiveType
{
    KillCount
}