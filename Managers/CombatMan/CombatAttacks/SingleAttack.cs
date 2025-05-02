using Microsoft.Xna.Framework;
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
        public List<Vector2> MovePath { get; set; }
        public Vector2 CurrentPos { get; set; }
        public float MoveQuickness { get; set; } = 200f;
    }
}
