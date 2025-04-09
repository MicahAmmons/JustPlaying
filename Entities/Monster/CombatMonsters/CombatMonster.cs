using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Entities.Monster.CombatMonsters
{
     public class CombatMonster
    {
        public float Difficulty { get; set; }
        public string IconPath { get; set; }
        public string Name { get; set; }

        public CombatMonster() 
        {


        }
    }
}
