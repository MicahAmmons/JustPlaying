using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Data.NPCs;
using PlayingAround.Game.Map;
using PlayingAround.Managers.Dialogue;
using PlayingAround.Managers.NPCHouse;
using PlayingAround.Managers.Tiles;
using PlayingAround.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Managers.NPCHouse
{
    public static class NPCManager
    {
        private static Dictionary<string, NPCData> _dataNPC;
        private static List<NPC> _currentNPCs => TileManager.CurrentMapTile.NPCs;
        public static List<NPC> CurrentNPCs;

        public static void LoadContent()
        {
            _dataNPC = JsonLoader.LoadNPCData();
        }
        public static NPC GenerateNPC(string name, TileCell cell)
        {
            Vector2 currentPos = new Vector2(cell.CenterPoint.X, cell.CenterPoint.Y);

            NPC npc = new NPC(_dataNPC[name])
            {
                currentPos = currentPos,
                AllDialogue = DialogueLibrary.GetDialogueData(name)
            };
            return npc;
        }
        public static void Draw(SpriteBatch spriteBatch)
        {
            if (SceneManager.CurrentState == SceneState.Dialogue || SceneManager.CurrentState == SceneState.Play)
            {
                DrawCurrentNPCs(spriteBatch);
            }
        }
        public static void DrawCurrentNPCs(SpriteBatch spriteBatch)
        {
            if (_currentNPCs == null)  return;

            foreach (var npc in _currentNPCs)
            {
                Vector2 currentPos = npc.currentPos;
                Vector2 drawSpot = TileManager.OffSetFromCenterOfDiamond(currentPos, npc.width, npc.height);
                Rectangle rect = new Rectangle((int)drawSpot.X, (int)drawSpot.Y - (npc.width / 2), npc.width, npc.height);
                spriteBatch.Draw(npc.texture, rect, Color.White);
            }
        }





    }
}
