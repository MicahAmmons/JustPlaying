using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Stats
{
    public class PlayerStats
    {
        public int MovementSpeed {  get; set; }
        public float MaxHealth { get; set; }
        public float CurrentHealth { get; set; }
        public float MaxMana { get; set; }
        public float CurrentMana { get; set; }
        public float Initiation { get; set; }

        public PlayerStats() 
        {
            MovementSpeed = 4;
            MaxMana = 15;
            CurrentMana = MaxMana;
            MaxHealth = 25;
            CurrentHealth = MaxHealth;
            Initiation = 1;
        }

    }
}
