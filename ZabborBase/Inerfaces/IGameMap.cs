using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Zabbor.ZabborBase.Inerfaces
{
    public interface IGameMap
    {
        bool IsTileWalkable(Point tileCoordinates);
        void Draw(SpriteBatch spriteBatch);
    }
}