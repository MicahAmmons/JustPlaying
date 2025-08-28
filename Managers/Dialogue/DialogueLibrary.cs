    using PlayingAround.Utils;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    namespace PlayingAround.Managers.Dialogue
    {
        public static class DialogueLibrary
        {
            private static Dictionary<string, DialogueData> _dialogueBaseData = new Dictionary<string, DialogueData>();


            public static void LoadContent()
            {
                _dialogueBaseData = JsonLoader.LoadDialogueData();
            }
        public static DialogueData GetDialogueData(string name)
        {
            if (!_dialogueBaseData.ContainsKey(name)) return null;
            return _dialogueBaseData[name];
        }
        }
    }
