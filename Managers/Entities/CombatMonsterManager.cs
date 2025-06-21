using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Interfaces;
using PlayingAround.Managers.Assets;
using PlayingAround.Managers.CombatMan;
using PlayingAround.Utils;

namespace PlayingAround.Managers.Entities
{
    public static class CombatMonsterManager
    {
        private static Dictionary<string, CombatMonsterData> _combatMonsterBaseData;
        private static Queue<ICombatant> _currentCombatMonsteres => CombatGuard.CurrentCombat.TurnOrder;

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
                Is = CombatMonsterType.Summoned
            };
            return newSummonedMon;
        }
       
        public static Vector2 GetMonsterWidthAndHeight(string name)
        {
            return new Vector2(_combatMonsterBaseData[name].DrawSpecifics.Width, _combatMonsterBaseData[name].DrawSpecifics.Height);
        }
        public static void Update(GameTime gameTime)
        {
            foreach (var mon in _currentCombatMonsteres)
            {
                if (mon.Is == CombatMonsterType.AI)
                {
                    mon.UpdateAnimation(gameTime);
                }
            }
        }

        public static void UpdateAllMovement(GameTime gameTime)
        {
            foreach (var mon in _currentCombatMonsteres)
            {
                if (mon.Is ==CombatMonsterType.Player || mon.CurrentStats.MovePath == null || mon.CurrentStats.MovePath.Count <= 0 || !mon.DrawSpecifics.AllowedToMove) continue;
                mon.UpdateMovement(gameTime);
            }
        }
    }
}
