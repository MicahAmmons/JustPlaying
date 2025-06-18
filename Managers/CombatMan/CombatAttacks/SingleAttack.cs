
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Managers.Assets;
using PlayingAround.Visuals;
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
            if (data.AttackHasIcon)
                Icon = AssetManager.GetTexture($"{Name}");
        }


    }

}

public enum AttackName
{
    Slam,
    Spit,

}

