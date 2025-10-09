using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PlayingAround.ButtonsFolder;
using PlayingAround.Entities.Monster.PlayMonsters;
using PlayingAround.Managers.CombatMan;
using PlayingAround.Managers;
using PlayingAround.Managers.NPCHouse;
using PlayingAround.Managers.Dialogue;
using PlayingAround.Managers.Tiles;
using PlayingAround.Game.Map;
using PlayingAround.Managers.Assets;

namespace PlayingAround.Interaction
{
    public abstract class InteractData
    {
        public Keys KeyToPress { get; set; }
        public Button Button { get; set; }
        public string Text { get; set; }    
        public abstract void BeginInteraction();
    }
    public sealed class InteractDataCombat : InteractData
    {
        public PlayMonsters PlayMon { get; set; }
        public InteractDataCombat()
        {
            KeyToPress = Keys.F;
            Text =  $"Press {KeyToPress} To Fight";
        }
        public override void BeginInteraction()
        {
            SceneManager.SetState(SceneState.Combat);
            CombatGuard.CreateNewCombat(PlayMon);
        }
    }
    public sealed class InteractDataDialogue : InteractData
    {
        public NPC Npc { get; set; }
        public InteractDataDialogue()
        {
            KeyToPress = Keys.T;
            Text = $"Press {KeyToPress} To Talk";
        }
        public override void BeginInteraction()
        {
            SceneManager.SetState(SceneState.Dialogue);
            //DialogueManager.StartNewDialogue(Npc);
        }
    }
    public sealed class InteractDataNextTile : InteractData
    {
        public NextTileData NextTile { get; set; }
        public InteractDataNextTile()
        {
            KeyToPress = Keys.N;
            Text = $"Press {KeyToPress} To Proceed";
        }
        public override void BeginInteraction()
        {
            MapTileTransitionManager.SetNextMapTile(NextTile);
            SceneManager.SetState(SceneState.MapTileTransition);
        }
    }
}
