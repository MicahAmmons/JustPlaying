using PlayingAround.AnimationFolder;
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
        public AttackName AttackName {  get; set; }
        public int BaseDamageMin {  get; set; }
        public int BaseDamageMax { get; set; }
        public int Range {  get; set; }
        public string Aspect { get; set; } = null;
        public List<CombatMonsterType> TargetType { get; set; }
        public ElementType ElementType { get; set; }
        public int AttackPerformedFrame { get; set; }
        public AnimationState AttackUpAnimation { get; set; }
        public AnimationState AttackDownAnimation { get; set; }
    }
}
