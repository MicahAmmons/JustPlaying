using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Stats
{
    public class PlayerStats
    {
        public int speed {  get; set; }
        public float mana {  get; set; }
        public float health { get; set; }

        public PlayerStats() 
        {
            speed = 1;
            mana = 15;
            health = 25;
        }

    }
}
