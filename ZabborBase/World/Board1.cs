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
        private readonly List<Npc> _npcs = new List<Npc>();

        public Board1(int width, int height)
        {
            _width = width;
            _height = height;
            _tiles = new TileType[width, height];

            GenerateLayout();
            PlaceNpcs();
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

        public bool IsTileWalkable(Point tileCoordinates)
        {
            if (tileCoordinates.X < 0 || tileCoordinates.X >= _width ||
                tileCoordinates.Y < 0 || tileCoordinates.Y >= _height)
            {
                return false;
            }

            // ZMODYFIKOWANA LOGIKA KOLIZJI
            // Sprawdź, czy na docelowym polu stoi jakiś NPC
            if (_npcs.Any(npc => npc.TilePosition == tileCoordinates))
            {
                return false;
            }

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
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    var tilePosition = new Vector2(x * Game1.TILE_SIZE, y * Game1.TILE_SIZE);
                    var tileColor = _tiles[x, y] == TileType.Solid ? new Color(139, 69, 19) : Color.DarkGreen;

                    spriteBatch.Draw(Placeholder.Texture,
                        new Rectangle((int)tilePosition.X, (int)tilePosition.Y, Game1.TILE_SIZE, Game1.TILE_SIZE),
                        tileColor);
                }
            }
            // NOWA PĘTLA DO RYSOWANIA NPC
            foreach (var npc in _npcs)
            {
                npc.Draw(spriteBatch);
            }
        }
    }
}