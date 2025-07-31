using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Zabbor.Core;
using Zabbor.ZabborBase.Entities;
using Zabbor.ZabborBase.Enums;

namespace Zabbor.ZabborBase.World
{
    public class Board1 : BaseMap
    {
        // Przekazujemy wymiary do konstruktora klasy bazowej
        public Board1(int width, int height) : base(width, height) { }

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
            _tiles[5, 5] = TileType.Solid;
            _tiles[5, 6] = TileType.Solid;
            _tiles[6, 5] = TileType.Solid;
            _tiles[10, 12] = TileType.Solid;
            _tiles[11, 12] = TileType.Solid;
        }

        protected override void PlaceNpcs()
        {
            _npcs.Add(new Npc(new Point(8, 8), Color.Yellow, GameIDs.Dialogues.NpcYellowGreeting));
            _npcs.Add(new Npc(new Point(15, 12), Color.Cyan, GameIDs.Dialogues.NpcCyanSwordLegend));
        }

        protected override void PlaceWarps()
        {
            _warps.Add(new Warp(new Point(49, 19), "Board2", new Point(0, 19)));
            _warps.Add(new Warp(new Point(18, 15), "Board2", new Point(10, 11)));
        }

        protected override void PlaceItems()
        {
            _worldItems.Add(new WorldItem(new Point(10, 15), GameIDs.Items.PotionHealthSmall));
            _worldItems.Add(new WorldItem(new Point(20, 22), GameIDs.Items.KeyRusty));
        }

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
                        tileColor = new Color(139, 69, 19); // Brązowy
                    else
                        tileColor = Color.DarkGreen; // Ciemnozielony
                    
                    spriteBatch.Draw(Placeholder.Texture, new Rectangle((int)tilePosition.X, (int)tilePosition.Y, Game1.TILE_SIZE, Game1.TILE_SIZE), tileColor);
                }
            }
            foreach (var item in _worldItems) item.Draw(spriteBatch);
            foreach (var npc in _npcs) npc.Draw(spriteBatch);
        }
    }
}