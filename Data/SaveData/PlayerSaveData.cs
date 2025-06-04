using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Data.SaveData
{
    public class PlayerSaveData
    {
        public float Speed { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string TextureKey { get; set; }
        public float CurrentPosX { get; set; }
        public float CurrentPosY { get; set; }
        public List<SummonsSaveData> PlayerSummons { get; set; }
        public int DrawEnlargementFactor { get; set; }
    }

}

