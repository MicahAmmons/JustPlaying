using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using PlayingAround.Data.NPCs;
using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Entities.Monster.PlayMonsters;
using PlayingAround.Entities.Summons;
using PlayingAround.Game.Map;
using PlayingAround.Managers.CombatMan.Aspects;
using PlayingAround.Managers.CombatMan.CombatAttacks;
using PlayingAround.Managers.Dialogue;
using PlayingAround.Managers.Escape.Settings;
using PlayingAround.Managers.NPCHouse;
using PlayingAround.Managers.Quests;

namespace PlayingAround.Utils
{
    public static class JsonLoader
    {
        public static string GetDataPath(params string[] parts)
        {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(new[] { basePath, "Data" }.Concat(parts).ToArray());
        }

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

        private static readonly string SummonProgressionPath = GetDataPath("Summons", "SummonDefJson", "SummonProgressionDefinitions.json");
        public static Dictionary<string, SummonProgressionData> LoadSummonProgressions()
        {
            if (!File.Exists(SummonProgressionPath))
                return new Dictionary<string, SummonProgressionData>();

            string json = File.ReadAllText(SummonProgressionPath);
            return JsonSerializer.Deserialize<Dictionary<string, SummonProgressionData>>(json);
        }

        private static readonly string AttackDataPath = GetDataPath("Attacks", "AttackData.json");
        public static Dictionary<string, SingleAttack> LoadAttackData()
        {
            string json = File.ReadAllText(AttackDataPath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };
            return JsonSerializer.Deserialize<Dictionary<string, SingleAttack>>(json, options);
        }

        private static readonly string CombatMonsterPath = GetDataPath("CombatMonsterData", "CombatMonsterData.json");
        public static Dictionary<string, CombatMonster> LoadCombatMonsterData()
        {
            string json = File.ReadAllText(CombatMonsterPath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };
            return JsonSerializer.Deserialize<Dictionary<string, CombatMonster>>(json, options);
        }

        private static readonly string ResistancePath = GetDataPath("Resistances", "Resistance.json");
        public static Dictionary<ElementType, Dictionary<string, ElementType>> LoadResistanceData()
        {
            string json = File.ReadAllText(ResistancePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };
            return JsonSerializer.Deserialize<Dictionary<ElementType, Dictionary<string, ElementType>>>(json, options);
        }

        private static readonly string PlayMonsterPath = GetDataPath("PlayMonsterJson", "PlayMonsters.json");
        public static Dictionary<string, List<PlayMonsterData>> LoadPlayMonsterData()
        {
            string json = File.ReadAllText(PlayMonsterPath);
            return JsonSerializer.Deserialize<Dictionary<string, List<PlayMonsterData>>>(json);
        }

        private static readonly string AspectDataPath = GetDataPath("Aspects", "AspectData.json");
        public static Dictionary<string, Aspect> LoadAspectData()
        {
            string json = File.ReadAllText(AspectDataPath);
            return JsonSerializer.Deserialize<Dictionary<string, Aspect>>(json);
        }

        private static readonly string NPCDataPath = GetDataPath("NPCs", "NPCData.json");
        public static Dictionary<string, NPCData> LoadNPCData()
        {
            string json = File.ReadAllText(NPCDataPath);
            return JsonSerializer.Deserialize<Dictionary<string, NPCData>>(json);
        }

        private static readonly string DialogueDataPath = GetDataPath("Dialogue", "DialogueData.json");
        public static Dictionary<string, DialogueData> LoadDialogueData()
        {
            string json = File.ReadAllText(DialogueDataPath);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };

            return JsonSerializer.Deserialize<Dictionary<string, DialogueData>>(json, options);
        }

        private static readonly string QuestDataPath = GetDataPath("Quests", "QuestData.json");
        public static Dictionary<string, QuestData> LoadQuests()
        {
            string json = File.ReadAllText(QuestDataPath);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };

            return JsonSerializer.Deserialize<Dictionary<string, QuestData>>(json, options);
        }
        private static readonly string SettingsDataPath = GetDataPath("Settings", "SettingData.json");
        public static Dictionary<string, Setting> LoadSettingData()
        {
            string json = File.ReadAllText(SettingsDataPath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };
            return JsonSerializer.Deserialize<Dictionary<string, Setting>>(json, options);
        }
    }
}
