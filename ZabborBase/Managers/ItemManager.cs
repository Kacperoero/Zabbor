using System.Collections.Generic;
using System.Linq;
using Zabbor.ZabborBase.Models;

namespace Zabbor.ZabborBase.Managers
{
    public static class ItemManager
    {
        private static readonly List<Item> _items =
        [
            new Item { Id = "potion_health_small", Name = "Mala Mikstura Zdrowia" },
            new Item { Id = "key_rusty", Name = "Zardzewialy Klucz" }
        ];

        public static Item GetItem(string id) => _items.FirstOrDefault(i => i.Id == id);
    }
}