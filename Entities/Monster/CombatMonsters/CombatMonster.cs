using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.AnimationFolder;
using PlayingAround.Game.Map;
using PlayingAround.Managers.Assets;
using PlayingAround.Managers.CombatMan.Aspects;
using PlayingAround.Managers.CombatMan.CombatAttacks;
using PlayingAround.Managers.Resistances;
using System;
using System.Collections.Generic;
using PlayingAround.Entities.Player;
using System.Numerics;
using System.Net;


namespace PlayingAround.Entities.Monster.CombatMonsters
{
     public class CombatMonster
    {
        //Stuff I think we will get rid of soon or change 
        public float Level { get; set; } = 1;
        public float Difficulty { get; set; }



        public TileCell PlayerMovementEndPoint { get; set; } = null;
        public List<Microsoft.Xna.Framework.Vector2> MovePath { get; set; } = new();
        public List<Aspect> Aspects { get; set; } = new List<Aspect>();
        public Microsoft.Xna.Framework.Vector2 currentPos;



        public string UniqueId;
        public ElementType ElementType { get; set; }
        public BaseCombatStats BaseStats { get; set; }
        public DrawSpecificStats DrawSpecifics { get; set; }
        public Dictionary<AnimationState, Animation> Animations { get; set; } = new Dictionary<AnimationState, Animation>();
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
        public Texture2D SpriteSheet {  get; set; }
        public Dictionary<ElementType, float> Resistances { get; set; }


        public CombatMonster (CombatMonsterData data, ElementType element = ElementType.None)
        {
            UniqueId = data.UniqueId;
            ElementType = element == ElementType.None ? data.DefaultElementType : element;
            BaseStats = data.BaseStats;
            DrawSpecifics = data.DrawSpecifics;
            DrawSpecifics.AllowedToMove = true;
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
            SpriteSheet = Icon;
            Resistances = ResistanceManager.GetResistances(ElementType);
            foreach (var kvp in data.AnimationData)
            {
                AnimationState state = kvp.Key;
                int row = kvp.Value[0];
                int frames = kvp.Value[1];
                int duration = kvp.Value[2];
                Animations[state] = new Animation(SpriteSheet, row, frames, duration);
            }
        }

        //combatMonster template to CombatMonster
        public CombatMonster()
        {

        }
        // Player to Player Combat Monster
        public CombatMonster(Player.Player player, ElementType element = ElementType.None)
        {
            UniqueId = "Player";
            ElementType = element == ElementType.None ? ElementType.Normal : element;
            BaseStats = player.BaseCombatStats;
            DrawSpecifics = player.DrawSpecifics;
            DrawSpecifics.AllowedToMove = true;
            Animations = player.Animation;
            ActionOrder = null;
            DecideWhichAttack = null;
            Name = UniqueId;
            CurrentStats = new CurrentCombatStats()
            {
                Health = player.CurrentCombatStats.Health,
                AP = BaseStats.AP,
                MP = BaseStats.MP,
            };
            Attacks = null;
            MonsterIs = CombatMonsterType.Player;
            Icon = AssetManager.GetTexture($"Hero_Blonde");
            Resistances = ResistanceManager.GetResistances(ElementType);



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
