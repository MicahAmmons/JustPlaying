using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Managers.CombatMan.CombatAttacks;
using PlayingAround.Utils;

namespace PlayingAround.Managers
{
    public static class CombatMonsterManager
    {

        private static Dictionary<string, CombatMonster> _combatMonsterBaseData;
        private static float _difficultyIncreasePerLevel = 0.2f;
        private static float _hPIncreasePerLevel = 1f;
        private static float _elementalAffinityIncreasePerLevel = 0.01f;
        public static void LoadContent()
        {
            _combatMonsterBaseData = JsonLoader.LoadCombatMonsterData();

            foreach (var kvp in _combatMonsterBaseData)
            {
                string monsterKey = kvp.Key;
                CombatMonster mon = kvp.Value;

                mon.Name = $"{monsterKey}";
                mon.IconTextureKey = $"MonsterIcons/{monsterKey}Icon";
                mon.Attacks = AttackManager.GetAttacks(mon.AttackStrings);
                mon.Resistances = ResistanceManager.GetResistances(mon.ElementType);
            }



        }
        public static List<CombatMonster> GetCombatMonsters(List<string> monStrings)
        {
            List<CombatMonster> mons = new List<CombatMonster>();
            foreach (var mon in monStrings)
            {
                mons.Add(new CombatMonster(_combatMonsterBaseData[mon]));
            }
            return mons;
        }
        public static List<CombatMonster> BalanceCombatMonsters(List<CombatMonster> monsters, float max, float min)
        {
            List<CombatMonster> finalMon = new List<CombatMonster>();
            Random rng = new Random();
            float targetDifficulty = GetRandomDifficulty(rng, min, max);
            Dictionary<CombatMonster, List<float>> floats = GetRandomDifficulties(targetDifficulty, monsters);
                foreach (var kvp in floats)
                {
                    CombatMonster mon = kvp.Key;
                    foreach (var flo in kvp.Value)
                    {
                    CombatMonster monster = new CombatMonster(mon);
                    finalMon.Add(MatchStatsToDifficulty(monster, flo));
                    }
                }

          

            return finalMon;
        }
        private static CombatMonster MatchStatsToDifficulty(CombatMonster mon, float diff)
        {
            Random random = new Random();
            float numberOfLevels = (diff - mon.BaseDifficulty) / 2 + 1;
            mon.Level = numberOfLevels;

            do
            {
                bool addHealth = random.Next(2) == 0;
                if (addHealth)
                {
                    mon.BaseHealth += _hPIncreasePerLevel;
                    mon.MaxHealth = mon.BaseHealth;
                    mon.CurrentHealth = mon.BaseHealth;
                }
                else
                {
                    mon.ElementalAffinity += _elementalAffinityIncreasePerLevel;
                }
           
                numberOfLevels --;
            } while (numberOfLevels > 0);

            return mon;
        }
        public static float GetRandomDifficulty(Random rng, float min, float max)
        {
            // Convert range to number of 0.2 steps
            int stepsMin = (int)Math.Ceiling(min / 0.2f);
            int stepsMax = (int)Math.Floor(max / 0.2f);

            // Pick a random integer step
            int step = rng.Next(stepsMin, stepsMax + 1);

            // Convert back to float
            return step * 0.2f;
        }
        public static Dictionary<CombatMonster, List<float>> GetRandomDifficulties(float targetDiff, List<CombatMonster> mons)
        {
            Random rng = new Random();
            const float step = 0.2f;
            const float tolerance = 0.4f;

            Dictionary<CombatMonster, List<float>> result = new();
            float currentTotal = 0f;

            foreach (var mon in mons)
            {
                float baseDiff = mon.BaseDifficulty;
                result[mon] = new List<float> { baseDiff };
                currentTotal += baseDiff;
            }

            List<CombatMonster> editableMonsters = new List<CombatMonster>(mons);

            while (currentTotal + step <= targetDiff + tolerance)
            {
                var chosen = editableMonsters[rng.Next(editableMonsters.Count)];
                result[chosen].Add(step);
                currentTotal += step;
            }

            return result;
        }



        public static float GetMonsterDifficulty(CombatMonster mon)
        {
            return (mon.MP / 4f * 0.25f) + (mon.BaseHealth / 10f * 0.5f) + (mon.ElementalAffinity / 1f * 0.25f);
        }



    }
}
