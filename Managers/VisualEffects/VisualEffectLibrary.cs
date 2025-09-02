using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Managers.Assets;
using PlayingAround.Utils;
using PlayingAround.Visuals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Managers.VisualEffects
{
    public static class VisualEffectLibrary
    {
        private static Dictionary<string, VisualEffectData> _effects;

        public static void LoadContent()
        {
         //  _effects = JsonLoader.LoadVEData();
        }
        public static VisualEffectData GetVE(string name)
        {
            return _effects[name];
        }
    }
}
