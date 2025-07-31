using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Zabbor.Screens;

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
            var screenPosition = new Vector2(tilePosition.X * GameplayScreen.TILE_SIZE, tilePosition.Y * GameplayScreen.TILE_SIZE);
            _graphics = new Placeholder(screenPosition, new Point(GameplayScreen.TILE_SIZE), color);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _graphics.Draw(spriteBatch);
        }
    }
}