using PlayingAround.Managers.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.AnimationFolder.GlowTex
{
    public class GlowTextureData
    {
        public string name { get; set; }
        public string color { get; set; }
        public float speed { get; set; }
        public float delay { get; set; }
        public float minFade { get; set; } = 0f; 
        public float maxFade { get; set; } = 1f;


    }
}
