// UI/InventoryScreen.cs
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Zabbor.ZabborBase.Managers;

namespace Zabbor.ZabborBase.UI
{
    public class InventoryScreen
    {
        private SpriteFont _font;
        private Rectangle _panel;

        public InventoryScreen(SpriteFont font, GraphicsDevice graphicsDevice)
        {
            _font = font;
            var panelWidth = 400;
            var panelHeight = 500;
            _panel = new Rectangle(
                graphicsDevice.Viewport.Width / 2 - panelWidth / 2,
                graphicsDevice.Viewport.Height / 2 - panelHeight / 2,
                panelWidth, panelHeight);
        }

        public void Draw(SpriteBatch spriteBatch, Dictionary<string, int> items)
        {
            spriteBatch.Draw(Placeholder.Texture, _panel, Color.DarkBlue * 0.9f);

            var position = new Vector2(_panel.X + 20, _panel.Y + 20);
            spriteBatch.DrawString(_font, "Ekwipunek (Nacisnij I aby zamknac)", position, Color.White);
            position.Y += 40;

            if (items.Count == 0)
            {
                spriteBatch.DrawString(_font, "Pusto", position, Color.Gray);
            }
            else
            {
                foreach (var itemEntry in items)
                {
                    var item = ItemManager.GetItem(itemEntry.Key);
                    var text = $"{item.Name} x{itemEntry.Value}";
                    spriteBatch.DrawString(_font, text, position, Color.White);
                    position.Y += 30;
                }
            }
        }
    }
}