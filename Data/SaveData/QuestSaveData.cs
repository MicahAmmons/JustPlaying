using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Data.SaveData
{
    public class QuestSaveData
    {
        public QuestStage stage { get; set; } = QuestStage.NotStarted;
        public Dictionary<string, SavedQuestObjective> Objectives { get; set; } = new();
    }

    public class SavedQuestObjective
    {
        public int ProgressCount { get; set; } = 0;
        public QuestObjectiveProgressionState ProgressState { get; set; } = QuestObjectiveProgressionState.NotStarted;
        
    }
}

public enum QuestObjectiveProgressionState
{
    Default,
    Completed,
    InProgress,
    NotStarted
}
public enum QuestStage
{
    NotStarted,
    Accepted,
    Completed,
    Failed,           // optional, in case you support quest failure
    Declined          // optional, for declined/repeatable quests
}