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

public class SaveManager
{
    public static Dictionary<string, GameSaveData> SaveFiles { get; private set; } = new();

    private static readonly string saveFolder = Path.Combine(AppContext.BaseDirectory, "Data", "SaveData", "SaveJson");
    public static GameSaveData CurrentGameSaveData;
    public static string CurrentSaveKey { get; private set; }

    public static void LoadAllSaves()
    {
        SaveFiles.Clear();

        var saveFiles = Directory.GetFiles(saveFolder, "saveGame*.json");


        foreach (var path in saveFiles)
        {
            try
            {
                var json = File.ReadAllText(path);
                var data = JsonSerializer.Deserialize<GameSaveData>(json);

                if (data != null)
                {
                    SaveFiles[Path.GetFileNameWithoutExtension(path)] = data;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load {path}: {ex.Message}");
            }
        }
    }

    public static void SetCurrentGameSave(string key)
    {
        CurrentSaveKey = key;
        CurrentGameSaveData = SaveFiles[key];

        PlayerManager.LoadContent(CurrentGameSaveData.Player);
        TileCellManager.Initialize();
        UIManager.LoadContent();
        ResistanceManager.LoadContent(); // Loads Resistance Data
        PlayMonsterManager.LoadContent(); // Loads Play Monster Data
        AspectManager.LoadAspects(); // Load Aspect Data
        AttackManager.LoadContent(); //Loads attack data
        CombatMonsterManager.LoadContent(); // Loads Combat Monster Data
        TileManager.Initialize(CurrentGameSaveData.MapTile.CurrentTileId);
        SceneManager.SetState(SceneManager.SceneState.Play);
        DayCycleManager.LoadContent(CurrentGameSaveData.DayCycle.Day);
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
            DayCycle = DayCycleManager.SaveDayCycle()
        };

        var path = Path.Combine(saveFolder, CurrentSaveKey + ".json");
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);
    }
}
