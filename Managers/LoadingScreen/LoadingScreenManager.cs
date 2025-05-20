using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Managers.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Managers.LoadingScreen
{
    public static class LoadingScreenManager
    {

        public static void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(AssetManager.GetTexture("fightBackground"), new Rectangle(1920, 1080, 0, 0), Color.White);
        }
    }
}
