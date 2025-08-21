using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Managers.Assets;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.ButtonsFolder
{
    public class ButtonManager
    {
        public List<Button> buttons;
    }
    public class Button
    {
        public bool AllowedToBeDrawn = false;
        public bool AllowedToInputTrack = false;
        public Texture2D Texture = AssetManager.GetTexture("fightBackground");
        public Vector2 DrawPoint;
        public Rectangle DrawRectangle;
        public ButtonType ButtonType;

    }
}

public enum ButtonType
{
    Move,
    Attack,

}