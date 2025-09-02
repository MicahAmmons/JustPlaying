using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Entities.Monster.PlayMonsters;
using PlayingAround.Game.Map;
using PlayingAround.Managers.CombatMan.Aspects;
using PlayingAround.Managers.CombatMan.CombatAttacks;
using PlayingAround.Managers.Dialogue;
using PlayingAround.Managers.Escape.Settings;
using PlayingAround.Managers.NPCHouse;
using PlayingAround.Managers.Quests;
using PlayingAround.Managers.Triggers;
using PlayingAround.Managers.VisualEffects;
using PlayingAround.Visuals;

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


        private static readonly string AttackDataPath = GetDataPath("Attacks", "AttackData.json");
        public static Dictionary<AttackName, SingleAttackData> LoadAttackData()
        {
            string json = File.ReadAllText(AttackDataPath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };
            return JsonSerializer.Deserialize<Dictionary<AttackName, SingleAttackData>>(json, options);
        }

        private static readonly string CombatMonsterPath = GetDataPath("CombatMonsterData", "CombatMonsterData.json");
        public static Dictionary<string, CombatMonsterData> LoadCombatMonsterData()
        {
            string json = File.ReadAllText(CombatMonsterPath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };
            return JsonSerializer.Deserialize<Dictionary<string, CombatMonsterData>>(json, options);
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
        public static Dictionary<string, PlayMonsterData> LoadPlayMonsterData()
        {
            string json = File.ReadAllText(PlayMonsterPath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };
            return JsonSerializer.Deserialize<Dictionary<string, PlayMonsterData>>(json, options);
        }

        private static readonly string AspectDataPath = GetDataPath("Aspects", "AspectData.json");
        public static Dictionary<string, AspectData> LoadAspectData()
        {
            string json = File.ReadAllText(AspectDataPath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };
            return JsonSerializer.Deserialize<Dictionary<string, AspectData>>(json, options);
        }

        private static readonly string NPCDataPath = GetDataPath("NPCs", "NPCData.json");
        public static Dictionary<string, NPCData> LoadNPCData()
        {
            string json = File.ReadAllText(NPCDataPath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };
            return JsonSerializer.Deserialize<Dictionary<string, NPCData>>(json, options);
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

        private static readonly string TriggerDataPath = GetDataPath("Triggers", "TriggerData.json");
        public static Dictionary<string, Trigger> LoadTriggerData()
        {
            string json = File.ReadAllText(TriggerDataPath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };
            return JsonSerializer.Deserialize<Dictionary<string, Trigger>>(json, options);
        }

        private static readonly string VEDataPath = GetDataPath("VE", "veData.json");
        public static Dictionary<string, VisualEffectData> LoadVEData()
        {
            string json = File.ReadAllText(VEDataPath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };
            return JsonSerializer.Deserialize<Dictionary<string, VisualEffectData>>(json, options);
        }
    }
}
