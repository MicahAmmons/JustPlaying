using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Managers.CombatMan.Aspects;
using PlayingAround.Visuals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Managers.CombatMan.CombatAttacks
{
    public class SingleAttackData
    {

        public int BaseDamageMin {  get; set; }
        public int BaseDamageMax { get; set; }
        public bool AttackHasIcon { get; set; } = false;
        public int Range {  get; set; }
        public string Aspect { get; set; }
        public string Strength { get; set; }
        public string WhenApplyEffect { get; set; }
        public VisualTiming VisualTiming { get; set; }
        public int VisualVelocity { get; set; }
        public bool Animated { get; set; }
        public List<CombatMonsterType> TargetType { get; set; }


    }
}
