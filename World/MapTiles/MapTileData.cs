using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PlayingAround.Game.Map
{
    public class MapTileData
    {
        public int Id { get; set; }
        public int GridX { get; set; }
        public int GridY { get; set; }

        [JsonPropertyName("background")]
        public string Background { get; set; }

        [JsonPropertyName("cells")]
        public List<TileCellData> Cells { get; set; } = new();

        public List<string> Monsters { get; set; } = new();
    }
}
