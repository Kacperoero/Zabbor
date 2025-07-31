using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Zabbor.ZabborBase.Models
{
    public class SaveData
    {
        public string SaveName { get; set; }
        public string CurrentMapId { get; set; }
        public SerializablePoint PlayerTilePosition { get; set; }
        public Dictionary<string, int> PlayerInventory { get; set; }
        public Dictionary<string, List<SerializablePoint>> RemovedWorldItems { get; set; }
    }
}