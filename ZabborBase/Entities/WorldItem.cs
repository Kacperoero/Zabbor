using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Zabbor.Screens;

namespace Zabbor.ZabborBase.Entities
{
    public class WorldItem
    {
        public Point TilePosition { get; private set; }
        public string ItemId { get; private set; }
        private Placeholder _graphics;

        public WorldItem(Point tilePosition, string itemId)
        {
            TilePosition = tilePosition;
            ItemId = itemId;
            var screenPosition = new Vector2(tilePosition.X * GameplayScreen.TILE_SIZE, tilePosition.Y * GameplayScreen.TILE_SIZE);
            // Przedmioty na mapie będą małymi, czerwonymi kwadratami
            _graphics = new Placeholder(screenPosition, new Point(16, 16), Color.Red);
        }

        public void Draw(SpriteBatch spriteBatch) => _graphics.Draw(spriteBatch);
    }
}