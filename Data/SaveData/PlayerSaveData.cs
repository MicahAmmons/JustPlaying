using PlayingAround.AnimationFolder;
using PlayingAround.Entities.Monster.CombatMonsters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Data.SaveData
{
    public class PlayerSaveData
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public string TextureKey { get; set; }
        public float CurrentPosX { get; set; }
        public float CurrentPosY { get; set; }
        public int Health { get; set; }
        public string AnimationData { get; set; }
    }

}

