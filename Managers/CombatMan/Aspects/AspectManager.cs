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
        public static Aspect GetAspect(string effect, ElementType element = ElementType.None)
        {
            AspectData aspectTempl = _aspectData[effect];
            var aspectCopy = DeepCopyHelper.DeepCopy(aspectTempl);
            Aspect asp = new    Aspect(effect, aspectCopy)
            {
                
            };

            return asp;
            
            
        }

        public static bool IsAspectUnlocked(string aspectName)
        {
            // Place holder for how to track if the player has unlocked Aspect 
            return false;
        }
    }
}
