using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Data
{
    public class PlayerSaveData
    {
        public float Speed { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string TextureKey { get; set; }  // Used to reload the texture via AssetManager
        public float FeetCenterX { get; set; }
        public float FeetCenterY { get; set; }
    }

}

