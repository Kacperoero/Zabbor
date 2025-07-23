// World/Warp.cs
using Microsoft.Xna.Framework;

namespace Zabbor.ZabborBase.World // Użyj swojej przestrzeni nazw
{
    public class Warp
    {
        // Pozycja portalu na mapie źródłowej
        public Point SourceTile { get; private set; }
        // Nazwa (ID) mapy docelowej
        public string DestinationMapId { get; private set; }
        // Pozycja, na której gracz pojawi się na mapie docelowej
        public Point DestinationTile { get; private set; }

        public Warp(Point sourceTile, string destinationMapId, Point destinationTile)
        {
            SourceTile = sourceTile;
            DestinationMapId = destinationMapId;
            DestinationTile = destinationTile;
        }
    }
}