
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Managers.Assets;
using PlayingAround.Visuals;
using System.Text.Json.Serialization;

namespace PlayingAround.Managers.CombatMan.CombatAttacks
{
    public class SingleAttack
    {
        public AttackName Name { get; set; }
        public string AttackIcon { get; set; }
        public Texture2D AttackIconTexture { get; set; }
        public ElementType ElementDamage { get; set; } = ElementType.None;
        [JsonPropertyName("Range")] public int Range { get; set; }
        [JsonPropertyName("Aspect")] public string Aspect { get; set; }
        [JsonPropertyName("BaseDamageMin")] public int MinDamage { get; set; }
        [JsonPropertyName("BaseDamageMax")] public int MaxDamage { get; set; }
        [JsonPropertyName("AttacksHasIcon")] public bool AttackHasIcon { get; set; }
        [JsonPropertyName("VisualVelocity")] public float VisualVelocity { get; set; } = 200f;
        [JsonPropertyName("Animated")] public bool Animated { get; set; } = false;
        [JsonPropertyName("VisualTiming")] public VisualTiming VisualTiming { get; set; } 
        [JsonPropertyName("WhenApplyAspect")] public string WhenApplyAspect { get; set; }

        public SingleAttack CloneWithElement(ElementType element)
        {
            var iconKey = $"{element} {Name}";
            Texture2D iconTexture;

            try
            {
                iconTexture = AssetManager.GetTexture(iconKey);
            }
            catch
            {
                // fallback to base icon and color it by element
                var fallbackTexture = AssetManager.GetTexture(AttackIcon);
                iconTexture = AssetManager.GetIconWithElementColored(fallbackTexture, element);
            }

            return new SingleAttack
            {
                Name = this.Name,
                ElementDamage = element,
                Range = this.Range,
                Aspect = this.Aspect,
                MinDamage = this.MinDamage,
                MaxDamage = this.MaxDamage,
                AttackHasIcon = this.AttackHasIcon,
                VisualVelocity = this.VisualVelocity,
                Animated = this.Animated,
                VisualTiming = this.VisualTiming,
                WhenApplyAspect = this.WhenApplyAspect,
                AttackIcon = iconKey,
                AttackIconTexture = iconTexture
            };
        }


    }

}

public enum AttackName
{
    Slam,
    Spit,

}

