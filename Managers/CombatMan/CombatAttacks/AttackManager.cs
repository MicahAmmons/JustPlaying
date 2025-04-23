using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Game.Map;
using PlayingAround.Managers.Movement.CombatGrid;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Managers.CombatMan.CombatAttacks
{
    public static class AttackManager
    {


        public static void PerformAttack(
           SingleAttack attack,
           CombatMonster attacker,
           List<CombatMonster> target,
           List<TileCell> affectedCells)

        {
            foreach (var tar in target)
            {
                if (AttackHits(attack, attacker, tar))
                {
                    
                    tar.CurrentHealth -= CalculateDamage(attack, tar);
                }
            }


        }

        public static float CalculateDamage(SingleAttack attack, CombatMonster attacker)
        {
            return attack.Damage;
        }
        public static bool AttackHits(SingleAttack att, CombatMonster attacker, CombatMonster target)
        {
            return true;
        }

        public static Dictionary<CombatMonster, List<TileCell>> GetAttackSpecificBehavior(string targetPhrase, string key, List<TileCell> inRangeCells, TileCell origin)
        
            {
                Dictionary<CombatMonster, List<TileCell>> result = new();
                Dictionary<CombatMonster, TileCell> playerMonsters = CombatManager.GetCombatMonMap("player");
                Dictionary < CombatMonster, TileCell > aiMonsters = CombatManager.GetCombatMonMap("ai");
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
        public static (SingleAttack, Dictionary<CombatMonster, List<TileCell>>) GetAttackSpecificBehavior(Dictionary<SingleAttack, Dictionary<CombatMonster, List<TileCell>>> attacks, TileCell origin, string chooseAttackPhrase)
             {
            if (attacks.Count == 1)
            {
                return (attacks.First().Key, attacks.First().Value);
            }
            switch (chooseAttackPhrase)
            {
                case "shortestRange":
                    int shortestRange = int.MaxValue;
                    SingleAttack chosenAttack = null;

                    foreach (var attack in attacks.Keys)
                    {
                        if (attack.Range < shortestRange)
                        {
                            shortestRange = attack.Range;
                            chosenAttack = attack;
                        }
                    }
                        return (chosenAttack, attacks[chosenAttack]);
            }
            return (null, new Dictionary<CombatMonster, List<TileCell>>());
        }

    }
}
