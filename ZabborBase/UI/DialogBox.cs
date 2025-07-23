// UI/DialogBox.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Zabbor.ZabborBase.UI // Użyj swojej przestrzeni nazw
{
    public class DialogBox
    {
        private readonly string _text;
        private readonly Rectangle _boxRectangle;
        private readonly Vector2 _textPosition;
        private readonly SpriteFont _font;
        private readonly Vector2 _hintPosition;

        public DialogBox(string text, SpriteFont font, GraphicsDevice graphicsDevice)
        {
            _text = text;
            _font = font;

            int boxHeight = 120;
            _boxRectangle = new Rectangle(
                20, 
                graphicsDevice.Viewport.Height - boxHeight - 20, 
                graphicsDevice.Viewport.Width - 40, 
                boxHeight);

            _textPosition = new Vector2(_boxRectangle.X + 20, _boxRectangle.Y + 20);

            // Obliczamy pozycję dla podpowiedzi
            string hintText = "Zamknij (Q)";
            var hintSize = _font.MeasureString(hintText);
            _hintPosition = new Vector2(
                _boxRectangle.Right - hintSize.X - 20,
                _boxRectangle.Bottom - hintSize.Y - 10
            );
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Rysujemy tło okna dialogowego
            spriteBatch.Draw(Placeholder.Texture, _boxRectangle, Color.Black * 0.8f);

            // Rysujemy główny tekst
            spriteBatch.DrawString(_font, _text, _textPosition, Color.White);

            // Rysujemy podpowiedź
            spriteBatch.DrawString(_font, "Zamknij (Q)", _hintPosition, Color.Gray);
        }
    }
}