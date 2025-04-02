using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PlayingAround.Game.Map
{
    public class MapTileData
    {
        public int GridX { get; set; }
        public int GridY { get; set; }
        public int GridZ { get; set; }
        [JsonIgnore]
        public string Id => $"{GridX}_{GridY}_{GridZ}";

        [JsonPropertyName("background")]
        public string Background { get; set; }

        [JsonPropertyName("cells")]
        public List<TileCellData> Cells { get; set; } = new();

        public List<string> Monsters { get; set; } = new();
    }
}
