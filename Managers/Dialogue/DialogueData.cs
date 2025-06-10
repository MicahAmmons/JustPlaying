using System;
using System.Collections.Generic;

namespace PlayingAround.Managers.Dialogue
{
    public class DialogueData
    {
        public string npcId { get; set; }
        public DialogueStage defaultNode { get; set; }
        public List<DialogueStage> stages { get; set; }
    }

    public class DialogueNode
    {
        public string text { get; set; }
        public List<DialogueResponse> responses { get; set; }
    }

    public class DialogueStage : DialogueNode
    {
        public string id { get; set; }
        public List<DialogueCondition> conditions { get; set; } = new();
        public List<DialogueEffect> effects { get; set; } = new(); // optional
    }

    public class DialogueResponse
    {
        public string text { get; set; }
        public string nextDialogue { get; set; }
        public List<DialogueEffect> effects { get; set; } = new(); // allow per-response effects
    }

    public class DialogueCondition
    {
        public DialogueConditionType type { get; set; }
        public string questId { get; set; }
        public string objectiveId { get; set; } // optional, only needed for objective-related conditions
        public QuestStage questStage { get; set; }
    }

    public class DialogueEffect
    {
        public DialogueEffectType type { get; set; }         // "StartQuest", "CompleteQuest", etc.
        public string questId { get; set; }      // used by most effects
        public QuestStage stage { get; set; }
        public string itemId { get; set; }       // optional, for item-based effects
        public string customTrigger { get; set; } // optional, for game-defined triggers
    }
}
