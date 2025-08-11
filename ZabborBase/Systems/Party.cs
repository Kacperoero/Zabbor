using System.Collections.Generic;
using Zabbor.ZabborBase.Entities;

namespace Zabbor.ZabborBase.Systems
{
    public class Party
    {
        public List<Character> Members { get; private set; }
        public Inventory SharedInventory { get; private set; }

        public Party()
        {
            Members = new List<Character>();
            SharedInventory = new Inventory();
        }

        public void AddMember(Character character)
        {
            if (Members.Count < 3)
            {
                Members.Add(character);
            }
        }
    }
}