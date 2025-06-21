using System.Collections.Generic;
using System.IO;
using System;
using System.Text.Json;
using PlayingAround.Data.SaveData;
using System.Linq;
using PlayingAround.Managers;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Managers.CombatMan.Aspects;
using PlayingAround.Managers.CombatMan.CombatAttacks;
using PlayingAround.Managers.CombatMan;
using PlayingAround.Managers.UI;
using PlayingAround.Managers.DayManager;
using PlayingAround.Managers.Entities;
using PlayingAround.Managers.Tiles;
using PlayingAround.Managers.Quests;
using System.Text.Json.Serialization;
using PlayingAround.Utils;
using PlayingAround.Managers.Escape.Settings;
using PlayingAround.Managers.Resistances;

public class SaveManager
{
    public static Dictionary<string, GameSaveData> SaveFiles { get; private set; } = new();

    //private static readonly string saveFolder = Path.Combine(SavePathHelper.GetSaveFolder("PlayingAround"), "SaveJson");
    private static readonly string saveFolder =
#if DEBUG
    Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\Data\SaveData\SaveJson"));
#else
    JsonLoader.GetDataPath("SaveData", "SaveJson");
#endif






    public static GameSaveData CurrentGameSaveData;
    public static string CurrentSaveKey { get; private set; }
    public static void LoadAllSaves()
    { 
        SaveFiles.Clear();

        // Ensure the folder exists
        if (!Directory.Exists(saveFolder))
            Directory.CreateDirectory(saveFolder);

        var allJsonFiles = Directory.GetFiles(saveFolder, "*.json");

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        foreach (var path in allJsonFiles)
        {
            string fileName = Path.GetFileNameWithoutExtension(path);

            if (!fileName.StartsWith("saveGame", StringComparison.OrdinalIgnoreCase))
            {
                File.Delete(path);
                Console.WriteLine($"Deleted irrelevant file: {fileName}");
                continue;
            }

            try
            {
                var json = File.ReadAllText(path);
                var data = JsonSerializer.Deserialize<GameSaveData>(json, options);

                if (data != null)
                {
                    SaveFiles[fileName] = data;
                    Console.WriteLine($"Loaded valid save: {fileName}");
                }
                else
                {
                    File.Delete(path);
                    Console.WriteLine($"Deleted corrupt save: {fileName}");
                }
            }
            catch (Exception ex)
            {
                File.Delete(path);
                Console.WriteLine($"Deleted unreadable save '{fileName}': {ex.Message}");
            }
        }
    }


    public static void SetCurrentGameSave(string key)
    {
        CurrentSaveKey = key;
        CurrentGameSaveData = SaveFiles[key];
    }

    public static void LoadCurrentGameSave() 
    {
        SummonedMonsterManager.LoadContent(CurrentGameSaveData.SummonedData);
        PlayerManager.LoadContent(CurrentGameSaveData.Player);
        QuestManager.LoadContent(CurrentGameSaveData.Quests);
        UIManager.LoadContent();
        ResistanceManager.LoadContent(); // Loads Resistance Data
        PlayMonsterManager.LoadContent(); // Loads Play Monster Data
        AspectManager.LoadAspects(); // Load Aspect Data
        AttackManager.LoadContent(); //Loads attack data
        CombatMonsterManager.LoadContent(); // Loads Combat Monster Data
        TileManager.Initialize(CurrentGameSaveData.MapTile.CurrentTileId);
        SceneManager.SetState(SceneState.Play);
        DayCycleManager.LoadContent(CurrentGameSaveData.DayCycle);
        QuestLibrary.LoadContent();
        SettingsSuper.LoadSaveContent(CurrentGameSaveData.Settings);
     //   CombatManager.Initialize();

    }
    public static string CreateNewGame()
    {
        string templatePath = Path.Combine(saveFolder, "saveGameTemplate.json");

        if (!File.Exists(templatePath))
        {
            Console.WriteLine("Template save file not found.");
            return null;
        }

        // Find the next available gameSave#.json
        int saveIndex = 1;
        string newSavePath;
        string newKey;

        do
        {
            newKey = $"savegame{saveIndex}";
            newSavePath = Path.Combine(saveFolder, newKey + ".json");
            saveIndex++;
        } while (File.Exists(newSavePath));

        // Copy the template to the new save slot
        File.Copy(templatePath, newSavePath);

        // Deserialize the new file and add it to SaveFiles
        try
        {
            var json = File.ReadAllText(newSavePath);
            var newSaveData = JsonSerializer.Deserialize<GameSaveData>(json);

            if (newSaveData != null)
            {
                SaveFiles[newKey] = newSaveData;
                Console.WriteLine($"New save created: {newKey}");
                return newKey;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to create new game: {ex.Message}");
        }

        return null;
    }

    public static void SaveGame()
    {
        GameSaveData data = new GameSaveData
        {
            Player = PlayerManager.SavePlayer(),
            MapTile = TileManager.SaveMapTile(),
            DayCycle = DayCycleManager.SaveDayCycle(),
            Quests = QuestManager.SaveQuestData(),
            Settings = SettingsSuper.SaveSettingData(),
            SummonedData = SummonedMonsterManager.Save()
        };

        var path = Path.Combine(saveFolder, CurrentSaveKey + ".json");
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);
    }

    private static string GetProjectRootPath()
    {
#if DEBUG
        var dir = AppContext.BaseDirectory;
        for (int i = 0; i < 3; i++) // climbs out of /bin/Debug/netX
            dir = Directory.GetParent(dir)?.FullName ?? dir;
        return dir;
#else
    return AppContext.BaseDirectory;
#endif
    }
}
