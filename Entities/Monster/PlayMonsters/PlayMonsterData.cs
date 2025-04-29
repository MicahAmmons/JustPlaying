using Microsoft.Xna.Framework;

using System.Text.Json.Serialization;

namespace PlayingAround.Entities.Monster.PlayMonsters
{
    public class PlayMonsterData
    {
        [JsonPropertyName("movementSpeed")] public float MovementSpeed { get; set; }
        [JsonIgnore] public Rectangle PacingBoundaryRect => PacingBoundary?.ToRectangle() ?? new Rectangle();
        [JsonPropertyName("pacingBoundary")] public MonsterRectangle PacingBoundary { get; set; }
        [JsonPropertyName("movementPattern")] public string MovementPattern { get; set; }
        public string IconPath { get; set; }
        [JsonPropertyName("difficulty")] public float Difficulty { get; set; }
        [JsonPropertyName("pauseDuration")] public float PauseDuration { get; set; }
    }

    public class MonsterRectangle
    {
        [JsonPropertyName("x")] public int X { get; set; }
        [JsonPropertyName("y")] public int Y { get; set; }
        [JsonPropertyName("width")] public int Width { get; set; }
        [JsonPropertyName("height")] public int Height { get; set; }

        // Helper to convert to a real Rectangle
        public Rectangle ToRectangle() => new Rectangle(X, Y, Width, Height);
    }

}
