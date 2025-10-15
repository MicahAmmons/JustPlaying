using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Game.Map;
using PlayingAround.Managers.Dialogue;
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
        private static List<NPC> _currentNPCs => TileManager.CurrentMapTile?.NPCs;
        public static List<NPC> CurrentNPCs;

        public static void LoadContent()
        {
            _dataNPC = JsonLoader.LoadNPCData();
        }
        public static void Update(GameTime gametime)
        {
            if (_currentNPCs == null) { return; }
            if (_currentNPCs.Count == 0 ) return;
            foreach ( var npc in _currentNPCs)
            {
                npc.Update(gametime);
            }
        }
        public static NPC GenerateNPC(string name, TileCell cell)
        {
            Vector2 currentPos = new Vector2(cell.CenterPoint.X, cell.CenterPoint.Y);

            NPC npc = new NPC(_dataNPC[name], currentPos, DialogueLibrary.GetDialogueData(name))
            {
            };
            npc.SetStartingPoint(cell.CenterPoint);
            return npc;
        }
        public static void DrawStaticFrames(SpriteBatch spriteBatch)
        {
            if (SceneManager.CurrentState == SceneState.Dialogue || SceneManager.CurrentState == SceneState.Play)
            {
                foreach (var npc in  _currentNPCs)
                {
                    npc?.DrawStaticFrames(spriteBatch);
                }
            }
        }

        internal static void DrawCloudMovement(SpriteBatch spriteBatch, Effect fx)
        {
            foreach (var npc in _currentNPCs)
            {
                npc.DrawCloudMovement(spriteBatch, fx);
            }
        }
    }
}
