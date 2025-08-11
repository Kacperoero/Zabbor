// Screens/CombatScreen.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Zabbor.ZabborBase.Enums;
using Zabbor.Core;
using Zabbor.ZabborBase.Entities;
using Zabbor.ZabborBase.Models;
using Microsoft.Xna.Framework.Input;

namespace Zabbor.Screens
{
    public class CombatScreen
    {
        private SpriteFont _font;
        
        // Lista wszystkich uczestników walki
        private List<Character> _combatants;
        // Indeks postaci, której jest aktualnie tura
        private int _currentTurnIndex;

        public CombatScreen(SpriteFont font)
        {
            _font = font;
            _combatants = new List<Character>();
        }

        public void StartCombat()
        {
            // --- TYMCZASOWE TWORZENIE POSTACI NA POTRZEBY TESTÓW ---
            var playerParty = new List<Character>
            {
                new Character("Rycerz", CharacterClass.Knight, new CharacterStats { Speed = 10 }, true),
                new Character("Mag", CharacterClass.Mage, new CharacterStats { Speed = 8 }, true),
                new Character("Łucznik", CharacterClass.Hunter, new CharacterStats { Speed = 12 }, true)
            };
            var enemies = new List<Character>
            {
                new Character("Goblin", CharacterClass.Knight, new CharacterStats { Speed = 9 }, false),
                new Character("Szkielet", CharacterClass.Hunter, new CharacterStats { Speed = 7 }, false)
            };
            // ----------------------------------------------------

            _combatants.Clear();
            _combatants.AddRange(playerParty);
            _combatants.AddRange(enemies);

            _combatants = _combatants.OrderByDescending(c => c.Stats.Speed).ToList();

            _currentTurnIndex = 0;
        }

        private void NextTurn()
        {
            _currentTurnIndex = (_currentTurnIndex + 1) % _combatants.Count;
        }

        public GameState Update(GameTime gameTime)
        {
            if (InputManager.WasKeyPressed(Keys.Q)) return GameState.Gameplay;

            if (_combatants.Count == 0) return GameState.Gameplay;

            var currentCombatant = _combatants[_currentTurnIndex];

            if (currentCombatant.IsPlayerControlled)
            {
                // Jeśli to tura gracza, czekamy na akcję (na razie tylko spacja)
                if (InputManager.WasKeyPressed(Keys.Space))
                {
                    // Tutaj w przyszłości będzie menu akcji (Atak, Czar, Obrona)
                    // Na razie po prostu przechodzimy do następnej tury
                    NextTurn();
                }
            }
            else
            {
                // Jeśli to tura wroga, wykonuje on swoją akcję automatycznie (na razie nic nie robi)
                // i od razu przechodzimy do następnej tury.
                // W przyszłości można tu dodać opóźnienie, aby akcja wroga była widoczna.
                NextTurn();
            }

            return GameState.Combat;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Rysowanie kolejki tury po prawej stronie
            var position = new Vector2(1000, 50);
            spriteBatch.DrawString(_font, "KOLEJKA:", position, Color.White);
            position.Y += 40;

            for (int i = 0; i < _combatants.Count; i++)
            {
                var combatant = _combatants[i];
                var color = combatant.IsPlayerControlled ? Color.Cyan : Color.Red;
                var text = $"{i + 1}. {combatant.Name} (Spd: {combatant.Stats.Speed})";
                
                if (i == _currentTurnIndex)
                {
                    spriteBatch.DrawString(_font, "> " + text, position, Color.Yellow);
                }
                else
                {
                    spriteBatch.DrawString(_font, "  " + text, position, color);
                }
                position.Y += 30;
            }

            // Informacja o akcji
            if (_combatants.Count > 0 && _combatants[_currentTurnIndex].IsPlayerControlled)
            {
                var activePlayer = _combatants[_currentTurnIndex];
                var actionText = $"Tura gracza: {activePlayer.Name}. Wcisnij Spacje, aby kontynuowac.";
                var textSize = _font.MeasureString(actionText);
                var textPosition = new Vector2( (1280 - textSize.X) / 2, 650);
                spriteBatch.DrawString(_font, actionText, textPosition, Color.White);
            }
        }
    }
}