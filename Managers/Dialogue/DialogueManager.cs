using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Entities.Player;
using PlayingAround.Manager;
using PlayingAround.Managers.Entities;
using PlayingAround.Managers.NPCHouse;
using PlayingAround.Managers.Quests;
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
            if (SceneManager.IsState(SceneState.Dialogue))
            {
                DialogueBox.Draw(spriteBatch);
            }
        }
        public static void Update()
        {
            if (SceneManager.IsState(SceneState.Dialogue))
            {
                UpdatePlayerInput();
            }
        }
        public static void UpdatePlayerInput()
        {
            if (InputManager.IsLeftClick())
            {
                var mousePos = new Vector2(InputManager.Mouse.X, InputManager.Mouse.Y);
                var selectedIndex = DialogueBox.GetClickedResponseIndex(mousePos);
                if (selectedIndex is int i && i < _currentDialogueStage.responses.Count)
                {
                    var response = _currentDialogueStage.responses[i];
                    HandleResponse(response);
                }
            }

        }
        private static void HandleResponse(DialogueResponse response)
        {
            if (response.effects != null || response.effects.Count >= 0)
                foreach (var effect in response.effects)
                    HandleEffect(effect);

            if (!string.IsNullOrEmpty(response.nextDialogue))
            {
                _currentDialogueStage = _currentDialogue.stages.FirstOrDefault(s => s.id == response.nextDialogue);
                DialogueBox.SetDialogue(
                    null,
                    _currentDialogueStage.text,
                    _currentDialogueStage.responses.Select(r => r.text).ToList()
                );
            }
            else
            {
                EndDialogue();
            }
        }
        public static void HandleEffect(DialogueEffect effect)
        {
            switch (effect.type)
            {
                case DialogueEffectType.SetQuestStage:
                    QuestManager.UpdateQuestStageTo(effect.questId, effect.questStage);
                    break;
                case DialogueEffectType.CompleteQuest:
                    QuestManager.CompleteQuest(effect.questId);
                    GiveReward(effect);
                    break;
                case DialogueEffectType.StartQuest:
                    QuestManager.StartQuest(effect.questId);
                    break;
                case DialogueEffectType.SetObjectiveProgressState:
                    QuestManager.SetObjectiveProgress(effect.progressionStateId, effect.questId, effect.ObjectiveId);
                    break;
            }
        }
        public static void GiveReward(DialogueEffect effect)
        {
            
        }
        public static void StartNewDialogue(NPC npc)
        {
            PlayerManager.AllowPlayerMovement(false);
            _currentDialogue = npc.AllDialogue;
            _currentDialogueStage = FetchCurrentStage(npc.AllDialogue);
                DialogueBox.SetDialogue(
                                   npc.name,
                                   _currentDialogueStage.text,
                                   _currentDialogueStage.responses.Select(r => r.text).ToList()
                                   );
            
        }
        private static DialogueStage FetchCurrentStage(DialogueData data)
        {
            foreach (var stage in data.stages)
            {
                if (stage == null || stage.conditions == null || stage.conditions.Count <= 0) continue;
                if (ConditionsAreMet(stage.conditions))
                    return stage;
            }
            return _currentDialogue.defaultNode;
        }
        private static bool ConditionsAreMet(List<DialogueCondition> conditions)
        {
            // return false if anything fails, otherwise true if it makes it all teh way through 
            foreach (var condition in conditions)
            {
                switch (condition.type)
                {
                    case DialogueConditionType.QuestStage:
                        if (QuestManager.GetStage(condition.questId) != condition.questStage)
                            return false;
                        break;

                    case DialogueConditionType.ObjectiveProgress:
                        if (!QuestManager.ObjectiveProgressIs(condition.questId, condition.objectiveId, condition.progressionStateId))
                            return false;
                        break;
                }
            }

            return true; // all conditions passed
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
            DialogueBox.ClearDialogue();
            PlayerManager.AllowPlayerMovement(true);
            SceneManager.SetState(SceneState.Play);
        }

    }
}

public enum DialogueConditionType
{
    None,
    QuestStage,
    ObjectiveProgress

}
public enum DialogueEffectType
{
    SetQuestStage,
    CompleteQuest,
    StartQuest,
    SetObjectiveProgressState
}