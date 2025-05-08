using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Entities.Monster.PlayMonsters;
using PlayingAround.Entities.Summons;
using PlayingAround.Game.Map;
using PlayingAround.Managers.CombatMan.Aspects;
using PlayingAround.Managers.CombatMan.CombatAttacks;
using PlayingAround.Managers.Movement.CombatGrid;
using PlayingAround.Stats;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PlayingAround.Entities.Monster.CombatMonsters
{
     public class CombatMonster
    {
        public float Difficulty { get; set; }
        public string IconTextureKey { get; set; }

        public Vector2 currentPos;

        public Vector2 startingPos;
        public int SP { get; set; }
        [JsonPropertyName("mp")] public int MP { get; set; }
        [JsonPropertyName("movementQuickness")] public float MovementQuickness { get; set; }
        [JsonPropertyName("chooseAttackBehavior")] public string ChooseAttackBehavior { get; set; } // add number of cells moved

        [JsonPropertyName("turnBehavior")] public string TurnBehavior { get; set; }
        [JsonPropertyName("movementPattern")] public string MovementPattern { get; set; }
        [JsonPropertyName("attacks")] public List<string> AttackStrings { get; set; }

        [JsonPropertyName("monsterType")] public string MonsterType { get; set; }

        [JsonPropertyName("baseDifficulty")] public float BaseDifficulty { get; set; }

        [JsonPropertyName("elementType")] public string ElementType { get; set; }

        [JsonPropertyName("baseHealth")] public float BaseHealth { get; set; }

        [JsonPropertyName("initiation")] public float Initiation { get; set; }
        [JsonPropertyName("elementalAffinity")] public float ElementalAffinity { get; set; }
        public bool isPlayerControled { get; set; } = false;
        public List<SingleAttack> Attacks { get; set; }
        public Dictionary<string, float> Resistances { get; set; }
        public float Level { get; set; } = 1;
        public Texture2D IconTexture { get; set; }
        public bool isPlayerMovementControled { get; set; }

        public bool IsSummon {  get; set; }
        public List<Vector2> attackPath1 { get; set; } = null;
        public List<Vector2> attackPath2 { get; set; } = null;
        public List<Vector2> projectileAttackPath1 { get; set; } = null;
        public Texture2D projectileTexture { get; set; } = null;
        public List<Vector2> projectileMovePath {  get; set; } = null;
        public SingleAttack CurrentAttack { get; set; } = null;
        public List<CombatMonster> CurrentAttackEffectedMonsters { get; set; } = null;
        public List<TileCell> CurrentAttackEffectedCells { get; set; } = null;
        public bool IsFlashingRed;
        public float DamageFlashTimer = 0f;
        public bool AllowedToMove = true;

        public CombatMonster(SummonedMonster mon, CombatMonster comMon)
        {
            BaseSummonCost = mon.SummonCost;
            Name = mon.Name;
            IconTextureKey = mon.IconTextureString;
            MaxHealth = mon.MaxHealth;
            CurrentHealth = mon.MaxHealth;
            Level =  mon.Level;
            isPlayerControled = true;
            isPlayerMovementControled = true;
            Attacks = comMon.Attacks;
            BaseDifficulty = comMon.BaseDifficulty;
            ElementType = comMon.ElementType;
            Initiation = comMon.Initiation;
            MP = comMon.MP;
            MonsterType = comMon.MonsterType;
            MovementPattern = comMon.MovementPattern;
            MovementQuickness = comMon.MovementQuickness;
            Resistances = comMon.Resistances;
            IsSummon = true;

        }


        public TileCell CurrentCell { get; set; }
        public int TurnNumber { get; set; } = 0;
        public Queue<string> OrderOfActions { get; set; }
         public float MaxHealth { get; set; }
        public float MaxMana { get; set; }
        public float CurrentMana { get; set; }
        public float CurrentHealth { get; set; }
        public int ID { get; set; }
        public bool PathGenerated { get; set; } = false;
        public TileCell PlayerMovementEndPoint { get; set; }
        public List<Vector2> MovePath { get; set; } = new();
        public int BaseSummonCost { get; set; }
        public float AttackPower { get; set; } = 1;
        public string Name { get; set; }
        public string NamePlusLevel { get; set; }
        public List<Aspect> Aspects { get; set; } = new List<Aspect>();


        public CombatMonster()
        {

        }

        public CombatMonster(CombatMonster original)
        {
            Difficulty = original.Difficulty;
            IconTextureKey = original.IconTextureKey;
            MovementQuickness = original.MovementQuickness;
            isPlayerControled = original.isPlayerControled;
            TurnBehavior = original.TurnBehavior;
            MovementPattern = original.MovementPattern;
            ChooseAttackBehavior = original.ChooseAttackBehavior;
            BaseHealth = original.BaseHealth;
            BaseDifficulty = original.BaseDifficulty;
            Initiation = original.Initiation;
            ElementType = original.ElementType;
            Attacks = original.Attacks;
            Name = original.Name;
            MaxHealth = original.BaseHealth;
            CurrentHealth = original.BaseHealth;
            MP = original.MP;
            MonsterType = original.MonsterType;
            Resistances = original.Resistances;
            ElementalAffinity = original.ElementalAffinity;
            BaseSummonCost = original.BaseSummonCost;

        }

        public CombatMonster(Player.Player player)
        {
            PlayerStats stats = player.stats;
            //Speed = stats.MovementSpeed;
            MaxHealth = stats.MaxHealth;
            CurrentHealth = stats.CurrentHealth;
            Name = "Player";
            MaxMana = stats.MaxMana;
            CurrentMana = stats.CurrentMana;
            isPlayerControled = true;
            AttackPower = 1;
            Initiation = stats.Initiation;
            MovementQuickness = 200f;
            MovementPattern = "straight";
            MP = player.stats.MP;
            SP = player.stats.SP;
        }



        //public CombatMonster (Summoner summon)
        //{

        //    isPlayerControled = true;
        //}
    }
}
