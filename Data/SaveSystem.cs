using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PlayingAround.Data
{
    public static class SaveSystem
    {
        private static readonly string SaveDirectory = Path.GetFullPath(Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Data\SaveJson"));
        private static readonly string SavePath = Path.Combine(SaveDirectory, "savegame.json");


        public static void SaveGame(GameSaveData saveData)
        {
            var json = JsonSerializer.Serialize(saveData, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SavePath, json);
        }

        public static GameSaveData LoadGame()
        {
            if (!File.Exists(SavePath)) return null;

            var json = File.ReadAllText(SavePath);
            return JsonSerializer.Deserialize<GameSaveData>(json);
        }
    }

}
