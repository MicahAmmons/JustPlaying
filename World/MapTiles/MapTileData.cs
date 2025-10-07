using PlayingAround.AnimationFolder.GlowTex;
using PlayingAround.Entities.Monster;
using PlayingAround.Entities.Monster.PlayMonsters;
using PlayingAround.Smoke;
using PlayingAround.World.MapTiles.CellHighlights;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PlayingAround.Game.Map
{
    public class MapTileData
    {
        [JsonPropertyName("gridX")]  public int GridX { get; set; }

        [JsonPropertyName("gridY")]  public int GridY { get; set; }

        [JsonPropertyName("gridZ")]  public int GridZ { get; set; }
        [JsonPropertyName("behindGlowTexturesFullSize")] public List<GlowTextureData> GlowTexture { get; set; }
        public string Id { get; set; }
        [JsonPropertyName("backgroundSmokeTexture")] public SmokeTexture BackgroundSmokeTexture { get; set; }

        [JsonPropertyName("buildBackground")]  public List<string> BackgroundBuildOrder { get; set; }
        
        [JsonPropertyName("cells")]  public List<TileCellData> Cells { get; set; } = new();

        [JsonPropertyName("monsters")] public List<string> MonsterStrings { get; set; } = new();
        [JsonPropertyName("difficultyMax")] public float DifficultyMax { get; set; } = 1;
        [JsonPropertyName("difficultyMin")] public float DifficultyMin { get; set; } = 1;

        [JsonPropertyName("totalSpawns")] public int TotalMonsterSpawns { get; set; }
        [JsonPropertyName("tileHighlights")] public TileCellHighlightData TileHighlightData { get; set; }
        [JsonPropertyName("backgroundColor")] public string BackgroundColor {  get; set; }
        


    }
}
