using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Entities.Player;
using PlayingAround.Managers.Entities;
using PlayingAround.Managers.NPCHouse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Managers.Dialogue
{
    public static class DialogueManager
    {

        private static Player _currentPlayer => PlayerManager.CurrentPlayer;
        private static DialogueData _currentDialogue;
        private static DialogueStage _currentDialogueStage;

        public static void Draw(SpriteBatch spriteBatch)
        {
            if (SceneManager.CurrentState == SceneManager.SceneState.Dialogue)
            {
                DrawCurrentStage(spriteBatch);
            }
        }
        private static void DrawCurrentStage(SpriteBatch spriteBatch)
        {

        }
        public static void Update()
        {
            UpdatePlayerInput();
        }
        public static void UpdatePlayerInput()
        {

        }
        public static void StartNewDialogue(DialogueData dialogue)
        {
           _currentDialogue = dialogue;
            _currentDialogueStage = FetchCurrentStage(dialogue);
        }
        private static DialogueStage FetchCurrentStage(DialogueData data)
        {
            foreach (var stage in data.stages)
            {
                if (stage == null || stage.conditions == null || stage.conditions.Count <= 0) continue;
                if (ConditionsAreMet(stage.conditions))
                    return stage;
            }
            return null;
        }
        private static bool ConditionsAreMet(List<DialogueCondition> conditions)
        {
            foreach (var condition in conditions)
            {
               switch (condition.type)
                {
                    case DialogueConditionType.QuestCompleted:
                        QuestManager.IsQuestComplet();
                        break;
                    case DialogueConditionType.QuestNotCompleted:

                        break;
                    case DialogueConditionType.QuestObjectiveCompleted:

                        break ;
                }
            }
            return true;
        }
        private static void EndDialogue()   
        {
            if (_currentDialogue != null)
            {
                _currentDialogue = null;
            }
            if (_currentDialogueStage != null)
            {
                _currentDialogueStage = null;
            }
        }

    }
}

public enum DialogueConditionType
{
    None,
    QuestNotCompleted,
    QuestCompleted,
    QuestObjectiveCompleted

}