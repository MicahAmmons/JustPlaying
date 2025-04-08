using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using PlayingAround.Entities.Monster.PlayMonsters;
using PlayingAround.Game.Map;

namespace PlayingAround.Utils
{
    public static class JsonLoader
    {
        public static MapTileData LoadTileData(string filePath)
        {
            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<MapTileData>(json);
        }
        public static Dictionary<string, List<PlayMonsterData>> LoadPlayMonsterData(string filePath)
        {
            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<Dictionary<string, List<PlayMonsterData>>>(json);
        }

    }
}