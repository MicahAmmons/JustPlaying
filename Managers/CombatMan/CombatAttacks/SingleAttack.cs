
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Game.Map;
using PlayingAround.Interfaces;
using PlayingAround.Managers.Assets;
using PlayingAround.Managers.Tiles;
using PlayingAround.Visuals;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PlayingAround.Managers.CombatMan.CombatAttacks
{
    public class SingleAttack
    {
        public AttackName Name { get; set; }
        public Texture2D Icon { get; set; }
        public ElementType ElementDamage { get; set; } = ElementType.None;
        public int Range { get; set; }
        public string Aspect { get; set; }
        public int MinDamage { get; set; }
        public int MaxDamage { get; set; }
        public float VisualVelocity { get; set; }
        public bool Animated { get; set; } = false;
         public VisualTiming VisualTiming { get; set; } 
        public string WhenApplyAspect { get; set; }
        public Dictionary<ICombatant, TileCell> ActiveTargetMap { get; set; } 
        public List<CombatMonsterType> TargetType { get; set; } = new List<CombatMonsterType>();
        public VisualEffect Visual {  get; set; }
        public List<TileCell> CellsWithinRange { get; set; } = new List<TileCell>();
       
        public SingleAttack (AttackName name, SingleAttackData data, ElementType element = ElementType.None)
        {
            Name = name;
            ElementDamage = element == ElementType.None ? ElementType.Normal : element;
            Range = data.Range;
            Aspect = data.Aspect;
            MinDamage = data.BaseDamageMin;
            MaxDamage = data.BaseDamageMax;
            VisualVelocity = data.VisualVelocity > 0? data.VisualVelocity: 200f;
            Animated = data.Animated;
            VisualTiming = data.VisualTiming;
            WhenApplyAspect = data.WhenApplyEffect;
            if (data.TargetType.Count > 0)
            {
                foreach (var tar in data.TargetType)
                {
                    TargetType.Add(tar);
                }
            }
            if (data.AttackHasIcon)
                Icon = AssetManager.GetTexture($"{Name}");
            ActiveTargetMap = new Dictionary<ICombatant, TileCell>();
        }
        public void UpdateTargetMap(TileCell currentCell)
        {
            ActiveTargetMap.Clear();
            var playerMap = CombatGuard.CurrentCombat.PlayerControlledMonsterMap;
            var monsterMap = CombatGuard.CurrentCombat.AIControlledMonsterMap;
            if (TargetType.Contains(CombatMonsterType.Player))
            {
                foreach (var kvp in playerMap)
                {
                    ICombatant combatant = kvp.Key;
                    TileCell cell = kvp.Value;
                    if (TileManager.CheckManhattanDistance(currentCell, cell) <= Range)
                    {
                        ActiveTargetMap[combatant] = cell ;
                    }
                }
                return;
            }
            if (TargetType.Contains(CombatMonsterType.Player))
            {

                return;
            }
            if (TargetType.Contains(CombatMonsterType.Player))
            {

                return;
            }
        }

        public void SetAttackRangeOptions(TileCell cell)
        {
            List<TileCell> cells = TileManager.GetFloodFillTileWithinRange(cell, Range, true);
            CellsWithinRange = cells;
        }
    }

}

public enum AttackName
{
    Slam,
    Spit,

}

