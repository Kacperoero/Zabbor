using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Zabbor.Managers // Użyj poprawnej przestrzeni nazw
{
    public static class DialogueManager
    {
        private static Dictionary<string, string> _dialogues;

        public static void LoadDialogues()
        {
            string contentPath = Path.Combine(AppContext.BaseDirectory, "Content", "dialogues.json");
            if (!File.Exists(contentPath))
            {
                throw new FileNotFoundException("Nie można znaleźć pliku z dialogami!", contentPath);
            }
            string jsonString = File.ReadAllText(contentPath);
            _dialogues = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString);
        }

        public static string GetDialogue(string id)
        {
            // Zwracamy dialog o danym ID lub informację o błędzie, jeśli go nie ma
            return _dialogues.GetValueOrDefault(id, "Brak dialogu o tym ID.");
        }
    }
}