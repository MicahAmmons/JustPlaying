using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Managers.Assets;
using PlayingAround.Managers.CombatMan.CombatAttacks;
using PlayingAround.Managers.Resistances;
using PlayingAround.Utils;

namespace PlayingAround.Managers.Entities
{
    public static class CombatMonsterManager
    {
        private static Dictionary<string, CombatMonsterData> _combatMonsterBaseData;
        private static float _difficultyIncreasePerLevel = 0.2f;
        private static float _hPIncreasePerLevel = 1f;
        private static float _elementalAffinityIncreasePerLevel = 1f;

        public static void LoadContent()
        {
            _combatMonsterBaseData = JsonLoader.LoadCombatMonsterData();
        }
    
        public static List<CombatMonster> GetCombatMonsters(List<string> monStrings)
        {

            List<CombatMonster> mons = new List<CombatMonster>();
            foreach (var mon in monStrings)
            {
                var dataCopy = DeepCopyHelper.DeepCopy(_combatMonsterBaseData[mon]);
                mons.Add(new CombatMonster(dataCopy));
            }
            return mons;
        }
        public static CombatMonster SummonMonsterToCombat(string name)
        {
            var dataCopy = DeepCopyHelper.DeepCopy(_combatMonsterBaseData[name]);
            CombatMonster newSummonedMon = new CombatMonster(dataCopy)
            {
                MonsterIs = CombatMonsterType.Summoned
            };
            return newSummonedMon;
        }
       
        public static Vector2 GetMonsterWidthAndHeight(string name)
        {
            return new Vector2(_combatMonsterBaseData[name].DrawSpecifics.Width, _combatMonsterBaseData[name].DrawSpecifics.Height);
        }
    }
}
