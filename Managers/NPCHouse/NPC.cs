using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Managers.Assets;
using PlayingAround.Managers.Dialogue;
using PlayingAround.Managers.Tiles;
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
        public string name { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        private readonly NPCData _data;
        public DialogueData AllDialogue { get; set; }
        public Texture2D HeadTexture;
        public Color HeadColor;
        public Color EyeColor;
        public Texture2D LeftEyeTexture;
        public Texture2D RightEyeTexture;
        public Vector2 currentPos { get; set; }
        public Vector2 drawFromPosition { get; set; }

        public NPC(NPCData data, Vector2 Pos, DialogueData dialogueData)
        {
            _data = data;
             currentPos = Pos;
            drawFromPosition = TileManager.OffSetFromCenterOfDiamond(currentPos, width, height);
            AllDialogue = dialogueData;
            name = data.name;
            width = data.width;
            height = data.height;
            HeadTexture = AssetManager.GetTexture(data.headTexturePath);
            LeftEyeTexture = AssetManager.GetTexture("LeftFlat");
            RightEyeTexture = AssetManager.GetTexture("RightOutward");
            EyeColor = ColorPalette.GetElementColor( data.eyeColor);
            HeadColor = ColorPalette.GetElementColor(data.headColor);
        }
        public void Draw(SpriteBatch sb)
        {
            Rectangle rect = new Rectangle((int)drawFromPosition.X, (int)drawFromPosition.Y - (width / 2), width, height);
            sb.Draw(HeadTexture, rect, HeadColor);
            sb.Draw(LeftEyeTexture, rect, EyeColor);
            sb.Draw(RightEyeTexture, rect, EyeColor);
        }

    }
}
