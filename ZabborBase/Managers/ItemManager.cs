using System.Collections.Generic;
using System.Linq;
using Zabbor.Core;
using Zabbor.ZabborBase.Models;

namespace Zabbor.ZabborBase.Managers
{
    public static class ItemManager
    {
        private static readonly List<Item> _items = new List<Item>
        {
            // Używamy stałych zamiast stringów
            new Item { Id = GameIDs.Items.PotionHealthSmall, Name = "Mala Mikstura Zdrowia" },
            new Item { Id = GameIDs.Items.KeyRusty, Name = "Zardzewialy Klucz" }
        };

        public static Item GetItem(string id) => _items.FirstOrDefault(i => i.Id == id);
    }
}