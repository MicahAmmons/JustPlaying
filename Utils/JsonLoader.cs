using System;
using System.Collections.Generic;
using System.IO;
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
using PlayingAround.Managers.NPCHouse;
using PlayingAround.Managers.Quests;

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
                  AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Data\Summons\SummonDefJson\SummonProgressionDefinitions.json"));

        public static Dictionary<string, SummonProgressionData> LoadSummonProgressions()
        {
            if (!File.Exists(SummonProgressionPath))
                return new Dictionary<string, SummonProgressionData>();

            var json = File.ReadAllText(SummonProgressionPath);
            return JsonSerializer.Deserialize<Dictionary<string, SummonProgressionData>>(json);
        }

        private static readonly string AttackDataPath = Path.GetFullPath(Path.Combine(
                 AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Data\Attacks\AttackData.json"));
        public static Dictionary<string, SingleAttack> LoadAttackData()
        {
            string json = File.ReadAllText(AttackDataPath);
            return JsonSerializer.Deserialize<Dictionary<string, SingleAttack>>(json);
        }

        private static readonly string CombatMonsterPath = Path.GetFullPath(Path.Combine(
         AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Data\CombatMonsterData\CombatMonsterData.json"));
        public static Dictionary<string, CombatMonster> LoadCombatMonsterData()
        {
            string json = File.ReadAllText(CombatMonsterPath);
            return JsonSerializer.Deserialize<Dictionary<string, CombatMonster>>(json);
        }

        private static readonly string ResistancePath = Path.GetFullPath(Path.Combine(
          AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Data\Resistances\Resistance.json"));
        public static Dictionary<string, Dictionary<string, string>> LoadResistanceData()
        {
            string json = File.ReadAllText(ResistancePath);
            return JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(json);
        }

        private static readonly string PlayMonsterPath = Path.GetFullPath(Path.Combine(
  AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Data\PlayMonsterJson\PlayMonsters.json"));
        public static Dictionary<string, List<PlayMonsterData>> LoadPlayMonsterData()
        {
            string json = File.ReadAllText(PlayMonsterPath);
            return JsonSerializer.Deserialize<Dictionary<string, List<PlayMonsterData>>>(json);
        }

        private static readonly string AspectDataPath = Path.GetFullPath(Path.Combine(
AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Data\Aspects\AspectData.json"));
        public static Dictionary<string, Aspect> LoadAspectData()
        {
            string json = File.ReadAllText(AspectDataPath);
            return JsonSerializer.Deserialize<Dictionary<string, Aspect>>(json);
        }

        private static readonly string NPCDataPath = Path.GetFullPath(Path.Combine(
AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Data\NPCs\NPCData.json"));
        public static Dictionary<string, NPCData> LoadNPCData()
        {
            string json = File.ReadAllText(NPCDataPath);
            return JsonSerializer.Deserialize<Dictionary<string, NPCData>>(json);
        }

        private static readonly string DialogueDataPath = Path.GetFullPath(Path.Combine(
AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Data\Dialogue\DialogueData.json"));
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
        private static readonly string QuestDataPath = Path.GetFullPath(Path.Combine(
AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Data\Quests\QuestData.json"));
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


    }
}