using System.Collections.Generic;
using System.IO;
using System;
using System.Text.Json;
using PlayingAround.Data.SaveData;
using System.Linq;
using PlayingAround.Manager;
using PlayingAround.Managers;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Managers.CombatMan.Aspects;
using PlayingAround.Managers.CombatMan.CombatAttacks;
using PlayingAround.Managers.CombatMan;
using PlayingAround.Managers.UI;

public class SaveManager
{
    public static Dictionary<string, GameSaveData> SaveFiles { get; private set; } = new();

    private static readonly string saveFolder = Path.Combine(AppContext.BaseDirectory, "Data", "SaveData", "SaveJson");
    public static GameSaveData CurrentGameSaveData;

    public static void LoadAllSaves()
    {
        SaveFiles.Clear();

        var saveFiles = Directory.GetFiles(saveFolder, "saveGame*.json")
                                 .Concat(Directory.GetFiles(saveFolder, "saveGameTemplate.json"));

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

      

 
        CombatManager.Initialize();

    }
}
