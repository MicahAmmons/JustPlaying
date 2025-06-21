using Microsoft.Xna.Framework;
using PlayingAround.AnimationFolder;
using PlayingAround.Data.SaveData;
using PlayingAround.Game.Map;
using PlayingAround.Interfaces;
using PlayingAround.Managers.CombatMan.CombatAttacks;
using System.Collections.Generic;
using System.ComponentModel;

namespace PlayingAround.Entities.Monster.CombatMonsters
{
    public class CombatMonsterData
    {

        public string UniqueId { get; set; }
        public ElementType DefaultElementType { get; set; }
        public BaseCombatStats BaseStats {  get; set; }
        public DrawSpecificStats DrawSpecifics { get; set; } 
        public Dictionary<AnimationState, int[]> AnimationData { get; set; }
        public Dictionary<AttackName, ElementType> AttackData { get; set; }
        public List<MonsterActionOrder> ActionOrder { get; set; }
        public List<ChooseWhichMonsterAttack> DecideWhichAttack { get; set; }
    }
    public class BaseCombatStats
    {
        public int MP { get; set; }
        public int AP { get; set; }
        public int Health { get; set; }
        public int Initiative { get; set; }
        public Dictionary<ElementType, float> Resistances { get; set; }
        public Queue<ChooseWhichMonsterAttack> DecideWhichAttack { get; set; }
        public Queue<MonsterActionOrder> ActionOrder { get; set; }


    }
    public class OutOfCombatAnimatedStats
    {
        public Vector2 CurrentPos { get; set; }
        public bool IsPaused { get; set; } = false;
        public float PauseTimer { get; set; } = 0f;
        public float PauseDurationMax { get; set; } = 0f;
        public float PauseDurationMin { get; set; } = 0f;
        public float CurrentPauseDuration { get; set; } = 0f;
    }
    public class DrawSpecificStats
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int MovementQuickness { get; set; }
        public MovementPatternType MovementPattern {  get; set; }
        public bool IsFlashingRed = false;
        public float DamageFlashTimer = 0f;
        public bool AllowedToMove = false;
    }
    public class CurrentCombatStats
    {
        public int MP { get; set; }
        public int AP { get; set; }
        public int Health { get; set; }
        public Vector2 Pos { get; set; } = new Vector2();
        public Dictionary<ElementType, float> Resistances { get; set; }
        public List<Vector2> AttackPath1 { get; set; } = null;
        public List<Vector2> AttackPath2 { get; set; } = null;
        public SingleAttack Attack { get; set; } = null;
        public List<ICombatant> AttackEffectedCombatants { get; set; } = null;
        public List<TileCell> AttackEffectedCells { get; set; } = null;
        public List<TileCell> AttackRange {  get; set; } = null;
        public TileCell MovementEndPoint { get; set; } = null;
        public List<Vector2> MovePath { get; set; } = new();
        public Queue<MonsterActionOrder> ActionOrder { get; set; } = new Queue<MonsterActionOrder>();
        public Queue<ChooseWhichMonsterAttack> ChooseWhichAttack { get; set; } = new Queue<ChooseWhichMonsterAttack>();
        public ElementType ElementType { get; set; }
        public (string name, SummonedSavedStats data)? CurrentSelectedSummon {  get; set; }


    }
}