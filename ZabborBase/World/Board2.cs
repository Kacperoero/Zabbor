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

        public Board2(int width, int height)
        {
            _width = width;
            _height = height;
            _tiles = new TileType[width, height];
            GenerateLayout();
            PlaceWarps();
        }

        private void GenerateLayout()
        {
            // Inny układ dla drugiej mapy
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    _tiles[x, y] = TileType.Walkable;
                }
            }
            _tiles[10, 10] = TileType.Solid;
            _tiles[11, 10] = TileType.Solid;
        }

        private void PlaceWarps()
        {
            _warps.Clear();
            // Portal powrotny z lewej krawędzi Board2 na prawą krawędź Board1
            _warps.Add(new Warp(new Point(0, 19), "Board1", new Point(49, 19)));
        }

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

        public Npc GetNpcAt(Point tilePosition) => null;
        public Warp GetWarpAt(Point tilePosition) => _warps.FirstOrDefault(w => w.SourceTile == tilePosition);

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    var tilePosition = new Vector2(x * Game1.TILE_SIZE, y * Game1.TILE_SIZE);
                    var currentTilePoint = new Point(x, y);
                    Color tileColor;

                    // ---- NOWA LOGIKA KOLOROWANIA ----
                    if (GetWarpAt(currentTilePoint) != null)
                    {
                        tileColor = Color.Purple;
                    }
                    else if (_tiles[x, y] == TileType.Solid)
                    {
                        tileColor = Color.DarkSlateGray;
                    }
                    else
                    {
                        tileColor = Color.CornflowerBlue;
                    }
                    
                    spriteBatch.Draw(Placeholder.Texture, new Rectangle((int)tilePosition.X, (int)tilePosition.Y, Game1.TILE_SIZE, Game1.TILE_SIZE), tileColor);
                }
            }
        }
    }
}