// UI/MainMenuScreen.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Zabbor.ZabborBase.Enums;
using Zabbor.ZabborBase.Managers;
using System.Collections.Generic;
using System.Linq;

namespace Zabbor.ZabborBase.UI // Użyj swojej przestrzeni nazw
{
    public class MainMenuScreen
    {
        private SpriteFont _font;
        private Viewport _viewport;
        private List<string> _menuItems = new List<string> { "Rozpocznij Gre", "Wyjdz" };
        private int _selectedIndex = 0;
        private KeyboardState _previousKeyboardState;

        public MainMenuScreen(SpriteFont font, Viewport viewport)
        {
            _font = font;
            _viewport = viewport;
            if (Enumerable.Range(0, 10).Any(i => SaveManager.SaveFileExists(i)))
            {
                _menuItems.Insert(1, "Wczytaj Gre");
            }
        }

        public GameState Update()
        {
            var kState = Keyboard.GetState();

            // Obsługa strzałek w dół i w górę
            if (kState.IsKeyDown(Keys.Down) && _previousKeyboardState.IsKeyUp(Keys.Down))
            {
                _selectedIndex = (_selectedIndex + 1) % _menuItems.Count;
            }
            if (kState.IsKeyDown(Keys.Up) && _previousKeyboardState.IsKeyUp(Keys.Up))
            {
                _selectedIndex--;
                if (_selectedIndex < 0)
                {
                    _selectedIndex = _menuItems.Count - 1;
                }
            }

            // Obsługa klawisza Enter
            if (kState.IsKeyDown(Keys.Enter) && _previousKeyboardState.IsKeyUp(Keys.Enter))
            {
                string selectedItem = _menuItems[_selectedIndex];
                switch (selectedItem)
                {
                    case "Rozpocznij Gre": return GameState.NewGame;
                    case "Wczytaj Gre": return GameState.ShowLoadScreen;
                    case "Wyjdz": return GameState.Exit;
                }
            }
            
            _previousKeyboardState = kState;
            return GameState.MainMenu;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            string title = "Zabbor"; // Zmieniony tytuł
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