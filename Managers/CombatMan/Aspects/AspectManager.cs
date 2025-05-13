using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Managers.CombatMan.CombatAttacks;
using PlayingAround.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PlayingAround.Managers.CombatMan.Aspects
{
    public static class AspectManager
    {

        public static Dictionary<string, Aspect>  _aspectData;

        public static void LoadAspects()
        {
            _aspectData = JsonLoader.LoadAspectData();

        }
        public static void ResolveAspect(CombatMonster mon, TickedTiming timing)
        {
            foreach (var asp in mon.Aspects)
            {
                if (asp.WhenTicked != timing)
                    continue;

                if (asp.IsDamage)
                    AttackManager.ApplyDamage(asp.Damage, mon);

                asp.Duration -= 1;
            }

            mon.Aspects.RemoveAll(a => a.Duration <= 0);
            mon.AspectsResolved = true;
        }




        public static void ApplyAspect(CombatMonster target, SingleAttack attack, CombatMonster attacker = null)
        {
            string effect = attack.Effect;
            Aspect aspectTempl = _aspectData[effect];
            Aspect asp = new Aspect(aspectTempl)
            {
                Damage = aspectTempl.Damage,
            };

                target.Aspects.Add(asp);
            
            
        }

    }
}
