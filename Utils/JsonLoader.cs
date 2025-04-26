using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using PlayingAround.Entities.Monster.PlayMonsters;
using PlayingAround.Entities.Summons;
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


        private static readonly string SummonProgressionPath = Path.GetFullPath(Path.Combine(
                  AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Entities\Summons\SummonDefJson\SummonProgressionDefinitions.json"));

        public static Dictionary<string, SummonProgressionData> LoadSummonProgressions()
        {
            if (!File.Exists(SummonProgressionPath))
                return new Dictionary<string, SummonProgressionData>();

            var json = File.ReadAllText(SummonProgressionPath);
            return JsonSerializer.Deserialize<Dictionary<string, SummonProgressionData>>(json);
        }
    }
}