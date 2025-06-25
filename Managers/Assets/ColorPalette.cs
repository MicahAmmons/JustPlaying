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
        public static Color TransparentWhite = new Color(255, 255, 255, 128);




        public static Color Fire = new Color(255, 69, 0);              // OrangeRed
        public static Color Ice = new Color(173, 216, 230);            // LightBlue
        public static Color Earth = new Color(139, 69, 19);            // SaddleBrown
        public static Color Wind = new Color(144, 238, 144);           // LightGreen
        public static Color Acid = new Color(154, 205, 50);            // YellowGreen
        public static Color Metal = new Color(192, 192, 192);          // Silver
        public static Color Electricity = new Color(255, 255, 0);      // Yellow
        public static Color Water = new Color(0, 191, 255);            // DeepSkyBlue
        public static Color Light = new Color(255, 250, 205);          // LemonChiffon
        public static Color Dark = new Color(72, 61, 139);             // DarkSlateBlue
        public static Color Normal = new Color(211, 211, 211);         // LightGray

        public static Color GetElementColor(ElementType element)
        {
            return element switch
            {
                ElementType.Fire => Fire,
                ElementType.Ice => Ice,
                ElementType.Earth => Earth,
                ElementType.Wind => Wind,
                ElementType.Acid => Acid,
                ElementType.Metal => Metal,
                ElementType.Electricity => Electricity,
                ElementType.Water => Water,
                ElementType.Light => Light,
                ElementType.Dark => Dark,
                ElementType.Normal => Normal,
                _ => Color.White
            };
        }
    }
}
