using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Managers.Dialogue
{
    public class DialogueData
    {
        public string npcId { get; set; }
        public DialogueNode defaultNode { get; set; }
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
        public List<DialogueCondition> conditions { get; set; }
        public List<DialogueEffect> effects { get; set; }
    }

    public class DialogueResponse
    {
        public string text { get; set; }
        public string nextStage { get; set; }
    }

    public class DialogueCondition
    {
        public DialogueConditionType type { get; set; }
        public string questId { get; set; }
        public string objectiveId { get; set; }
    }


    public class DialogueEffect
    {
        public string type { get; set; } // e.g., "startQuest", "completeQuest", "custom"
        public string questId { get; set; }
        public string itemId { get; set; } // optional
        public string customTrigger { get; set; } // optional
    }

}



