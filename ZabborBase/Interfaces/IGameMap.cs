using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Zabbor.ZabborBase.Entities;

namespace Zabbor.ZabborBase.Interfaces
{
    public interface IGameMap
    {
        bool IsTileWalkable(Point tileCoordinates);
        void Draw(SpriteBatch spriteBatch);
        Npc GetNpcAt(Point tilePosition);
    }
}