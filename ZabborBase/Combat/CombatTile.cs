using Microsoft.Xna.Framework;
using Zabbor.ZabborBase.Entities;
using Zabbor.ZabborBase.Enums;

namespace Zabbor.ZabborBase.Combat
{
    public class CombatTile
    {
        public Point Position { get; private set; }
        public CombatTileType Type { get; private set; }
        public Character Occupant { get; private set; }

        public bool IsWalkable => Type != CombatTileType.Obstacle && Occupant == null;

        public CombatTile(int x, int y, CombatTileType type = CombatTileType.Normal)
        {
            Position = new Point(x, y);
            Type = type;
            Occupant = null;
        }

        public void SetOccupant(Character character)
        {
            Occupant = character;
        }

        public void ClearOccupant()
        {
            Occupant = null;
        }
    }
}