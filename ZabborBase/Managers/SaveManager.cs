using System;
using System.IO;
using System.Text.Json;
using Zabbor.ZabborBase.Models;
using System.Collections.Generic;

namespace Zabbor.ZabborBase.Managers
{
    public static class SaveManager
    {
        private static string GetSaveFilePath(int slotIndex)
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                "MojaGraRPG", 
                $"save_{slotIndex}.json");
        }

        public static bool SaveFileExists(int slotIndex) => File.Exists(GetSaveFilePath(slotIndex));

        public static void SaveGame(SaveData data, int slotIndex)
        {
            var filePath = GetSaveFilePath(slotIndex);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(data, options);
            File.WriteAllText(filePath, jsonString);
        }

        public static SaveData LoadGame(int slotIndex)
        {
            if (!SaveFileExists(slotIndex)) return null;
            string jsonString = File.ReadAllText(GetSaveFilePath(slotIndex));
            return JsonSerializer.Deserialize<SaveData>(jsonString);
        }

        // Nowa metoda do wczytywania informacji o wszystkich slotach
        public static List<SaveData> GetAllSaveSlotsInfo()
        {
            var infos = new List<SaveData>();
            for (int i = 0; i < 10; i++)
            {
                infos.Add(LoadGame(i)); // Wczytujemy plik lub dodajemy null, jeśli nie istnieje
            }
            return infos;
        }
    }
}