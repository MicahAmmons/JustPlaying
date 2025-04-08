using System.Text.Json.Serialization;

public class TileCellData
{
    [JsonPropertyName("x")] public int X { get; set; }
    [JsonPropertyName("y")] public int Y { get; set; }
    [JsonPropertyName("walkable")] public bool Walkable { get; set; }
    [JsonPropertyName("z")] public int Z { get; set; }

    [JsonPropertyName("behindOverlay")] public string? BehindOverlay { get; set; }
    [JsonPropertyName("frontOverlay")] public string? FrontOverlay { get; set; }
    [JsonPropertyName("npc")] public string? Npc { get; set; }
    [JsonPropertyName("monster")] public string? Monster { get; set; }
    [JsonPropertyName("trigger")] public string? Trigger { get; set; }
    [JsonPropertyName("nextTile")] public NextTileData? NextTile { get; set; }
}

public class NextTileData
{
    [JsonPropertyName("x")] public int NextX { get; set; }
    [JsonPropertyName("y")] public int NextY { get; set; }
    [JsonPropertyName("z")] public int NextZ { get; set; }
    [JsonPropertyName("direction")] public string NextDirection { get; set; }
}
