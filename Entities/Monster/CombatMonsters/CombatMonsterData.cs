using Microsoft.Xna.Framework;
using PlayingAround.AnimationFolder;
using PlayingAround.Game.Map;
using PlayingAround.Managers.CombatMan.CombatAttacks;
using System.Collections.Generic;

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
    public class PlayerCombatStats
    {

    }
    public class CurrentCombatStats
    {
        public int MP { get; set; }
        public int AP { get; set; }
        public int Health { get; set; }
        public List<Vector2> AttackPath1 { get; set; } = null;
        public List<Vector2> AttackPath2 { get; set; } = null;
        public SingleAttack Attack { get; set; } = null;
        public List<CombatMonster> AttackEffectedMonsters { get; set; } = null;
        public List<TileCell> AttackEffectedCells { get; set; } = null;
        public Queue<MonsterActionOrder> ActionOrder { get; set; } = new Queue<MonsterActionOrder>();
        public Queue<ChooseWhichMonsterAttack> ChooseWhichAttack { get; set; } = new Queue<ChooseWhichMonsterAttack>();
    }
}