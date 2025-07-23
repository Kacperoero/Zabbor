using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Zabbor.ZabborBase.Entities;
using Zabbor.ZabborBase.Enums;
using Zabbor.ZabborBase.Interfaces;

namespace Zabbor.ZabborBase.World
{
    public class Board1 : IGameMap
    {
        private readonly TileType[,] _tiles;
        private readonly int _width;
        private readonly int _height;
        private readonly List<Npc> _npcs = [];
        private readonly List<Warp> _warps = [];
        private readonly List<WorldItem> _worldItems = [];

        public Board1(int width, int height)
        {
            _width = width;
            _height = height;
            _tiles = new TileType[width, height];

            GenerateLayout();
            PlaceNpcs();
            PlaceWarps();
            PlaceItems();
        }

        private void PlaceWarps()
        {
            _warps.Clear();
            // 1. Przejście przy krawędzi mapy

            _warps.Add(new Warp(new Point(49, 19), "Board2", new Point(0, 19)));

            // 2. Przejście typu "drzwi" wewnątrz mapy
            _warps.Add(new Warp(new Point(18, 15), "Board2", new Point(10, 11)));
        }

        public Warp GetWarpAt(Point tilePosition)
        {
            return _warps.FirstOrDefault(w => w.SourceTile == tilePosition);
        }

        public Npc GetNpcAt(Point tilePosition)
        {
            return _npcs.FirstOrDefault(npc => npc.TilePosition == tilePosition);
        }

        private void PlaceNpcs()
        {
            _npcs.Add(new Npc(new Point(8, 8), Color.Yellow, "npc_yellow_greeting"));
            _npcs.Add(new Npc(new Point(15, 12), Color.Cyan, "npc_cyan_sword_legend"));
        }

        private void PlaceItems()
        {
            _worldItems.Add(new WorldItem(new Point(10, 15), "potion_health_small"));
            _worldItems.Add(new WorldItem(new Point(20, 22), "key_rusty"));
        }

        public WorldItem GetWorldItemAt(Point tilePosition) => _worldItems.FirstOrDefault(i => i.TilePosition == tilePosition);
        public void RemoveWorldItemAt(Point tilePosition) => _worldItems.RemoveAll(i => i.TilePosition == tilePosition);

        public bool IsTileWalkable(Point tileCoordinates)
        {
            // PRIORYTET 1: Sprawdź, czy pole jest portalem. Jeśli tak, zawsze można na nie wejść.
            if (GetWarpAt(tileCoordinates) != null)
            {
                return true;
            }

            // PRIORYTET 2: Sprawdź granice mapy.
            if (tileCoordinates.X < 0 || tileCoordinates.X >= _width ||
                tileCoordinates.Y < 0 || tileCoordinates.Y >= _height)
            {
                return false;
            }

            // PRIORYTET 3: Sprawdź, czy na polu stoi NPC.
            if (_npcs.Any(npc => npc.TilePosition == tileCoordinates))
            {
                return false;
            }

            // Na końcu sprawdź typ podłoża z mapy.
            return _tiles[tileCoordinates.X, tileCoordinates.Y] == TileType.Walkable;
        }

        private void GenerateLayout()
        {
            // To jest unikalny układ dla "Board1"
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (x == 0 || x == _width - 1 || y == 0 || y == _height - 1)
                    {
                        _tiles[x, y] = TileType.Solid;
                    }
                    else
                    {
                        _tiles[x, y] = TileType.Walkable;
                    }
                }
            }

            _tiles[5, 5] = TileType.Solid;
            _tiles[5, 6] = TileType.Solid;
            _tiles[6, 5] = TileType.Solid;
            _tiles[10, 12] = TileType.Solid;
            _tiles[11, 12] = TileType.Solid;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // 1. Rysuj kafelki mapy
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
                        tileColor = new Color(139, 69, 19); // Brązowy dla Board1
                    else
                        tileColor = Color.DarkGreen; // Ciemnozielony dla Board1
                    
                    spriteBatch.Draw(Placeholder.Texture, new Rectangle((int)tilePosition.X, (int)tilePosition.Y, Game1.TILE_SIZE, Game1.TILE_SIZE), tileColor);
                }
            }

            // 2. Rysuj przedmioty leżące na ziemi
            // ----> UPEWNIJ SIĘ, ŻE TA PĘTLA ISTNIEJE <----
            foreach (var item in _worldItems)
            {
                item.Draw(spriteBatch);
            }

            // 3. Rysuj NPC (na wierzchu)
            foreach (var npc in _npcs)
            {
                npc.Draw(spriteBatch);
            }
        }
    }
}