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
    public class AspectManager
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

                if (!asp.IsBuff)
                    mon.CurrentHealth -= asp.Damage;

                asp.Duration -= 1;
            }

            mon.Aspects.RemoveAll(a => a.Duration <= 0);
        }




        public static void ApplyAspect(CombatMonster attacker, List<CombatMonster> target, SingleAttack attack)
        {
            string effect = attack.Effect;
            Aspect aspectTempl = _aspectData[effect];
            Aspect asp = new Aspect(aspectTempl)
            {
                Damage = aspectTempl.Damage,
            };

            foreach (var comMon in target)
            {
                comMon.Aspects.Add(asp);
            }
            
        }

    }
}
