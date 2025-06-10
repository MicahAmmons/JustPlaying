using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Managers.Quests
{
    public class QuestData
    {
        public string description { get; set; }
        public List<QuestObjective> objectives { get; set; }
        public QuestRewards rewards { get; set; }
        public List<StageDescriptions> stageDescriptions { get; set; }
    }

    public class QuestObjective
    {
        public string id { get; set; }
        public string description { get; set; }
        public string targetId { get; set; }
        public int requiredCount { get; set; }
        public QuestObjectiveType objectiveType { get; set; }
        public ObjectiveActivationStage activationStage { get; set; }

    }

    public class StageDescriptions
    {
        public QuestStage activatedStage { get; set; }
        public string description { get; set; }
    }
    public class QuestRewards
    {
        public int experience { get; set; }
        public List<string> items { get; set; } = new();
        public List<string> customTriggers { get; set; } = new();
    }

}
public enum ObjectiveActivationStage
{
    Always,
    OnAccepted,
}
public enum QuestObjectiveType
{
    KillCount
}