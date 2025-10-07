using PlayingAround.ConditionsAndEffects.ConditionFolder;
using PlayingAround.ConditionsAndEffects.EffectFolder;
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
        public List<Condition> conditions { get; set; } = new();
        public List<Outcome> effects { get; set; } = new(); // optional
    }

    public class DialogueResponse
    {
        public string text { get; set; }
        public string nextDialogue { get; set; }
        public List<Outcome> effects { get; set; } = new(); // allow per-response effects
    }
}
