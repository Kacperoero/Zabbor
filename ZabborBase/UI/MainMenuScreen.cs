// UI/MainMenuScreen.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Zabbor.ZabborBase.Enums;
using Zabbor.ZabborBase.Managers;
using System.Collections.Generic;
using System.Linq;
using Zabbor.Core; // Upewnij się, że ten using jest dodany

namespace Zabbor.ZabborBase.UI // Użyj swojej przestrzeni nazw
{
    public class MainMenuScreen
    {
        private SpriteFont _font;
        private Viewport _viewport;
        
        // Lista jest inicjalizowana jako pusta. Wypełni ją konstruktor.
        private readonly List<string> _menuItems = new List<string>();
        private int _selectedIndex = 0;

        public MainMenuScreen(SpriteFont font, Viewport viewport, bool isGameInProgress)
        {
            _font = font;
            _viewport = viewport;

            // Logika budowania listy opcji od zera
            if (isGameInProgress)
            {
                _menuItems.Add("Wróć do gry");
            }
            
            _menuItems.Add("Nowa Gra");

            // Sprawdzamy, czy istnieje JAKIKOLWIEK plik zapisu od 0 do 9
            if (Enumerable.Range(0, 10).Any(i => SaveManager.SaveFileExists(i)))
            {
                _menuItems.Add("Wczytaj Grę");
            }

            _menuItems.Add("Wyjdź");
        }

        public GameState Update()
        {
            if (InputManager.WasKeyPressed(Keys.Down))
            {
                _selectedIndex = (_selectedIndex + 1) % _menuItems.Count;
            }
            if (InputManager.WasKeyPressed(Keys.Up))
            {
                _selectedIndex--;
                if (_selectedIndex < 0)
                {
                    _selectedIndex = _menuItems.Count - 1;
                }
            }

            if (InputManager.WasKeyPressed(Keys.Enter))
            {
                string selectedItem = _menuItems[_selectedIndex];
                switch (selectedItem)
                {
                    case "Wróć do gry": return GameState.ResumeGame;
                    case "Nowa Gra": return GameState.NewGame;
                    case "Wczytaj Grę": return GameState.ShowLoadScreen;
                    case "Wyjdź": return GameState.Exit;
                }
            }
            return GameState.MainMenu;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            string title = "Zabbor";
            var titlePosition = new Vector2(
                _viewport.Width / 2f - _font.MeasureString(title).X / 2f,
                _viewport.Height / 3f);

            spriteBatch.DrawString(_font, title, titlePosition, Color.White);

            for (int i = 0; i < _menuItems.Count; i++)
            {
                var color = (i == _selectedIndex) ? Color.Yellow : Color.White;
                var itemText = _menuItems[i];
                var itemPosition = new Vector2(
                    _viewport.Width / 2f - _font.MeasureString(itemText).X / 2f,
                    _viewport.Height / 2f + i * 40);

                spriteBatch.DrawString(_font, itemText, itemPosition, color);
            }
        }
    }
}