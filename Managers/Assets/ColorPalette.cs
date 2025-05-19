using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Managers.Assets
{
    public static class ColorPalette
    {
        public static Color Primary = new Color(25, 25, 112); // MidnightBlue
        public static Color Accent = new Color(173, 216, 230); // LightBlue
        public static Color Background = Color.Black;
        public static Color ButtonDefault = Color.Gray;
        public static Color ButtonHover = Color.LightGray;
        public static Color ButtonText = Color.White;
        public static Color Shadow = Color.Black;
        public static Color DarkColor = new Color(62, 27, 36);
        public static Color LightColor = new Color(255, 213, 167);

        // You can also include alpha variants
        public static Color TransparentWhite = new Color(255, 255, 255, 128);
    }

}
