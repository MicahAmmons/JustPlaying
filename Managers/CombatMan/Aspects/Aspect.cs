using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Managers.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PlayingAround.Managers.CombatMan.Aspects
{
    public class Aspect
    {
        public ElementType DamageType { get; set; }
        public string Description { get; set; }
        public float Duration { get; set; }
        public float Damage { get; set; }
        public bool IsBuff { get; set; }
        public TickedTiming WhenTicked { get; set; }
        public bool IsStackable { get; set; }
        public string IconKey { get; set; }
        public bool IsDamage { get; set; }
        public Texture2D Icon { get; set; }
        public string Name { get; set; }

        public Aspect(string name, AspectData data, ElementType element = ElementType.None)
        {
            Name = name;
            Description = data.Description;
            Duration = data.Duration;
            Damage = data.Damage;
            DamageType = element == ElementType.None ? data.DefaultElement: element;
            IsBuff = data.IsBuff;
            WhenTicked = data.WhenTicked;
            IsStackable = data.IsStackable;
            Icon = AssetManager.GetTexture($"AcidIcon");
            IsDamage = data.IsDamage;

        }

    }
    public enum TickedTiming
    {
        StartOfTurn,
        EndOfTurn,
        AfterAttack,
        AfterMovement,
        OnDamage,
        OnBeingHit
    }

}
