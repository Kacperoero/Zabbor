// Systems/Inventory.cs
using System.Collections.Generic;
using Zabbor.ZabborBase.Managers;
using Zabbor.ZabborBase.Models;

namespace Zabbor.ZabborBase.Systems // Użyj swojej przestrzeni nazw
{
    public class Inventory
    {
        private readonly Dictionary<string, int> _itemCounts = new Dictionary<string, int>();

        public void AddItem(string itemId)
        {
            if (_itemCounts.ContainsKey(itemId))
                _itemCounts[itemId]++;
            else
                _itemCounts[itemId] = 1;
        }

        public Dictionary<string, int> GetItems() => _itemCounts;
    }
}