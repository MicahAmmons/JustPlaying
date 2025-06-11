using Microsoft.Xna.Framework;

using System.Text.Json.Serialization;

namespace PlayingAround.Entities.Monster.PlayMonsters
{
    public class PlayMonsterData
    {
        [JsonPropertyName("movementSpeed")] public float MovementSpeed { get; set; }
        [JsonPropertyName("movementPattern")] public string MovementPattern { get; set; }
        public string IconPath { get; set; }
        [JsonPropertyName("difficulty")] public float Difficulty { get; set; }
        [JsonPropertyName("pauseDurationMax")] public float PauseDurationMax { get; set; }
        [JsonPropertyName("pauseDurationMin")] public float PauseDurationMin { get; set; }

    }


}
