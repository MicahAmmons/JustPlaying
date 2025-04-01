using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using PlayingAround.Game.Map;

namespace PlayingAround.Utils
{
    public static class JsonLoader
    {
        public static List<MapTileData> LoadTileData(string filePath)
        {
            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<MapTileData>>(json);
        } 
    }
}
