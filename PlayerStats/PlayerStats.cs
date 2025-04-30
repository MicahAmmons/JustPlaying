using PlayingAround.Entities.Summons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Stats
{
    public class PlayerStats
    {
        public int MP { get; set; } = 4;
        public int SP { get; set; }
        public float MaxHealth { get; set; } = 25;
        public float CurrentHealth { get; set; }
        public float MaxMana { get; set; } = 15; 
        public float CurrentMana { get; set; }
        public float Initiation { get; set; } = 1;
        public List<SummonedMonster> UnlockedSummons { get; set; }
        public List<SummonedMonster> LockedSummons { get; set; }



        public PlayerStats() 
        {
            CurrentMana = MaxMana;
            CurrentHealth = MaxHealth;
            SP = 3;
         }

    }
}
