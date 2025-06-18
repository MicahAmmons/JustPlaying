using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.AnimationFolder;
using PlayingAround.Entities.Monster.PlayMonsters;
using PlayingAround.Entities.Summons;
using PlayingAround.Game.Map;
using PlayingAround.Managers.Assets;
using PlayingAround.Managers.CombatMan.Aspects;
using PlayingAround.Managers.CombatMan.CombatAttacks;
using PlayingAround.Managers.Entities;
using PlayingAround.Managers.Movement.CombatGrid;
using PlayingAround.Managers.Resistances;
using PlayingAround.Stats;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Xml;


namespace PlayingAround.Entities.Monster.CombatMonsters
{
     public class CombatMonster
    {
        //Stuff I think we will get rid of soon or change 
        public float Level { get; set; } = 1;
        public float Difficulty { get; set; }



        public TileCell PlayerMovementEndPoint { get; set; } = null;
        public List<Vector2> MovePath { get; set; } = new();
        public List<Aspect> Aspects { get; set; } = new List<Aspect>();
        public Vector2 currentPos;



        public string UniqueId;
        public ElementType ElementType { get; set; }
        public BaseCombatStats BaseStats { get; set; }
        public DrawSpecificStats DrawSpecifics { get; set; }
        public Dictionary<AnimationState, Animation> Animations { get; set; }
        public List<SingleAttack> Attacks { get; set; } = new List<SingleAttack> { };
        public List<MonsterActionOrder> ActionOrder { get; set; }
        public List<ChooseWhichMonsterAttack> DecideWhichAttack { get; set; }
        public string Name { get; set; }
        public string NamePlusLevel { get; set; }
        public CurrentCombatStats CurrentStats { get; set; }
        public CombatMonsterType MonsterIs {  get; set; }
        public PlayerCombatStats PlayerCombatStats { get; set; } = null;
        public bool isDead { get; set; } = false;
        public Texture2D Icon { get; set; }
        public Dictionary<ElementType, float> Resistances { get; set; }


        public CombatMonster (CombatMonsterData data, ElementType element = ElementType.None)
        {
            UniqueId = data.UniqueId;
            ElementType = element == ElementType.None ? data.DefaultElementType : element;
            BaseStats = data.BaseStats;
            DrawSpecifics = data.DrawSpecifics;
            Animations = data.Animations;
            ActionOrder = data.ActionOrder;
            DecideWhichAttack = data.DecideWhichAttack;
            Name = UniqueId;
            CurrentStats = new CurrentCombatStats()
            {
                Health = BaseStats.Health,
                AP = BaseStats.AP,
                MP = BaseStats.MP,
            };
            foreach (var kvp in data.AttackData)
            {
                Attacks.Add(AttackManager.GetAttack(kvp.Key, kvp.Value));
            }
            MonsterIs = CombatMonsterType.AI;
            Icon = AssetManager.GetTexture($"{UniqueId}Icon");
            Resistances = ResistanceManager.GetResistances(ElementType);
            
        }

        //combatMonster template to CombatMonster
        public CombatMonster()
        {

        }
        // Player to Player Combat Monster
        public CombatMonster(Player.Player player)
        {
            PlayerStats stats = player.stats;
            BaseHealth = stats.MaxHealth;
            CurrentHealth = stats.CurrentHealth;
            Name = "Player";
            isPlayer = true;
            Initiation = stats.Initiation;
            MovementQuickness = 200f;
            MovementPattern = "straight";
            MP = player.stats.MP;
            IconTexture = player.IconTexture;
            Width = player.PlayerWidth;
            Height = player.PlayerHeight;
            Resistances = player.Resistances;
            BaseActionPoints = player.stats.ActionPoint;
            foreach (var kvp in player.Animation)
            {
                Animation[kvp.Key] = new Animation(player.Animation[kvp.Key]);
            }

            
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
    public enum CombatMonsterType
    {
        Summoned,
        Player,
        AI
    }
}
