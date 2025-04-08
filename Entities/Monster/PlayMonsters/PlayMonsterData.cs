using Microsoft.Xna.Framework;
using System;

using System.Text.Json.Serialization;

namespace PlayingAround.Entities.Monster.PlayMonsters
{
    public class PlayMonsterData
    {
        [JsonPropertyName("movementSpeed")]
        public float MovementSpeed { get; set; }

        [JsonPropertyName("pacingBoundary")]
        public Rectangle PacingBoundary { get; set; }

        [JsonPropertyName("movementPattern")]
        public string MovementPattern { get; set; }

        [JsonPropertyName("monsterType")]
        public string MonsterType { get; set; }

        [JsonPropertyName("difficulty")]
        public float Difficulty { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
