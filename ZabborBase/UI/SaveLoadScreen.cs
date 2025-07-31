// UI/SaveLoadScreen.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using Zabbor.ZabborBase.Managers;
using Zabbor.ZabborBase.Models;

namespace Zabbor.ZabborBase.UI
{
    public enum SaveLoadMode { Save, Load }

    public class SaveLoadScreen
    {
        private readonly SpriteFont _font;
        private readonly Viewport _viewport;
        private readonly SaveLoadMode _mode;
        private readonly List<SaveData> _slots;
        private int _selectedIndex = 0;
        private KeyboardState _previousKeyboardState;

        public SaveLoadScreen(SpriteFont font, Viewport viewport, SaveLoadMode mode, KeyboardState initialKeyboardState)
        {
            _font = font;
            _viewport = viewport;
            _mode = mode;
            _slots = SaveManager.GetAllSaveSlotsInfo();
            
            // Zapisujemy stan klawiatury z momentu tworzenia ekranu
            _previousKeyboardState = initialKeyboardState;
        }

        // Metoda zwraca wybrany slot (0-9) lub -1 jeśli akcja została anulowana
        public int Update()
        {
            var kState = Keyboard.GetState();

            if (kState.IsKeyDown(Keys.Escape) && _previousKeyboardState.IsKeyUp(Keys.Escape)) return -1; // Anuluj

            if (kState.IsKeyDown(Keys.Down) && _previousKeyboardState.IsKeyUp(Keys.Down))
                _selectedIndex = (_selectedIndex + 1) % _slots.Count;
            if (kState.IsKeyDown(Keys.Up) && _previousKeyboardState.IsKeyUp(Keys.Up))
            {
                _selectedIndex--;
                if (_selectedIndex < 0) _selectedIndex = _slots.Count - 1;
            }

            if (kState.IsKeyDown(Keys.Enter) && _previousKeyboardState.IsKeyUp(Keys.Enter))
            {
                // W trybie wczytywania można wybrać tylko niepusty slot
                if (_mode == SaveLoadMode.Load && _slots[_selectedIndex] == null)
                {
                    // Nic nie rób
                }
                else
                {
                    return _selectedIndex; // Zwróć wybrany slot
                }
            }

            _previousKeyboardState = kState;
            return -2; // Wartość oznaczająca "nic się nie stało, pozostań na ekranie"
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            string title = (_mode == SaveLoadMode.Save) ? "Zapisz Gre" : "Wczytaj Gre";
            var titlePos = new Vector2(_viewport.Width / 2f - _font.MeasureString(title).X / 2f, 50);
            spriteBatch.DrawString(_font, title, titlePos, Color.White);

            for (int i = 0; i < _slots.Count; i++)
            {
                var color = (i == _selectedIndex) ? Color.Yellow : Color.White;
                var slotText = $"Slot {i + 1}: ";
                slotText += _slots[i]?.SaveName ?? "[ Pusty ]";

                var textPos = new Vector2(100, 150 + i * 40);
                spriteBatch.DrawString(_font, slotText, textPos, color);
            }
        }
    }
}