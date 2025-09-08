using PlayingAround.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.AnimationFolder
{
    public static class AnimationLibrary
    {
        private static Dictionary<string, AnimationData> _data;


        public static void LoadContrent()
        {
            _data = JsonLoader.LoadAnimationData();
        }
        public static AnimationData GetAnimation(string name)
        {
            return _data[name];
        }

        internal static AnimationData GetIdleAnimationData(string name)
        {
            return _data[name];
        }
    }
}
