using Microsoft.Xna.Framework;
using PlayingAround.Visuals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PlayingAround.Managers.CombatMan.CombatAttacks
{
    public class SingleAttack
    {
        [JsonPropertyName("name")] public string Name { get; set; } 
        [JsonPropertyName("elementDamage")] public string ElementDamage { get; set; } 
        [JsonPropertyName("range")] public int Range { get; set; }
        [JsonPropertyName("effect")] public string Effect { get; set; }
        [JsonPropertyName("baseDamageMin")] public int MinDamage { get; set; }
        [JsonPropertyName("baseDamageMax")] public int MaxDamage { get; set; }
        [JsonPropertyName("target")] public string Target { get; set; }
        [JsonPropertyName("attacksHasIcon")] public bool AttackHasIcon { get; set; }
        [JsonPropertyName("visualVelocity")] public float VisualVelocity { get; set; } = 200f;
        [JsonPropertyName("texturePath")] public string TexturePath {  get; set; }
        [JsonPropertyName("animated")] public bool Animated { get; set; } = false;
        [JsonConverter(typeof(JsonStringEnumConverter))] public VisualTiming VisualTiming { get; set; } = VisualTiming.DuringAttack; // default if missing

    }
}
