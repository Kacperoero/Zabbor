using Zabbor.ZabborBase.Enums;
using Zabbor.ZabborBase.Models;

namespace Zabbor.ZabborBase.Entities
{
    public class Character
    {
        public string Name { get; set; }
        public CharacterClass Class { get; set; }
        public CharacterStats Stats { get; set; }

        public Character(string name, CharacterClass characterClass, CharacterStats stats)
        {
            Name = name;
            Class = characterClass;
            Stats = stats;
        }
    }
}