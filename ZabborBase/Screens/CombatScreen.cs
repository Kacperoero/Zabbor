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
    // Stany wewnętrzne dla ekranu walki
    public enum CombatState
    {
        PlayerActionSelect, // Gracz wybiera akcję
        PlayerTargetSelect, // Gracz wybiera cel
        EnemyTurn,          // Tura wroga
        Processing,         // Chwila przerwy na pokazanie co się stało
        Finished            // Walka zakończona
    }

    public class CombatScreen
    {
        private SpriteFont _font;
        private List<Character> _combatants;
        private int _currentTurnIndex;
        private CombatState _currentState;
        private List<Character> _playerParty;
        private List<Character> _enemies;
        private int _actionIndex = 0;
        private int _targetIndex = 0;
        private readonly string[] _actions = { "Atakuj", "Obron sie" };
        private string _combatLog = "";

        public CombatScreen(SpriteFont font)
        {
            _font = font;
        }

        public void StartCombat()
        {
            // Tworzymy tymczasowe postacie z pełnymi statystykami
            _playerParty = new List<Character>
            {
                new Character("Rycerz", CharacterClass.Knight, new CharacterStats { MaxHealth = 100, Armor = 5, Speed = 10, Attack = 15 }, true),
                new Character("Mag", CharacterClass.Mage, new CharacterStats { MaxHealth = 70, Armor = 2, Speed = 8, Attack = 20 }, true),
                new Character("Łucznik", CharacterClass.Hunter, new CharacterStats { MaxHealth = 60, Armor = 1, Speed = 12, Attack = 18 }, true)
            };
            _enemies = new List<Character>
            {
                new Character("Goblin", CharacterClass.Knight, new CharacterStats { MaxHealth = 40, Armor = 2, Speed = 9, Attack = 12 }, false),
                new Character("Szkielet", CharacterClass.Hunter, new CharacterStats { MaxHealth = 30, Armor = 0, Speed = 7, Attack = 10 }, false)
            };

            _combatants = new List<Character>();
            _combatants.AddRange(_playerParty);
            _combatants.AddRange(_enemies);
            _combatants = _combatants.OrderByDescending(c => c.Stats.Speed).ToList();
            _currentTurnIndex = 0;
            
            StartTurn();
        }

        private void StartTurn()
        {
            while (!_combatants[_currentTurnIndex].IsAlive)
            {
                _currentTurnIndex = (_currentTurnIndex + 1) % _combatants.Count;
            }

            var currentCombatant = _combatants[_currentTurnIndex];
            _combatLog = $"Tura: {currentCombatant.Name}";
            
            if (currentCombatant.IsPlayerControlled)
            {
                _currentState = CombatState.PlayerActionSelect;
                _actionIndex = 0;
            }
            else
            {
                _currentState = CombatState.EnemyTurn;
            }
        }

        private void NextTurn()
        {
            _currentTurnIndex = (_currentTurnIndex + 1) % _combatants.Count;
            StartTurn();
        }

        public GameState Update(GameTime gameTime)
        {
            if (InputManager.WasKeyPressed(Keys.Q)) return GameState.Gameplay;

            switch (_currentState)
            {
                case CombatState.PlayerActionSelect:
                    HandleActionSelection();
                    break;
                case CombatState.PlayerTargetSelect:
                    HandleTargetSelection();
                    break;
                case CombatState.EnemyTurn:
                    ExecuteEnemyTurn();
                    break;
            }

            return GameState.Combat;
        }

        private void HandleActionSelection()
        {
            if (InputManager.WasKeyPressed(Keys.Down)) _actionIndex = (_actionIndex + 1) % _actions.Length;
            if (InputManager.WasKeyPressed(Keys.Up))
            {
                _actionIndex--;
                if (_actionIndex < 0) _actionIndex = _actions.Length - 1;
            }

            if (InputManager.WasKeyPressed(Keys.Enter))
            {
                if (_actions[_actionIndex] == "Atakuj")
                {
                    _currentState = CombatState.PlayerTargetSelect;
                    _targetIndex = 0;
                }
                else // "Obron sie" na razie nic nie robi
                {
                    _combatLog = $"{_combatants[_currentTurnIndex].Name} broni sie!";
                    NextTurn();
                }
            }
        }

        private void HandleTargetSelection()
        {
            if (InputManager.WasKeyPressed(Keys.Down)) _targetIndex = (_targetIndex + 1) % _enemies.Count;
            if (InputManager.WasKeyPressed(Keys.Up))
            {
                _targetIndex--;
                if (_targetIndex < 0) _targetIndex = _enemies.Count - 1;
            }
            if (InputManager.WasKeyPressed(Keys.Escape)) _currentState = CombatState.PlayerActionSelect;

            if (InputManager.WasKeyPressed(Keys.Enter))
            {
                var attacker = _combatants[_currentTurnIndex];
                var target = _enemies[_targetIndex];
                
                target.TakeDamage(attacker.Stats.Attack);
                _combatLog = $"{attacker.Name} atakuje {target.Name}!";
                
                if (!target.IsAlive)
                {
                    _combatLog += $" {target.Name} pokonany!";
                    _combatants.Remove(target);
                    _enemies.Remove(target);
                }

                if (_enemies.Count == 0)
                {
                    _combatLog = "Zwyciestwo!";
                    _currentState = CombatState.Finished;
                }
                else
                {
                    NextTurn();
                }
            }
        }

        private void ExecuteEnemyTurn()
        {
            var attacker = _combatants[_currentTurnIndex];
            var target = _playerParty.Where(p => p.IsAlive).OrderBy(p => p.Stats.CurrentHealth).FirstOrDefault();

            if (target != null)
            {
                target.TakeDamage(attacker.Stats.Attack);
                _combatLog = $"{attacker.Name} atakuje {target.Name}!";

                if (!target.IsAlive) _combatLog += $" {target.Name} pokonany!";
                
                if (!_playerParty.Any(p => p.IsAlive))
                {
                    _combatLog = "Porażka...";
                    _currentState = CombatState.Finished;
                }
                else
                {
                    NextTurn();
                }
            }
        }
        
        public void Draw(SpriteBatch spriteBatch)
        {
            for(int i = 0; i < _playerParty.Count; i++)
            {
                var member = _playerParty[i];
                var text = $"{member.Name}\nHP: {member.Stats.CurrentHealth}/{member.Stats.MaxHealth}";
                spriteBatch.DrawString(_font, text, new Vector2(100, 100 + i * 80), member.IsAlive ? Color.White : Color.Gray);
            }

            for(int i = 0; i < _enemies.Count; i++)
            {
                var enemy = _enemies[i];
                var text = $"{enemy.Name}\nHP: {enemy.Stats.CurrentHealth}/{enemy.Stats.MaxHealth}";
                var color = (_currentState == CombatState.PlayerTargetSelect && i == _targetIndex) ? Color.Yellow : Color.White;
                spriteBatch.DrawString(_font, text, new Vector2(800, 100 + i * 80), color);
            }
            
            if(_currentState == CombatState.PlayerActionSelect)
            {
                for(int i = 0; i < _actions.Length; i++)
                {
                    var color = (i == _actionIndex) ? Color.Yellow : Color.White;
                    spriteBatch.DrawString(_font, _actions[i], new Vector2(100, 500 + i * 40), color);
                }
            }

            // Log walki
            var logSize = _font.MeasureString(_combatLog);
            spriteBatch.DrawString(_font, _combatLog, new Vector2((1280 - logSize.X) / 2, 600), Color.White);
        }
    }
}