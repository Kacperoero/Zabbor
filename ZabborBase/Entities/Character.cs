using System;
using Zabbor.ZabborBase.Enums;
using Zabbor.ZabborBase.Models;

namespace Zabbor.ZabborBase.Entities
{
    public class Character
    {
        public string Name { get; set; }
        public CharacterClass Class { get; set; }
        public CharacterStats Stats { get; set; }
        public bool IsPlayerControlled { get; set; }
        public bool IsAlive => Stats.CurrentHealth > 0;

        public Character(string name, CharacterClass characterClass, CharacterStats stats, bool isPlayerControlled)
        {
            Name = name;
            Class = characterClass;
            Stats = stats;
            Stats.CurrentHealth = Stats.MaxHealth;
            IsPlayerControlled = isPlayerControlled;
        }

        public void TakeDamage(int damageAmount)
        {
            int damageTaken = Math.Max(1, damageAmount - Stats.Armor);
            Stats.CurrentHealth -= damageTaken;
        }
    }
}