using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Zabbor.ZabborBase.Entities
{
    public class Npc
    {
        public Point TilePosition { get; private set; }
        public string DialogueId { get; private set; }
        private readonly Placeholder _graphics;

        public Npc(Point tilePosition, Color color, string dialogueId)
        {
            TilePosition = tilePosition;
            DialogueId = dialogueId;
            var screenPosition = new Vector2(tilePosition.X * Game1.TILE_SIZE, tilePosition.Y * Game1.TILE_SIZE);
            _graphics = new Placeholder(screenPosition, new Point(Game1.TILE_SIZE), color);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _graphics.Draw(spriteBatch);
        }
    }
}