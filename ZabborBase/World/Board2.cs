using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Zabbor.ZabborBase.Entities;
using Zabbor.ZabborBase.Enums;

namespace Zabbor.ZabborBase.World
{
    public class Board2 : BaseMap
    {
        public Board2(int width, int height) : base(width, height) { }

        protected override void GenerateLayout()
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    _tiles[x, y] = (x == 0 || x == _width - 1 || y == 0 || y == _height - 1)
                        ? TileType.Solid
                        : TileType.Walkable;
                }
            }
            _tiles[10, 10] = TileType.Solid;
            _tiles[11, 10] = TileType.Solid;
        }

        protected override void PlaceNpcs() { } // Brak NPC na tej mapie

        protected override void PlaceWarps()
        {
            _warps.Add(new Warp(new Point(0, 19), "Board1", new Point(49, 19)));
        }

        protected override void PlaceItems() { } // Brak przedmiotów na tej mapie

        public override void Draw(SpriteBatch spriteBatch)
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    var tilePosition = new Vector2(x * Game1.TILE_SIZE, y * Game1.TILE_SIZE);
                    var currentTilePoint = new Point(x, y);
                    Color tileColor;

                    if (GetWarpAt(currentTilePoint) != null)
                        tileColor = Color.Purple;
                    else if (_tiles[x, y] == TileType.Solid)
                        tileColor = Color.DarkSlateGray;
                    else
                        tileColor = Color.CornflowerBlue;
                    
                    spriteBatch.Draw(Placeholder.Texture, new Rectangle((int)tilePosition.X, (int)tilePosition.Y, Game1.TILE_SIZE, Game1.TILE_SIZE), tileColor);
                }
            }
            foreach (var item in _worldItems) item.Draw(spriteBatch);
            foreach (var npc in _npcs) npc.Draw(spriteBatch);
        }
    }
}