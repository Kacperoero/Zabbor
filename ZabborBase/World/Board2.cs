using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Zabbor.ZabborBase.Entities;
using Zabbor.ZabborBase.Enums;
using Zabbor.ZabborBase.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Zabbor.ZabborBase.World
{
    public class Board2 : IGameMap
    {
        private readonly TileType[,] _tiles;
        private readonly int _width;
        private readonly int _height;
        private readonly List<Npc> _npcs = [];
        private readonly List<Warp> _warps = [];
        private readonly List<WorldItem> _worldItems = [];

        public Board2(int width, int height)
        {
            _width = width;
            _height = height;
            _tiles = new TileType[width, height];
            GenerateLayout();
            PlaceWarps();
            PlaceNpcs();
            PlaceItems();
        }

        private void GenerateLayout()
        {
            // Uspójniony layout z ramką, jak w Board1
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (x == 0 || x == _width - 1 || y == 0 || y == _height - 1)
                        _tiles[x, y] = TileType.Solid;
                    else
                        _tiles[x, y] = TileType.Walkable;
                }
            }
            _tiles[10, 10] = TileType.Solid;
            _tiles[11, 10] = TileType.Solid;
        }

        private void PlaceWarps()
        {
            _warps.Clear();
            _warps.Add(new Warp(new Point(0, 19), "Board1", new Point(49, 19)));
        }
        
        // Puste metody, gotowe na przyszłość
        private void PlaceNpcs() { }
        private void PlaceItems() { }

        public bool IsTileWalkable(Point tileCoordinates)
        {
            if (GetWarpAt(tileCoordinates) != null) return true;
            if (tileCoordinates.X < 0 || tileCoordinates.X >= _width || tileCoordinates.Y < 0 || tileCoordinates.Y >= _height) return false;
            if (_npcs.Any(npc => npc.TilePosition == tileCoordinates)) return false;
            return _tiles[tileCoordinates.X, tileCoordinates.Y] == TileType.Walkable;
        }

        public Npc GetNpcAt(Point tilePosition) => _npcs.FirstOrDefault(npc => npc.TilePosition == tilePosition);
        public Warp GetWarpAt(Point tilePosition) => _warps.FirstOrDefault(w => w.SourceTile == tilePosition);
        public WorldItem GetWorldItemAt(Point tilePosition) => _worldItems.FirstOrDefault(i => i.TilePosition == tilePosition);
        public void RemoveWorldItemAt(Point tilePosition) => _worldItems.RemoveAll(i => i.TilePosition == tilePosition);

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
                        tileColor = Color.DarkSlateGray;
                    else
                        tileColor = Color.CornflowerBlue;
                    
                    spriteBatch.Draw(Placeholder.Texture, new Rectangle((int)tilePosition.X, (int)tilePosition.Y, Game1.TILE_SIZE, Game1.TILE_SIZE), tileColor);
                }
            }

            // 2. Rysuj przedmioty
            foreach (var item in _worldItems)
            {
                item.Draw(spriteBatch);
            }

            // 3. Rysuj NPC
            foreach (var npc in _npcs)
            {
                npc.Draw(spriteBatch);
            }
        }
    }
}