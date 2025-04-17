using PlayingAround.Entities.Monster;
using PlayingAround.Entities.Monster.PlayMonsters;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PlayingAround.Game.Map
{
    public class MapTileData
    {
        [JsonPropertyName("gridX")]
        public int GridX { get; set; }

        [JsonPropertyName("gridY")]
        public int GridY { get; set; }

        [JsonPropertyName("gridZ")]
        public int GridZ { get; set; }

        public string Id { get; set; }

        [JsonPropertyName("background")]
        public string Background { get; set; }

        [JsonPropertyName("cells")]
        public List<TileCellData> Cells { get; set; } = new();

        [JsonPropertyName("monsters")]
        public List<string> MonsterStrings { get; set; } = new();
        [JsonPropertyName("difficulty")]
        public float Difficulty { get; set; } = 0;

        [JsonPropertyName("totalSpawns")]
        public int TotalMonsterSpawns { get; set; }


    }
}
