using Microsoft.Xna.Framework;
using Zabbor.ZabborBase.Enums;

namespace Zabbor.ZabborBase.Combat
{
    public class CombatGrid
    {
        public CombatTile[,] Tiles { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public CombatGrid(int width, int height)
        {
            Width = width;
            Height = height;
            Tiles = new CombatTile[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Tiles[x, y] = new CombatTile(x, y);
                }
            }
        }

        public CombatTile GetTile(int x, int y)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                return Tiles[x, y];
            }
            return null;
        }
    }
}