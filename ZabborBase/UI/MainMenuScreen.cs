using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Zabbor.ZabborBase.Enums;

namespace Zabbor.ZabborBase.UI
{
    public class MainMenuScreen
    {
        private SpriteFont _font;
        private Viewport _viewport;

        // ---- Nowe pola dla interaktywnego menu ----
        private readonly string[] _menuItems = ["Rozpocznij Gre", "Wyjdz"];
        private int _selectedIndex = 0;
        private KeyboardState _previousKeyboardState;


        public MainMenuScreen(SpriteFont font, Viewport viewport)
        {
            _font = font;
            _viewport = viewport;
        }

        public GameState Update()
        {
            var kState = Keyboard.GetState();

            // Obsługa strzałek w dół i w górę
            if (kState.IsKeyDown(Keys.Down) && _previousKeyboardState.IsKeyUp(Keys.Down))
            {
                _selectedIndex++;
                if (_selectedIndex >= _menuItems.Length)
                {
                    _selectedIndex = 0; // Zawijanie na początek listy
                }
            }
            if (kState.IsKeyDown(Keys.Up) && _previousKeyboardState.IsKeyUp(Keys.Up))
            {
                _selectedIndex--;
                if (_selectedIndex < 0)
                {
                    _selectedIndex = _menuItems.Length - 1; // Zawijanie na koniec listy
                }
            }
            
            // Zapisujemy stan klawiatury na koniec, aby w następnej klatce wiedzieć, co było wciśnięte
            _previousKeyboardState = kState;


            // Obsługa klawisza Enter
            if (kState.IsKeyDown(Keys.Enter))
            {
                // Zwróć stan w zależności od wybranej opcji
                switch (_selectedIndex)
                {
                    case 0: // "Rozpocznij Gre"
                        return GameState.Gameplay;
                    case 1: // "Wyjdz"
                        return GameState.Exit;
                }
            }

            return GameState.MainMenu; // Domyślnie pozostań w menu
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            string title = "Zabbor";
            var titlePosition = new Vector2(
                _viewport.Width / 2 - _font.MeasureString(title).X / 2,
                _viewport.Height / 3);

            spriteBatch.DrawString(_font, title, titlePosition, Color.White);

            // Rysujemy opcje menu w pętli
            for (int i = 0; i < _menuItems.Length; i++)
            {
                // Zmieniamy kolor, jeśli opcja jest zaznaczona
                var color = (i == _selectedIndex) ? Color.Yellow : Color.White;
                
                var itemText = _menuItems[i];
                var itemPosition = new Vector2(
                    _viewport.Width / 2 - _font.MeasureString(itemText).X / 2,
                    _viewport.Height / 2 + i * 40); // 40 pikseli odstępu między opcjami

                spriteBatch.DrawString(_font, itemText, itemPosition, color);
            }
        }
    }
}