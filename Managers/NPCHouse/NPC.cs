using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Data.NPCs;
using PlayingAround.Managers.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PlayingAround.Managers.NPCHouse
{
    public class NPC
    {
        [JsonPropertyName("name")] public string name { get; set; }
        [JsonPropertyName("drawEnlargementFactor")] public int drawEnlargementFactor { get; set; }
        public Vector2 currentPos { get; set; }
        public Vector2 drawFromPosition { get; set; }

        public Texture2D texture { get; set; }
        public string texturePath { get; set; }

        public List<Vector2> MovePath = new List<Vector2>();
        public bool AllowedToMove = true;
        public float MovementQuickness = 200;

        public NPC()
        {

        }
        public NPC(NPCData data)
        {
            name = data.name;
            texture = AssetManager.GetTexture("DefaultNPC");
        }


    }
}
