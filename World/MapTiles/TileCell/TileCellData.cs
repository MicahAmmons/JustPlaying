using System.Collections.Generic;
using System.Text.Json.Serialization;

public class TileCellData
{
    [JsonPropertyName("x")] public int X { get; set; }
    [JsonPropertyName("y")] public int Y { get; set; }
    [JsonPropertyName("walkable")] public bool Walkable { get; set; }
    [JsonPropertyName("z")] public int Z { get; set; }
    [JsonPropertyName("playerSpawnbable")] public bool PlayerSpawnable { get; set; }
    [JsonPropertyName("monsterSpawnable")] public bool MonsterSpawnable { get; set; }
    [JsonPropertyName("playMonsterSpawnable")] public bool PlayMonsterSpawnable { get; set; }

    [JsonPropertyName("behindOverlay")] public string? BehindOverlay { get; set; }
    [JsonPropertyName("frontOverlay")] public string? FrontOverlay { get; set; }
    [JsonPropertyName("npc")] public string? NPCName { get; set; }
    [JsonPropertyName("monster")] public List<string?> Monsters { get; set; }
    [JsonPropertyName("trigger")] public string? Trigger { get; set; }
    [JsonPropertyName("nextTileCell")] public NextTileData NextTile { get; set; }
    [JsonIgnore] public bool CanSpawn => Walkable && NextTile == null;
}

public class NextTileData
{
    [JsonPropertyName("x")] public int NextX { get; set; }
    [JsonPropertyName("y")] public int NextY { get; set; }
    [JsonPropertyName("z")] public int NextZ { get; set; }
    [JsonPropertyName("direction")] public string NextDirection { get; set; }
}
