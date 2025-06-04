using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Utils
{
    public static class DrawDiamondTexture
    {

        private static GraphicsDevice _graphics; 
        public static void LoadContent(GraphicsDevice graphics)
        {
            _graphics = graphics;
        }
        public static Texture2D GetDiamond(int width, int height, Color color)
        {
            Texture2D texture = new Texture2D(_graphics, width, height);
            Color[] data = new Color[width * height];

            int centerX = width / 2;
            int centerY = height / 2;

            for (int y = 0; y < height; y++)
            {
                // Normalized distance from vertical center
                float normY = Math.Abs(y - centerY) / (float)centerY;
                int halfRowWidth = (int)(centerX * (1 - normY));

                int startX = centerX - halfRowWidth;
                int endX = centerX + halfRowWidth;

                for (int x = startX; x <= endX; x++)
                {
                    if (x >= 0 && x < width)
                        data[y * width + x] = color;
                }
            }

            texture.SetData(data);
            return texture;
        }
    }
}
