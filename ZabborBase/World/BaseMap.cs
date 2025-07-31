// World/BaseMap.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Zabbor.ZabborBase.Entities;
using Zabbor.ZabborBase.Enums;
using Zabbor.ZabborBase.Interfaces;

namespace Zabbor.ZabborBase.World
{
    public abstract class BaseMap : IGameMap
    {
        // Pola chronione (protected) są dostępne dla klas dziedziczących
        protected readonly TileType[,] _tiles;
        protected readonly int _width;
        protected readonly int _height;
        protected readonly List<Npc> _npcs = new List<Npc>();
        protected readonly List<Warp> _warps = new List<Warp>();
        protected List<WorldItem> _worldItems = new List<WorldItem>();

        protected BaseMap(int width, int height)
        {
            _width = width;
            _height = height;
            _tiles = new TileType[width, height];

            // Wywołujemy metody, które będą musiały zaimplementować klasy dziedziczące
            GenerateLayout();
            PlaceNpcs();
            PlaceWarps();
            PlaceItems();
        }

        // Metody abstrakcyjne - każda mapa MUSI dostarczyć własną implementację
        protected abstract void GenerateLayout();
        protected abstract void PlaceNpcs();
        protected abstract void PlaceWarps();
        protected abstract void PlaceItems();

        // --- Poniżej znajduje się wspólna, powtarzalna logika ---

        public virtual bool IsTileWalkable(Point tileCoordinates)
        {
            if (GetWarpAt(tileCoordinates) != null) return true;
            if (tileCoordinates.X < 0 || tileCoordinates.X >= _width || tileCoordinates.Y < 0 || tileCoordinates.Y >= _height) return false;
            if (_npcs.Any(npc => npc.TilePosition == tileCoordinates)) return false;
            return _tiles[tileCoordinates.X, tileCoordinates.Y] == TileType.Walkable;
        }

        public virtual Npc GetNpcAt(Point tilePosition) => _npcs.FirstOrDefault(npc => npc.TilePosition == tilePosition);
        public virtual Warp GetWarpAt(Point tilePosition) => _warps.FirstOrDefault(w => w.SourceTile == tilePosition);
        public virtual WorldItem GetWorldItemAt(Point tilePosition) => _worldItems.FirstOrDefault(i => i.TilePosition == tilePosition);

        public virtual void RemoveWorldItemAt(Point tilePosition) => _worldItems.RemoveAll(i => i.TilePosition == tilePosition);
        public virtual void RemoveItems(List<Point> itemPositions)
        {
            if (itemPositions == null) return;
            _worldItems.RemoveAll(item => itemPositions.Contains(item.TilePosition));
        }

        public abstract void Draw(SpriteBatch spriteBatch); // Rysowanie pozostawiamy każdej mapie, bo używa innych kolorów
    }
}