using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Entities.Monster.PlayMonsters;
using PlayingAround.Game.Map;
using PlayingAround.Interfaces;
using PlayingAround.Managers.Assets;
using PlayingAround.Managers.CombatMan.Aspects;
using PlayingAround.Managers.Movement;
using PlayingAround.Utils;
using PlayingAround.Visuals;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using static PlayingAround.Entities.Monster.CombatMonsters.CombatMonster;
using static System.Net.Mime.MediaTypeNames;

namespace PlayingAround.Managers.CombatMan.CombatAttacks
{
    public static class AttackManager
    {


        private static Dictionary<AttackName, SingleAttackData> _attackData;

        public static void LoadContent()
        {

            _attackData = JsonLoader.LoadAttackData();
           
        }
        public static SingleAttack GetAttack(AttackName name, ElementType element = ElementType.None)
        {
            var dataCopy = DeepCopyHelper.DeepCopy(_attackData[name]);
            return new SingleAttack(name, dataCopy, element);

        }
        public static void PerformAttack(SingleAttack attack, ICombatant target)
        {
            float damage = CalculateDamage(attack);
            target.ApplyDamage(damage, attack.ElementDamage);
            if (attack.Aspect != null)
            target.ApplyAspect(attack.Aspect, attack.ElementDamage);  
        }

        public static float CalculateDamage(SingleAttack attack)
        {
            float minDam = attack.MinDamage;
            float maxDam = attack.MaxDamage;
            float finalDamage = float.MaxValue;

            finalDamage = RandomHut.rng.Next((int)minDam, (int)maxDam + 1);
            return finalDamage;
        }
    }
}
