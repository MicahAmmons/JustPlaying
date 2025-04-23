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

        [JsonPropertyName("type")] public string TypeRaw { get; set; }  // temporary raw field

        public List<string> Type => TypeRaw?.Split(',').Select(t => t.Trim()).ToList() ?? new();

        [JsonPropertyName("range")] public int Range { get; set; }

        [JsonPropertyName("effect")] public string Effect { get; set; }
        [JsonPropertyName("cost")] public int Cost { get; set; }
        [JsonPropertyName("damage")] public int Damage { get; set; }
        [JsonPropertyName("target")] public string Target { get; set; } 

    }
}
