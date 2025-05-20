using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PlayingAround.Data.SaveData
{
    public static class SaveSystem
    {
        private static readonly string SaveDirectory = Path.GetFullPath(Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Data\SaveData\SaveJson"));



        public static void SaveGame(GameSaveData data, string filename)
        {
            var path = Path.Combine(SaveDirectory, filename);
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }

        public static GameSaveData LoadGame(string filename)
        {
            var path = Path.Combine(SaveDirectory, filename);
            if (!File.Exists(path)) return null;

            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<GameSaveData>(json);
        }

    }

}
