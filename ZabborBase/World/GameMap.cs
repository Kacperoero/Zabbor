// World/GameMap.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Zabbor.ZabborBase.Entities;
using Zabbor.ZabborBase.Enums;
using Zabbor.ZabborBase.Interfaces;
using Zabbor.ZabborBase.Models;

namespace Zabbor.ZabborBase.World
{
    // Klasy pomocnicze do wczytywania danych z JSON (bez zmian)
    public class MapData {
        public string WalkableColor { get; set; }
        public string SolidColor { get; set; }
        public List<string> Tiles { get; set; }
        public List<NpcData> Npcs { get; set; }
        public List<ItemData> Items { get; set; }
        public List<WarpData> Warps { get; set; }
    }
    public class NpcData {
        public int TileX { get; set; }
        public int TileY { get; set; }
        public string Color { get; set; }
        public string DialogueId { get; set; }
    }
    public class ItemData {
        public int TileX { get; set; }
        public int TileY { get; set; }
        public string ItemId { get; set; }
    }
    public class WarpData {
        public int SourceX { get; set; }
        public int SourceY { get; set; }
        public string DestMapId { get; set; }
        public int DestX { get; set; }
        public int DestY { get; set; }
    }

    public class GameMap : IGameMap
    {
        private TileType[,] _tiles;
        private int _width;
        private int _height;
        private List<Npc> _npcs = new List<Npc>();
        private List<Warp> _warps = new List<Warp>();
        private List<WorldItem> _worldItems = new List<WorldItem>();
        private Color _walkableColor;
        private Color _solidColor;

        public GameMap(string mapId)
        {
            LoadMapFromFile(mapId);
        }

        private void LoadMapFromFile(string mapId)
        {
            // Budujemy pełną, poprawną ścieżkę do pliku JSON w folderze Content
            string mapPath = Path.Combine(AppContext.BaseDirectory, "Content", $"{mapId}.json");

            // Sprawdzamy, czy plik istnieje, na wypadek literówki w ID mapy
            if (!File.Exists(mapPath))
            {
                throw new FileNotFoundException($"Nie znaleziono pliku mapy: {mapPath}");
            }

            // Wczytujemy plik i parsujemy go
            string jsonString = File.ReadAllText(mapPath);

            // Tworzymy opcje, które ignorują wielkość liter w nazwach właściwości
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var mapData = JsonSerializer.Deserialize<MapData>(jsonString, options);

            // Reszta metody pozostaje bez zmian...
            _walkableColor = GetColorFromString(mapData.WalkableColor);
            _solidColor = GetColorFromString(mapData.SolidColor);

            _height = mapData.Tiles.Count;
            _width = mapData.Tiles[0].Length;
            _tiles = new TileType[_width, _height];

            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    char tileChar = mapData.Tiles[y][x];
                    if (tileChar == '#' || tileChar == 'W' || tileChar == 'D')
                        _tiles[x, y] = TileType.Solid;
                    else
                        _tiles[x, y] = TileType.Walkable;
                }
            }
            
            foreach (var npcData in mapData.Npcs)
                _npcs.Add(new Npc(new Point(npcData.TileX, npcData.TileY), GetColorFromString(npcData.Color), npcData.DialogueId));

            foreach (var itemData in mapData.Items)
                _worldItems.Add(new WorldItem(new Point(itemData.TileX, itemData.TileY), itemData.ItemId));

            foreach (var warpData in mapData.Warps)
                _warps.Add(new Warp(new Point(warpData.SourceX, warpData.SourceY), warpData.DestMapId, new Point(warpData.DestX, warpData.DestY)));
        }

        // ---- NOWA METODA POMOCNICZA ----
        private Color GetColorFromString(string colorName)
        {
            return colorName switch
            {
                "DarkGreen" => Color.DarkGreen,
                "SaddleBrown" => Color.SaddleBrown,
                "CornflowerBlue" => Color.CornflowerBlue,
                "DarkSlateGray" => Color.DarkSlateGray,
                "Yellow" => Color.Yellow,
                "Cyan" => Color.Cyan,
                _ => Color.Magenta, // Domyślny, jaskrawy kolor na wypadek błędu w JSON
            };
        }

        // --- Reszta metod bez zmian ---
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
        public void RemoveItems(List<Point> itemPositions)
        {
            if (itemPositions == null) return;
            _worldItems.RemoveAll(item => itemPositions.Contains(item.TilePosition));
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    var tilePosition = new Vector2(x * Game1.TILE_SIZE, y * Game1.TILE_SIZE);
                    var currentTilePoint = new Point(x, y);
                    Color tileColor;

                    if (GetWarpAt(currentTilePoint) != null) tileColor = Color.Purple;
                    else if (_tiles[x, y] == TileType.Solid) tileColor = _solidColor;
                    else tileColor = _walkableColor;
                    
                    spriteBatch.Draw(Placeholder.Texture, new Rectangle((int)tilePosition.X, (int)tilePosition.Y, Game1.TILE_SIZE, Game1.TILE_SIZE), tileColor);
                }
            }
            foreach (var item in _worldItems) item.Draw(spriteBatch);
            foreach (var npc in _npcs) npc.Draw(spriteBatch);
        }
    }
}