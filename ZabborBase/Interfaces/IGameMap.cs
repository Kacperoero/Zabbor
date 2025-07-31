using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Zabbor.ZabborBase.Entities;
using Zabbor.ZabborBase.World;

namespace Zabbor.ZabborBase.Interfaces
{
    public interface IGameMap
    {
        bool IsTileWalkable(Point tileCoordinates);
        void Draw(SpriteBatch spriteBatch);
        Npc GetNpcAt(Point tilePosition);
        Warp GetWarpAt(Point tilePosition);
        WorldItem GetWorldItemAt(Point tilePosition);
        void RemoveWorldItemAt(Point tilePosition);
        void RemoveItems(List<Point> itemPositions);
    }
}