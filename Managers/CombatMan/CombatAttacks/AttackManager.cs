using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Entities.Monster.PlayMonsters;
using PlayingAround.Game.Map;
using PlayingAround.Managers.CombatMan.Aspects;
using PlayingAround.Managers.Movement.CombatGrid;
using PlayingAround.Utils;
using PlayingAround.Visuals;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PlayingAround.Entities.Monster.CombatMonsters.CombatMonster;
using static System.Net.Mime.MediaTypeNames;

namespace PlayingAround.Managers.CombatMan.CombatAttacks
{
    public static class AttackManager
    {


        private static Dictionary<string, SingleAttack> _attackData;

        public static void LoadContent()
        {
            _attackData = JsonLoader.LoadAttackData();
        }

        public static void PerformAttack(
           SingleAttack attack,
           CombatMonster attacker,
           List<CombatMonster> target,
           List<TileCell> affectedCells)

        {
            if (attack == null) return;
            foreach (var tar in target)
            {
                float damage = CalculateDamage(attack, tar);
                if (damage > 0)
                {
                    ApplyDamage(damage, tar);
                    ApplyAspect(attack, tar);
                }
               


            }
        }
        public static void ApplyAspect(SingleAttack attack, CombatMonster target)
        {
            switch (attack.WhenApplyAspect)
            {
                case "OnDamage":
                    AspectManager.ApplyAspect(target, attack);
                    break;
            }
        }
        public static void ApplyDamageVisualEffect(CombatMonster tar, float damage)
        {
            var effect = new VisualEffect(
                 tar.currentPos + new Vector2(0, -10),  // startPos
                 new Vector2(0, -20),                   // velocity
                 0.5f,
                 Color.Red,// duration
                 damage.ToString(),                     // text
                 null
             // texture (or some texture if needed)
             );
            CombatGuard.CurrentCombat.VisualEffectManager.AddEffect(effect);
            tar.IsFlashingRed = true;
            tar.DamageFlashTimer = 0.35f; // 0.35 seconds of red flash
        }
        public static void ApplyDamage(float damage, CombatMonster tar)
        {
            tar.CurrentHealth -= damage;
            ApplyDamageVisualEffect(tar, damage );
          
        }
        public static float CalculateDamage(SingleAttack attack, CombatMonster attacker)
        {
            float minDam = attack.MinDamage;
            float maxDam = attack.MaxDamage;
            string damageType = attack.ElementDamage.ToLower();
            Dictionary<string, float> resistances = attacker.Resistances;

            Random random = new Random();
            float baseDamage = random.Next((int)minDam, (int)maxDam + 1); 

            float resistanceMultiplier = resistances.TryGetValue(damageType, out float resistance) ? resistance : 1.0f;

            float finalDamage = baseDamage * resistanceMultiplier;

            return finalDamage;
        }



        public static List<SingleAttack> GetAttacks(List<string> atts)
        {
            List<SingleAttack> attacks = new List<SingleAttack>();
            foreach (var att in atts)
            {
                attacks.Add(_attackData[att]);
            }  
            return attacks;
        }

        public static (CombatMonster, List<TileCell>) TargetClosestEnemy(List<TileCell> inRangeCells, TileCell origin)
        {
            CombatMonster closestMon = null;
            TileCell closestCell = null;
            int shortestDistance = int.MaxValue;
            (CombatMonster, List<TileCell>) result = new();
            Dictionary<CombatMonster, TileCell> playerMonsters = CombatGuard.CurrentCombat.PlayerControlledMonsterMap;
            Dictionary<CombatMonster, TileCell> aiMonsters = CombatGuard.CurrentCombat.AIControlledMonsterMap;


            foreach (var kvp in playerMonsters)
            {
                CombatMonster mon = kvp.Key;
                TileCell cell = kvp.Value;

                if (inRangeCells.Contains(cell))
                {
                    int distance = GridMovement.CheckManhattanDistance(origin, cell);

                    if (distance < shortestDistance)
                    {
                        shortestDistance = distance;
                        closestMon = mon;
                        closestCell = cell;
                    }
                }
            }
            if (closestMon != null && closestCell != null)
            {
                result = (closestMon, new List<TileCell> { closestCell });
            }
            return result;
        }
        public static Dictionary<CombatMonster, List<TileCell>> GetAttackSpecificBehavior(string targetPhrase, string key, List<TileCell> inRangeCells, TileCell origin)
        
            {
                Dictionary<CombatMonster, List<TileCell>> result = new();
                Dictionary<CombatMonster, TileCell> playerMonsters = CombatGuard.CurrentCombat.AIControlledMonsterMap;
                Dictionary < CombatMonster, TileCell > aiMonsters = CombatGuard.CurrentCombat.PlayerControlledMonsterMap;
            if (key == "Target")
                {
                    switch (targetPhrase)
                    {
                        case "closestEnemy":
                            CombatMonster closestMon = null;
                            TileCell closestCell = null;
                            int shortestDistance = int.MaxValue;

                            

                            foreach (var kvp in playerMonsters)
                            {
                                CombatMonster mon = kvp.Key;
                                TileCell cell = kvp.Value;

                                if (inRangeCells.Contains(cell))
                                {
                                    int distance = GridMovement.CheckManhattanDistance(origin, cell);

                                    if (distance < shortestDistance)
                                    {
                                        shortestDistance = distance;
                                        closestMon = mon;
                                        closestCell = cell;
                                    }
                                }
                            }
                            if (closestMon != null && closestCell != null)
                            {
                                result[closestMon] = new List<TileCell> { closestCell };
                            }
                            break;
                    case "lowestHPInRange":
                        float lowestHP = int.MaxValue;
                        CombatMonster lowestHPMon = null;
                        TileCell lowestHPCell = null;   
                        foreach (var kvp in playerMonsters)
                        {
                            CombatMonster mon = kvp.Key;
                            TileCell cell = kvp.Value;
                            if (inRangeCells.Contains(cell))
                            {
                                float hp = mon.CurrentHealth;
                                if (hp < lowestHP)
                                {
                                    lowestHP = hp;
                                    lowestHPCell = cell;
                                    lowestHPMon = mon;
                                }
                            }
                        }
                        if (lowestHPMon != null && lowestHPCell != null)
                        {
                            result[lowestHPMon] = new List<TileCell> { lowestHPCell };
                        }
                        break;
                }
                }
                return result;
            }

        // IF THE MOSNTER HAS MULTIPLE ATTACKS WITHIN RANGE TO USE, THIS METHOD DECIDES WHICH ONE
        public static (SingleAttack, Dictionary<CombatMonster, List<TileCell>>) ChooseWhichAttack(
      Dictionary<SingleAttack, Dictionary<CombatMonster, List<TileCell>>> attacks,
      TileCell origin,
      Queue<ChooseWhichMonsterAttack> key)
        {
            if (attacks == null || attacks.Count == 0)
                return (null, new Dictionary<CombatMonster, List<TileCell>>());

            if (attacks.Count == 1)
                return (attacks.First().Key, attacks.First().Value);

            Queue<ChooseWhichMonsterAttack> strategyQueue = new Queue<ChooseWhichMonsterAttack>(key); // clone it

            while (strategyQueue.Count > 0)
            {
                ChooseWhichMonsterAttack strategy = strategyQueue.Dequeue();

                switch (strategy)
                {
                    case ChooseWhichMonsterAttack.ShortestRange:
                        int shortestRange = int.MaxValue;
                        SingleAttack chosenShortest = null;

                        foreach (var attack in attacks.Keys)
                        {
                            if (attack.Range < shortestRange)
                            {
                                shortestRange = attack.Range;
                                chosenShortest = attack;
                            }
                        }

                        if (chosenShortest != null)
                            return (chosenShortest, attacks[chosenShortest]);

                        break;

                    // case ChooseWhichMonsterAttack.HighestDamage:
                    // case ChooseWhichMonsterAttack.PriorityAttackName:
                    // Add more cases here later

                    default:
                        continue; // move to next strategy
                }
            }

            // If no strategy matched
            return (null, new Dictionary<CombatMonster, List<TileCell>>());
        }
    }
}
