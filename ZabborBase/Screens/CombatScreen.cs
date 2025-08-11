using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Zabbor.ZabborBase.Enums;
using Zabbor.Core;
using Microsoft.Xna.Framework.Input;

namespace Zabbor.Screens
{
    public class CombatScreen
    {
        private SpriteFont _font;

        public CombatScreen(SpriteFont font)
        {
            _font = font;
        }

        // Metoda, która będzie uruchamiana przy rozpoczęciu walki
        public void StartCombat()
        {
            // Na razie pusta, w przyszłości będzie tu np. ładowanie przeciwników
        }

        public GameState Update(GameTime gameTime)
        {
            if (InputManager.WasKeyPressed(Keys.Q))
            {
                return GameState.Gameplay;
            }

            return GameState.Combat;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            string text = "Walka Rozpoczeta!\n(Nacisnij Q aby wrocic)";
            var textSize = _font.MeasureString(text);
            var textPosition = new Vector2(
                (1280 - textSize.X) / 2,
                (720 - textSize.Y) / 2);

            spriteBatch.DrawString(_font, text, textPosition, Color.White);
        }
    }
}