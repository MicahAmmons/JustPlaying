using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Data.SaveData
{
    public class SummonsSaveData
    {
        public string Name { get; set; }
        public int NumberOfKills { get; set; }
        public Dictionary<string, int> AbilityPoints { get; set; } = new Dictionary<string, int>
        {
            { "Health", 0 },
            { "Attack", 0 },
            { "Defense", 0 }
        };
    }
}
