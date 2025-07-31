using System;
using System.IO;
using System.Text.Json;
using Zabbor.ZabborBase.Models;

namespace Zabbor.ZabborBase.Managers
{
    public static class SaveManager
    {
        private static readonly string _saveFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
            "MojaGraRPG", 
            "savegame.json");

        public static bool SaveFileExists() => File.Exists(_saveFilePath);

        public static void SaveGame(SaveData data)
        {
            // Upewnij się, że folder istnieje
            Directory.CreateDirectory(Path.GetDirectoryName(_saveFilePath));

            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(data, options);
            File.WriteAllText(_saveFilePath, jsonString);
        }

        public static SaveData LoadGame()
        {
            if (!SaveFileExists()) return null;

            string jsonString = File.ReadAllText(_saveFilePath);
            return JsonSerializer.Deserialize<SaveData>(jsonString);
        }
    }
}