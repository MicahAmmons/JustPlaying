using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Interfaces;
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

        public static Dictionary<string, AspectData>  _aspectData;

        public static void LoadAspects()
        {
            _aspectData = JsonLoader.LoadAspectData();

        }
        public static void ResolveAspect(ICombatant mon, TickedTiming timing)
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
        }




        public static Aspect GetAspect(string effect, ElementType element = ElementType.None)
        {
            AspectData aspectTempl = _aspectData[effect];
            var aspectCopy = DeepCopyHelper.DeepCopy(aspectTempl);
            Aspect asp = new    Aspect(effect, aspectCopy)
            {
                
            };

            return asp;
            
            
        }

    }
}
