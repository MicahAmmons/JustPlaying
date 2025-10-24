
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.ActFolder;
using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Game.Map;
using PlayingAround.Interfaces;
using PlayingAround.Managers.Assets;
using PlayingAround.Managers.Tiles;
using PlayingAround.Visuals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace PlayingAround.Managers.CombatMan.CombatAttacks
{
    public class SingleAttack
    {
        public AttackName Name { get; set; }
        public int MinDamage { get; set; }
        public int MaxDamage { get; set; }
        public int Range { get; set; }
        public string Aspect { get; set; }
        public List<CombatMonsterType> TargetType { get; set; } = new List<CombatMonsterType>();
        public ElementType ElementDamage { get; set; } = ElementType.None;
        public bool AttackPerformedWhenFinished {  get; set; }
        public int AttackPerformedDownFrame { get; set; }
        public int AttackPerformedUpFrame { get; set; }
        public AnimationState AttackUpAnimation { get; set; }
        public AnimationState AttackDownAnimation { get; set; }
        public AttVisualEffectDetails VE { get; set; }
        public bool IsFinished { get; set; } = false;

       
        public SingleAttack (SingleAttackData data )
        {
            Name = data.AttackName;
            ElementDamage = data.ElementType;
            Range = data.Range;
            Aspect = data.Aspect;
            MinDamage = data.BaseDamageMin;
            MaxDamage = data.BaseDamageMax;
            AttackPerformedDownFrame = data.AttackPerformedDownFrame;
            AttackPerformedUpFrame = data.AttackPerformedUpFrame;
            AttackUpAnimation = data.AttackUpAnimation;
            AttackDownAnimation = data.AttackDownAnimation;
            AttackPerformedWhenFinished = data.AttackPerformedWhenFinished;
            VE = data.VE;
            if (data.TargetType.Count > 0)
            {
                foreach (var tar in data.TargetType)
                {
                    TargetType.Add(tar);
                }
            }
        }

        internal int AttackPerformedFrame(Vector2 dir)
        {
            return dir.Y < 0 ? AttackPerformedUpFrame : AttackPerformedDownFrame;
        }

    }

}

public enum AttackName
{
    Slam,
    Spit,
    GraspingRoot,
    LavaBall,
    IcicleStab,
    TurtleSmash

}

