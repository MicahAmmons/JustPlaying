
using PlayingAround.Visuals;
using System.Text.Json.Serialization;

namespace PlayingAround.Managers.CombatMan.CombatAttacks
{
    public class SingleAttack
    {
        [JsonPropertyName("name")] public string Name { get; set; } 
        [JsonPropertyName("elementDamage")] public ElementType ElementDamage { get; set; } 
        [JsonPropertyName("range")] public int Range { get; set; }
        [JsonPropertyName("effect")] public string Effect { get; set; }
        [JsonPropertyName("baseDamageMin")] public int MinDamage { get; set; }
        [JsonPropertyName("baseDamageMax")] public int MaxDamage { get; set; }
        [JsonPropertyName("target")] public string Target { get; set; }
        [JsonPropertyName("attacksHasIcon")] public bool AttackHasIcon { get; set; }
        [JsonPropertyName("visualVelocity")] public float VisualVelocity { get; set; } = 200f;
        [JsonPropertyName("texturePath")] public string TexturePath {  get; set; }
        [JsonPropertyName("animated")] public bool Animated { get; set; } = false;
        [JsonConverter(typeof(JsonStringEnumConverter))] public VisualTiming VisualTiming { get; set; } 
        [JsonPropertyName("whenApplyAspect")] public string WhenApplyAspect { get; set; }

    }
}
