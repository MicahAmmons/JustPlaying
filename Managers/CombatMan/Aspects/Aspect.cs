using Microsoft.Xna.Framework.Graphics;
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
        private Aspect aspect;

        [JsonPropertyName("name")] public string Name { get; set; }

        [JsonPropertyName("description")] public string Description { get; set; }

        [JsonPropertyName("duration")] public float Duration { get; set; }

        [JsonPropertyName("damage")] public float Damage { get; set; }

        [JsonPropertyName("damageType")] public string DamageType { get; set; }

        [JsonPropertyName("isBuff")] public bool IsBuff { get; set; }

        [JsonPropertyName("whenTicked")] public TickedTiming WhenTicked { get; set; }

        [JsonPropertyName("isStackable")] public bool isStackable { get; set; }
        [JsonPropertyName("iconKey")] public string IconKey { get; set; }
        [JsonIgnore] public Texture2D Icon { get; set; } // Loaded separately at runtime


        public Aspect()
        {


        }

        public Aspect(Aspect aspect)
        {
            Name = aspect.Name;
            Description = aspect.Description;
            Duration = aspect.Duration;
            Damage = aspect.Damage;
            DamageType = aspect.DamageType;
            IsBuff = aspect.IsBuff;
            WhenTicked = aspect.WhenTicked;
            isStackable = aspect.isStackable;
            IconKey = aspect.IconKey;
            Icon = aspect.Icon;
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
