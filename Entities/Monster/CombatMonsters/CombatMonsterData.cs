using Microsoft.Xna.Framework;
using PlayingAround.AnimationFolder;
using PlayingAround.Data.SaveData;
using PlayingAround.Game.Map;
using PlayingAround.Interfaces;
using PlayingAround.Managers.Assets;
using PlayingAround.Managers.CombatMan;
using PlayingAround.Managers.CombatMan.ActionLibrary;
using PlayingAround.Managers.CombatMan.CombatAttacks;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace PlayingAround.Entities.Monster.CombatMonsters
{
    public class CombatMonsterData
    {

        public string UniqueId { get; set; }
        public ElementType DefaultElementType { get; set; }
        public BaseCombatStats BaseStats {  get; set; }
        public DrawSpecificStats DrawSpecifics { get; set; } = new DrawSpecificStats() { IsFlashingRed = false};
        public AnimationData AnimationData { get; set; }
        public Dictionary<AttackName, ElementType> AttackData { get; set; }
        public List<AiAction> ActionOrder { get; set; } 
    }
    public class BaseCombatStats
    {
        public int MP { get; set; }
        public int AP { get; set; }
        public int Health { get; set; }
        public int Initiative { get; set; }
        public Dictionary<ElementType, float> Resistances { get; set; }
        public List<AiAction> ActionOrder { get; set; }
    }
    public class OutOfCombatAnimatedStats
    {
        public bool IsPaused { get; set; } = true;
        public float PauseTimer { get; set; } = 0f;
        public float PauseDurationMax { get; set; } = 0f;
        public float PauseDurationMin { get; set; } = 0f;
        public float CurrentPauseDuration { get; set; } = 0f;

    }
    public class DrawSpecificStats
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public VisualEffectManager VEManager { get; set; } = new VisualEffectManager();
        public MovementPatternType MovementPattern {  get; set; }
        public float MovementQuickness { get; set; }
        public bool IsFlashingRed = false;
        public float DamageFlashTimer = 0f;

        public bool DrawCellHightlight = false;
        public int shrink = 0;
        public Color HighlightCol = ColorPalette.DarkColor; 
    }
    public class CurrentCombatStats
    {
        public int MP { get; set; }
        public int AP { get; set; }
        public int Health { get; set; }
        public Dictionary<ElementType, float> Resistances { get; set; }
        public List<Vector2> AttackPath1 { get; set; } = null;
        public List<Vector2> AttackPath2 { get; set; } = null;
        public SingleAttack Attack { get; set; } = null;
        public ICombatant AttackEffectedCombatants { get; set; } = null;
        public TileCell AttackEffectedCells { get; set; } = null;
        public List<AiAction> Actions { get; set; }
        public ElementType ElementType { get; set; }
        public (string name, SummonedSavedStats data)? CurrentSelectedSummon {  get; set; }

    }
}