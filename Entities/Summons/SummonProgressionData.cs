using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Entities.Summons
{
    public class SummonProgressionData
    {


        public BaseStatsData BaseStats { get; set; }
        public int XPForLevel1 { get; set; }
        public float XPMultiplier { get; set; }
        public Dictionary<int, string> MilestoneLevels { get; set; }
        public Dictionary<string, int> StatGainPerPoint { get; set; }


    }


    public class BaseStatsData
    {
        public int Health { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
    }
}
