using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Entities.Monster.PlayMonsters;
using PlayingAround.Entities.Summons;
using PlayingAround.Game.Map;
using PlayingAround.Managers.Assets;
using PlayingAround.Managers.CombatMan.Aspects;
using PlayingAround.Managers.CombatMan.CombatAttacks;
using PlayingAround.Managers.Entities;
using PlayingAround.Managers.Movement.CombatGrid;
using PlayingAround.Stats;
using System;
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
        public int CurrentSP;
        [JsonPropertyName("mp")] public int MP { get; set; }
        [JsonPropertyName("movementQuickness")] public float MovementQuickness { get; set; }
        [JsonPropertyName("chooseAttackBehavior")] public string ChooseAttackBehavior { get; set; } // add number of cells moved

        [JsonPropertyName("movementPattern")] public string MovementPattern { get; set; }
        [JsonPropertyName("attacks")] public List<string> AttackStrings { get; set; }

        [JsonPropertyName("monsterType")] public string MonsterType { get; set; }

        [JsonPropertyName("baseDifficulty")] public float BaseDifficulty { get; set; }

        [JsonPropertyName("elementType")] public string ElementType { get; set; }

        [JsonPropertyName("baseHealth")] public float BaseHealth { get; set; }

        [JsonPropertyName("initiation")] public float Initiation { get; set; }
        [JsonPropertyName("elementalAffinity")] public float ElementalAffinity { get; set; }
        [JsonPropertyName("actionOrder")]public List<string> ActionOrderList { get; set; }

        [JsonPropertyName("decideWhichAttack")] public List<string> ChooseWhichAttacks { get; set; }
        [JsonPropertyName("width")] public int Width { get; set; }
        [JsonPropertyName("height")] public int Height { get; set; }
        [JsonPropertyName("uniqueId")] public string UniqueId { get; set; }

        public Queue<MonsterActionOrder> BaseOrderOfActions { get; set; }
        public Queue<ChooseWhichMonsterAttack> BaseChooseWhichAttack { get; set; }
        public Queue<MonsterActionOrder> CurrentOrderOfActions {  get; set; } = new Queue<MonsterActionOrder>();
        public Queue<ChooseWhichMonsterAttack> CurrentChooseWhichAttack {  get; set; } = new Queue<ChooseWhichMonsterAttack> ();
        public float CurrentMP;
        public bool CurrentIsPlayerControlled;


        public bool isPlayer { get; set; } = false;
        public bool isSummoned { get; set; } = false;
        public bool isMonster { get; set; } = false;
        public List<SingleAttack> Attacks { get; set; }
        public Dictionary<string, float> Resistances { get; set; }
        public float Level { get; set; } = 1;
        public Texture2D IconTexture { get; set; }
        public bool isPlayerMovementControled { get; set; }

        public bool IsSummon {  get; set; }
        public List<Vector2> attackPath1 { get; set; } = null;
        public List<Vector2> attackPath2 { get; set; } = null;
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
            //Passes in a copy 
            //Summoned monster
            BaseSummonCost = mon.SummonCost;
            Name = mon.Name;
            IconTextureKey = comMon.IconTextureKey;
            IconTexture = AssetManager.GetTexture(mon.IconTextureString);
            BaseHealth = comMon.BaseHealth;
            CurrentHealth = comMon.BaseHealth;
            Level =  mon.Level;
            isSummoned = true;
            isPlayerMovementControled = true;
            Attacks = comMon.Attacks;
            BaseDifficulty = comMon.BaseDifficulty;
            ElementType = comMon.ElementType;
            Initiation = comMon.Initiation;
            MP = comMon.MP;
            CurrentMP = comMon.MP;
            MonsterType = comMon.MonsterType;
            MovementPattern = comMon.MovementPattern;
            MovementQuickness = comMon.MovementQuickness;
            Resistances = comMon.Resistances;
            IsSummon = true;
            isMonster = false;
            isPlayer = false;
            isDead = false;
            Width = comMon.Width;
            Height = comMon.Height;
            UniqueId = comMon.UniqueId;


        }


//public TileCell CurrentCell { get; set; }
        public int TurnNumber { get; set; } = 0;
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
        public bool isDead { get; set; } = false;



        public CombatMonster()
        {

        }

        public CombatMonster(CombatMonster original)
        {
            //Monster monster CombatMonster CombatMONster
            Difficulty = original.Difficulty;
            IconTextureKey = original.IconTextureKey;
            MovementQuickness = original.MovementQuickness;
            isMonster = true;
            MovementPattern = original.MovementPattern;
            ChooseAttackBehavior = original.ChooseAttackBehavior;
            BaseHealth = original.BaseHealth;
            BaseDifficulty = original.BaseDifficulty;
            Initiation = original.Initiation;
            ElementType = original.ElementType;
            Attacks = original.Attacks;
            Name = original.Name;
            BaseHealth = original.BaseHealth;
            CurrentHealth = original.BaseHealth;
            MP = original.MP;
            MonsterType = original.MonsterType;
            Resistances = original.Resistances;
            ElementalAffinity = original.ElementalAffinity;
            BaseSummonCost = original.BaseSummonCost;
            ActionOrderList = original.ActionOrderList;
            ChooseWhichAttacks = original.ChooseWhichAttacks;
            Width = original.Width;
            Height = original.Height;
            UniqueId = original.UniqueId;
            BaseOrderOfActions = ConvertStringOrderOfActionToEnum(original.ActionOrderList);
            BaseChooseWhichAttack = ConvertStringWhichAttackToEnum(original.ChooseWhichAttacks);

        }
        private Queue<MonsterActionOrder> ConvertStringOrderOfActionToEnum(List<string> orderStri)
        {
            var queue = new Queue<MonsterActionOrder>();
            foreach (var str in orderStri)
            {
                if (Enum.TryParse(typeof(MonsterActionOrder), str, true, out var result))
                {
                    queue.Enqueue((MonsterActionOrder)result);
                }


            }

            return queue;
        }

        private Queue<ChooseWhichMonsterAttack> ConvertStringWhichAttackToEnum(List<string> orderStri)
        {
            var queue = new Queue<ChooseWhichMonsterAttack>();

                foreach (var str in orderStri)
                {
                    if (Enum.TryParse(typeof(ChooseWhichMonsterAttack), str, true, out var result))
                    {
                        queue.Enqueue((ChooseWhichMonsterAttack)result);
                    }
                
            }

            return queue;
        }
        public CombatMonster (string str)
        {
            string iconTexturePath = CombatMonsterManager.GetMonsterTextureString(str);
            IconTexture = AssetManager.GetTexture( iconTexturePath);
            Vector2 widthHeight = CombatMonsterManager.GetMonsterWidthAndHeight(str);
            Width = (int)widthHeight.X;
            Height = (int)widthHeight.Y;
       
        }
        public CombatMonster(Player.Player player)
        {
            PlayerStats stats = player.stats;
            //Speed = stats.MovementSpeed;
            BaseHealth = stats.MaxHealth;
            CurrentHealth = stats.CurrentHealth;
            Name = "Player";
            MaxMana = stats.MaxMana;
            CurrentMana = stats.CurrentMana;
            isPlayer = true;
            AttackPower = 1;
            Initiation = stats.Initiation;
            MovementQuickness = 200f;
            MovementPattern = "straight";
            MP = player.stats.MP;
            SP = player.stats.SP;
            Resistances = player.PlayerResistances;
            IconTexture = player.Texture;
            Width = player.PlayerWidth;
            Height = player.PlayerHeight;
        }



        //public CombatMonster (Summoner summon)
        //{

        //    isPlayerControled = true;
        //}
    }
    public enum MonsterActionOrder
    {
        AttackClosestEnemy,
        MoveTowardsClosestEnemy,
        AttackSelf,
        Exist,
    }
    public enum ChooseWhichMonsterAttack
    {
        ShortestRange
    }
}
