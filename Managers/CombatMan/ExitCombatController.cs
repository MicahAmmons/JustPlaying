using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.ButtonsFolder;
using PlayingAround.Manager;
using PlayingAround.Managers.Assets;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Managers.CombatMan
{
    public class ExitCombatController
    {
        public Button Button;
        public string OverButtonText;
        public Texture2D BehindButtonTexture = AssetManager.GetTexture("fightBackground");
        public Rectangle BehindButtonRectangle;
        public SpriteFont Font = AssetManager.GetFont("mainFont");

        public ExitCombatController(bool won)
        {
            BehindButtonRectangle = new Rectangle(710, 440, 500, 200);
            Button = new Button(new Rectangle(885, 580, 150, 50));
        }
        public void Draw(SpriteBatch sb)
        {
            sb.Draw(BehindButtonTexture, BehindButtonRectangle, Color.White);
            sb.DrawString(Font, OverButtonText, BehindButtonRectangle, Color.White);
            if (Button != null){Button.Draw(sb);}
        }
        public void Update()
        {
            var mousePoint = new Point(InputManager.MouseX, InputManager.MouseY);
            bool leftPressedThisFrame = InputManager.IsLeftClick();
            if (Button != null) 
            { 
                Button.UpdateInput(mousePoint,leftPressedThisFrame); 
            }
        }
    }
}
