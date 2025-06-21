using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PlayingAround.Managers.CombatMan.Aspects
{
    public class AspectData
    {

        public string Description { get; set; }
        public float Duration { get; set; }
        public float Damage { get; set; }
        public ElementType DefaultElement { get; set; }
        public bool IsBuff { get; set; }
        public TickedTiming WhenTicked { get; set; }
        public bool IsStackable { get; set; }
        public bool IsDamage { get; set; }
    }
}