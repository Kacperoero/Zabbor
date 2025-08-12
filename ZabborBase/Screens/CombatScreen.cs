// Screens/CombatScreen.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Zabbor.ZabborBase.Enums;
using Zabbor.Core;
using Zabbor.ZabborBase.Entities;
using Zabbor.ZabborBase.Models;
using Zabbor.ZabborBase.Combat;
using System;
using Zabbor.ZabborBase;
using Microsoft.Xna.Framework.Input;

namespace Zabbor.Screens
{
    public enum CombatState { ProcessingTurn, PlayerActionSelect, PlayerMoveSelect, PlayerTargetTileSelect, ExecutingMove, EnemyTurn, Finished }

    public class CombatScreen
    {
        public const int TILE_SIZE = 32;
        private SpriteFont _font;
        private List<Character> _combatants;
        private Character _activeCombatant;
        private CombatState _currentState;
        private List<Character> _playerParty;
        private List<Character> _enemies;
        private int _actionIndex = 0;
        private readonly string[] _actions = { "Ruch", "Atakuj", "Obron sie" };
        private string _combatLog = "";
        private CombatGrid _combatGrid;
        private List<Point> _reachableTiles = new List<Point>();
        private List<Point> _attackableTiles = new List<Point>();
        private Point _cursorPosition;
        private float _enemyTurnTimer = 0f;
        private Queue<Point> _movePath;
        private float _moveTimer = 0f;
        private const float TIME_PER_TILE = 0.2f;

        public CombatScreen(SpriteFont font)
        {
            _font = font;
        }

        public void StartCombat()
        {
            _combatGrid = new CombatGrid(16, 9);
            _playerParty = new List<Character>
            {
                new Character("Rycerz", CharacterClass.Knight, new CharacterStats { MaxHealth = 100, Armor = 5, Speed = 10, Attack = 15, MovementPoints = 3, AttackRange = 1 }, true),
                new Character("Mag", CharacterClass.Mage, new CharacterStats { MaxHealth = 70, Armor = 2, Speed = 8, Attack = 20, MovementPoints = 3, AttackRange = 4 }, true),
                new Character("Łucznik", CharacterClass.Hunter, new CharacterStats { MaxHealth = 60, Armor = 1, Speed = 12, Attack = 18, MovementPoints = 4, AttackRange = 5 }, true)
            };
            _enemies = new List<Character>
            {
                new Character("Goblin", CharacterClass.Knight, new CharacterStats { MaxHealth = 40, Armor = 2, Speed = 9, Attack = 12, MovementPoints = 4, AttackRange = 1 }, false),
                new Character("Szkielet", CharacterClass.Hunter, new CharacterStats { MaxHealth = 30, Armor = 0, Speed = 7, Attack = 10, MovementPoints = 3, AttackRange = 4 }, false)
            };
            _playerParty[0].CombatPosition = new Point(1, 2);
            _playerParty[1].CombatPosition = new Point(1, 4);
            _playerParty[2].CombatPosition = new Point(1, 6);
            var random = new Random();
            _enemies[0].CombatPosition = new Point(random.Next(12, 15), random.Next(1, 4));
            _enemies[1].CombatPosition = new Point(random.Next(12, 15), random.Next(5, 8));
            _combatants = new List<Character>();
            _combatants.AddRange(_playerParty);
            _combatants.AddRange(_enemies);
            foreach (var character in _combatants)
            {
                character.Initiative = 0;
                _combatGrid.GetTile(character.CombatPosition.X, character.CombatPosition.Y)?.SetOccupant(character);
            }
            NextTurn();
        }

        private void NextTurn()
        {
            _currentState = CombatState.ProcessingTurn; // Zawsze przechodź przez stan przetwarzania
        }

        private void ProcessTurn()
        {
            while (true)
            {
                foreach (var c in _combatants.Where(ch => ch.IsAlive))
                {
                    c.Initiative += c.Stats.Speed;
                }
                var readyCombatants = _combatants.Where(c => c.IsAlive && c.Initiative >= 100).ToList();
                if (readyCombatants.Any())
                {
                    _activeCombatant = readyCombatants.OrderByDescending(c => c.Initiative).First();
                    _activeCombatant.Initiative -= 100;
                    _combatLog = $"Tura: {_activeCombatant.Name}";
                    if (_activeCombatant.IsPlayerControlled)
                    {
                        _currentState = CombatState.PlayerActionSelect;
                        _actionIndex = 0;
                    }
                    else
                    {
                        _currentState = CombatState.EnemyTurn;
                    }
                    return;
                }
            }
        }

        public GameState Update(GameTime gameTime)
        {
            if (InputManager.WasKeyPressed(Keys.Q) || _currentState == CombatState.Finished)
                return GameState.Gameplay;

            switch (_currentState)
            {
                case CombatState.ProcessingTurn: 
                    ProcessTurn(); 
                    break;
                case CombatState.PlayerActionSelect: HandleActionSelection(); break;
                case CombatState.PlayerMoveSelect: HandleMoveSelection(); break;
                case CombatState.PlayerTargetTileSelect: HandleTargetTileSelection(); break;
                case CombatState.ExecutingMove: HandleMoveExecution(gameTime); break;
                case CombatState.EnemyTurn: ExecuteEnemyTurn(gameTime); break;
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
                var activeCharacter = _activeCombatant;
                switch (_actions[_actionIndex])
                {
                    case "Ruch":
                        _reachableTiles = GetReachableTiles(activeCharacter);
                        if (_reachableTiles.Any())
                        {
                            _cursorPosition = activeCharacter.CombatPosition;
                            _currentState = CombatState.PlayerMoveSelect;
                        }
                        else
                        {
                            _combatLog = $"{activeCharacter.Name} nie ma gdzie sie ruszyc!";
                        }
                        break;
                    case "Atakuj":
                        _attackableTiles = GetAttackableTiles(activeCharacter);
                        _cursorPosition = activeCharacter.CombatPosition;
                        _currentState = CombatState.PlayerTargetTileSelect;
                        break;
                    case "Obron sie":
                        _combatLog = $"{activeCharacter.Name} broni sie!";
                        NextTurn();
                        break;
                }
            }
        }

        private void HandleMoveSelection()
        {
            if (InputManager.WasKeyPressed(Keys.Down)) _cursorPosition.Y++;
            if (InputManager.WasKeyPressed(Keys.Up)) _cursorPosition.Y--;
            if (InputManager.WasKeyPressed(Keys.Left)) _cursorPosition.X--;
            if (InputManager.WasKeyPressed(Keys.Right)) _cursorPosition.X++;
            _cursorPosition.X = Math.Clamp(_cursorPosition.X, 0, _combatGrid.Width - 1);
            _cursorPosition.Y = Math.Clamp(_cursorPosition.Y, 0, _combatGrid.Height - 1);

            if (InputManager.WasKeyPressed(Keys.Escape))
            {
                _reachableTiles.Clear();
                _currentState = CombatState.PlayerActionSelect;
            }
            if (InputManager.WasKeyPressed(Keys.Enter))
            {
                if (_reachableTiles.Contains(_cursorPosition))
                {
                    var path = FindPath(_activeCombatant.CombatPosition, _cursorPosition);
                    if (path != null)
                    {
                        _movePath = new Queue<Point>(path.Skip(1));
                        _currentState = CombatState.ExecutingMove;
                        _reachableTiles.Clear();
                    }
                }
            }
        }

        private void HandleTargetTileSelection()
        {
            if (InputManager.WasKeyPressed(Keys.Down)) _cursorPosition.Y++;
            if (InputManager.WasKeyPressed(Keys.Up)) _cursorPosition.Y--;
            if (InputManager.WasKeyPressed(Keys.Left)) _cursorPosition.X--;
            if (InputManager.WasKeyPressed(Keys.Right)) _cursorPosition.X++;
            _cursorPosition.X = Math.Clamp(_cursorPosition.X, 0, _combatGrid.Width - 1);
            _cursorPosition.Y = Math.Clamp(_cursorPosition.Y, 0, _combatGrid.Height - 1);

            if (InputManager.WasKeyPressed(Keys.Escape))
            {
                _attackableTiles.Clear();
                _currentState = CombatState.PlayerActionSelect;
            }

            if (InputManager.WasKeyPressed(Keys.Enter))
            {
                if (_attackableTiles.Contains(_cursorPosition))
                {
                    var attacker = _activeCombatant;
                    var target = _combatGrid.GetTile(_cursorPosition.X, _cursorPosition.Y).Occupant;

                    if (target != null && !target.IsPlayerControlled) // Można atakować tylko wrogów
                    {
                        int damageDealt = Math.Max(1, attacker.Stats.Attack - target.Stats.Armor);
                        target.TakeDamage(attacker.Stats.Attack);
                        _combatLog = $"{attacker.Name} atakuje {target.Name} i zadaje {damageDealt} obrazen!";

                        if (!target.IsAlive)
                        {
                            _combatLog += $" {target.Name} pokonany!";
                            _combatants.Remove(target);
                            _enemies.Remove(target);
                            _combatGrid.GetTile(target.CombatPosition.X, target.CombatPosition.Y).ClearOccupant();
                        }

                        if (!_enemies.Any(e => e.IsAlive))
                        {
                            _combatLog = "Zwyciestwo!";
                            _currentState = CombatState.Finished;
                        }
                        else
                        {
                            _attackableTiles.Clear();
                            NextTurn();
                        }
                    }
                    else
                    {
                        _combatLog = "Na tym polu nie ma celu!";
                    }
                }
                else
                {
                    _combatLog = "Cel poza zasiegiem!";
                }
            }
        }

        private void ExecuteEnemyTurn(GameTime gameTime)
        {
            _enemyTurnTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_enemyTurnTimer < 1f)
            {
                return;
            }
            _enemyTurnTimer = 0f;

            var attacker = _activeCombatant;
            var targetsInRange = GetTargetsInRange(attacker);
            var target = targetsInRange.OrderBy(p => p.Stats.CurrentHealth).FirstOrDefault();

            if (target != null) // Jeśli jest cel w zasięgu -> Atakuj
            {
                int damageDealt = Math.Max(1, attacker.Stats.Attack - target.Stats.Armor);
                target.TakeDamage(attacker.Stats.Attack);
                _combatLog = $"{attacker.Name} atakuje {target.Name} i zadaje {damageDealt} obrazen!";
                if (!target.IsAlive) _combatLog += $" {target.Name} pokonany!";
                
                if (!_playerParty.Any(p => p.IsAlive))
                {
                    _combatLog = "Porazka...";
                    _currentState = CombatState.Finished;
                }
                else
                {
                    NextTurn();
                }
            }
            else // Jeśli nie ma celu w zasięgu -> Spróbuj się ruszyć
            {
                var closestPlayer = _playerParty
                    .Where(p => p.IsAlive)
                    .OrderBy(p => Math.Abs(p.CombatPosition.X - attacker.CombatPosition.X) + Math.Abs(p.CombatPosition.Y - attacker.CombatPosition.Y))
                    .FirstOrDefault();

                if (closestPlayer != null)
                {
                    // ---- POPRAWIONA LOGIKA ----
                    // 1. Znajdź najbliższe wolne pole OBOK celu
                    Point destination = GetClosestAdjacentTile(attacker, closestPlayer);

                    // 2. Wyznacz ścieżkę do tego wolnego pola
                    var path = FindPath(attacker.CombatPosition, destination);

                    if (path != null && path.Count > 1)
                    {
                        int moveDistance = Math.Min(path.Count - 1, attacker.Stats.MovementPoints);
                        var actualPath = path.Take(moveDistance + 1);
                        _movePath = new Queue<Point>(actualPath.Skip(1));
                        _currentState = CombatState.ExecutingMove;
                        _combatLog = $"{attacker.Name} przemieszcza sie w strone {closestPlayer.Name}.";
                        return; 
                    }
                }
                // Jeśli nie da się ruszyć lub nie ma celu, po prostu zakończ turę
                NextTurn();
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // ---- 1. RYSOWANIE SIATKI I KURSORA ----
            for (int y = 0; y < _combatGrid.Height; y++)
            {
                for (int x = 0; x < _combatGrid.Width; x++)
                {
                    var tileScreenPos = new Rectangle(x * TILE_SIZE, y * TILE_SIZE, TILE_SIZE, TILE_SIZE);
                    var tileColor = new Color(40, 40, 40);

                    if (_currentState == CombatState.PlayerMoveSelect && _reachableTiles.Contains(new Point(x, y)))
                        tileColor = Color.DarkBlue;
                    else if (_currentState == CombatState.PlayerTargetTileSelect && _attackableTiles.Contains(new Point(x, y)))
                        tileColor = Color.DarkRed;

                    spriteBatch.Draw(Placeholder.Texture, new Rectangle(tileScreenPos.X, tileScreenPos.Y, TILE_SIZE + 1, TILE_SIZE + 1), new Color(50, 50, 50));
                    spriteBatch.Draw(Placeholder.Texture, tileScreenPos, tileColor);

                    if ((_currentState == CombatState.PlayerMoveSelect || _currentState == CombatState.PlayerTargetTileSelect) && _cursorPosition.X == x && _cursorPosition.Y == y)
                        spriteBatch.Draw(Placeholder.Texture, tileScreenPos, Color.White * 0.3f);
                }
            }

            // ---- 2. RYSOWANIE POSTACI I PASKÓW HP ----
            foreach (var character in _playerParty.Concat(_enemies))
            {
                if (!character.IsAlive) continue;
                var charColor = character.IsPlayerControlled ? Color.CornflowerBlue : Color.IndianRed;

                // POPRAWKA: Używamy _activeCombatant z pola klasy
                if (character == _activeCombatant)
                {
                    charColor = Color.Yellow;
                }

                var charScreenPos = new Rectangle(character.CombatPosition.X * TILE_SIZE, character.CombatPosition.Y * TILE_SIZE, TILE_SIZE, TILE_SIZE);
                spriteBatch.Draw(Placeholder.Texture, charScreenPos, charColor);

                if (character.Stats.MaxHealth > 0)
                {
                    var hpBarBackground = new Rectangle(charScreenPos.X, charScreenPos.Y - 10, TILE_SIZE, 5);
                    float healthPercentage = (float)character.Stats.CurrentHealth / character.Stats.MaxHealth;
                    var hpBarForeground = new Rectangle(charScreenPos.X, charScreenPos.Y - 10, (int)(TILE_SIZE * healthPercentage), 5);
                    spriteBatch.Draw(Placeholder.Texture, hpBarBackground, Color.DarkRed);
                    spriteBatch.Draw(Placeholder.Texture, hpBarForeground, Color.LimeGreen);
                }
            }

            // ---- 3. RYSOWANIE INTERFEJSU (LOG, MENU, KOLEJKA) ----

            // Log walki na dole ekranu
            var logText = _combatLog;
            var logTextSize = _font.MeasureString(logText);
            var logTextPosition = new Vector2((_combatGrid.Width * TILE_SIZE - logTextSize.X) / 2, 650);
            spriteBatch.Draw(Placeholder.Texture, new Rectangle((int)logTextPosition.X - 10, (int)logTextPosition.Y - 5, (int)logTextSize.X + 20, (int)logTextSize.Y + 10), Color.Black * 0.7f);
            spriteBatch.DrawString(_font, logText, logTextPosition, Color.White);

            // Menu i kolejka po prawej stronie
            var uiPosition = new Vector2(_combatGrid.Width * TILE_SIZE + 50, 50);

            // Rysowanie menu akcji
            if (_activeCombatant != null && _activeCombatant.IsPlayerControlled && _currentState == CombatState.PlayerActionSelect)
            {
                spriteBatch.DrawString(_font, "AKCJE:", uiPosition, Color.White);
                uiPosition.Y += 30;
                for (int i = 0; i < _actions.Length; i++)
                {
                    var color = (i == _actionIndex) ? Color.Yellow : Color.White;
                    spriteBatch.DrawString(_font, _actions[i], uiPosition, color);
                    uiPosition.Y += 25;
                }
            }

            uiPosition.Y += 30;

            // Rysowanie kolejki tury
            spriteBatch.DrawString(_font, "KOLEJKA:", uiPosition, Color.White);
            uiPosition.Y += 30;

            // Tworzymy tymczasową kolejkę do wyświetlenia
            var turnQueue = _combatants.Where(c => c.IsAlive).OrderByDescending(c => c.Initiative).ToList();

            foreach (var combatant in turnQueue)
            {
                var color = combatant.IsPlayerControlled ? Color.Cyan : Color.Red;
                var text = $"{combatant.Name} (HP: {combatant.Stats.CurrentHealth})";

                // POPRAWKA: Używamy _activeCombatant z pola klasy do podświetlenia
                if (combatant == _activeCombatant)
                    spriteBatch.DrawString(_font, "> " + text, uiPosition, Color.Yellow);
                else
                    spriteBatch.DrawString(_font, "  " + text, uiPosition, color);

                uiPosition.Y += 25;
            }
        }

        private List<Point> GetReachableTiles(Character character)
        {
            var reachableTiles = new List<Point>();
            var visited = new HashSet<Point> { character.CombatPosition };
            var queue = new Queue<(Point position, int remainingMove)>();
            queue.Enqueue((character.CombatPosition, character.Stats.MovementPoints));
            while (queue.Count > 0)
            {
                var (currentPos, remainingMove) = queue.Dequeue();
                if (currentPos != character.CombatPosition) reachableTiles.Add(currentPos);
                if (remainingMove > 0)
                {
                    Point[] neighbors = { new Point(currentPos.X, currentPos.Y - 1), new Point(currentPos.X, currentPos.Y + 1), new Point(currentPos.X - 1, currentPos.Y), new Point(currentPos.X + 1, currentPos.Y) };
                    foreach (var neighborPos in neighbors)
                    {
                        if (!visited.Contains(neighborPos))
                        {
                            visited.Add(neighborPos);
                            var tile = _combatGrid.GetTile(neighborPos.X, neighborPos.Y);
                            if (tile != null && tile.IsWalkable)
                            {
                                queue.Enqueue((neighborPos, remainingMove - 1));
                            }
                        }
                    }
                }
            }
            return reachableTiles;
        }

        private List<Character> GetTargetsInRange(Character attacker)
        {
            var targets = new List<Character>();
            var potentialTargets = attacker.IsPlayerControlled ? _enemies : _playerParty;

            foreach (var target in potentialTargets.Where(t => t.IsAlive))
            {
                int distance = Math.Abs(attacker.CombatPosition.X - target.CombatPosition.X) +
                               Math.Abs(attacker.CombatPosition.Y - target.CombatPosition.Y);

                if (distance <= attacker.Stats.AttackRange)
                {
                    targets.Add(target);
                }
            }
            return targets;
        }

        private List<Point> GetAttackableTiles(Character character)
        {
            var attackableTiles = new HashSet<Point>();
            for (int x = 0; x < _combatGrid.Width; x++)
            {
                for (int y = 0; y < _combatGrid.Height; y++)
                {
                    int distance = Math.Abs(character.CombatPosition.X - x) + Math.Abs(character.CombatPosition.Y - y);
                    if (distance > 0 && distance <= character.Stats.AttackRange)
                    {
                        attackableTiles.Add(new Point(x, y));
                    }
                }
            }
            return attackableTiles.ToList();
        }

        private List<Point> FindPath(Point start, Point end)
        {
            var queue = new Queue<List<Point>>();
            var visited = new HashSet<Point> { start };
            queue.Enqueue(new List<Point> { start });

            while (queue.Count > 0)
            {
                var path = queue.Dequeue();
                var current = path.Last();

                if (current == end) return path;

                Point[] neighbors = {
                    new Point(current.X, current.Y - 1), new Point(current.X, current.Y + 1),
                    new Point(current.X - 1, current.Y), new Point(current.X + 1, current.Y)
                };

                foreach (var neighbor in neighbors)
                {
                    if (!visited.Contains(neighbor))
                    {
                        var tile = _combatGrid.GetTile(neighbor.X, neighbor.Y);

                        // POPRAWKA: Sprawdzamy, czy pole jest puste LUB czy jest to nasz ostateczny cel
                        if (tile != null && (tile.Occupant == null || neighbor == end))
                        {
                            visited.Add(neighbor);
                            var newPath = new List<Point>(path) { neighbor };
                            queue.Enqueue(newPath);
                        }
                    }
                }
            }
            return null; // Nie znaleziono ścieżki
        }
        
        private Point GetClosestAdjacentTile(Character attacker, Character target)
        {
            Point bestTile = attacker.CombatPosition;
            int closestDist = int.MaxValue;

            Point[] neighbors = {
                new Point(target.CombatPosition.X, target.CombatPosition.Y - 1),
                new Point(target.CombatPosition.X, target.CombatPosition.Y + 1),
                new Point(target.CombatPosition.X - 1, target.CombatPosition.Y),
                new Point(target.CombatPosition.X + 1, target.CombatPosition.Y)
            };

            foreach (var neighbor in neighbors)
            {
                var tile = _combatGrid.GetTile(neighbor.X, neighbor.Y);
                if (tile != null && tile.IsWalkable)
                {
                    int dist = Math.Abs(attacker.CombatPosition.X - neighbor.X) + Math.Abs(attacker.CombatPosition.Y - neighbor.Y);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        bestTile = neighbor;
                    }
                }
            }
            return bestTile;
        }

        private void HandleMoveExecution(GameTime gameTime)
        {
            _moveTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_moveTimer < TIME_PER_TILE)
            {
                return;
            }
            _moveTimer = 0f;

            if (_movePath != null && _movePath.Count > 0)
            {
                var nextTile = _movePath.Dequeue();
                var character = _activeCombatant;

                _combatGrid.GetTile(character.CombatPosition.X, character.CombatPosition.Y).ClearOccupant();
                _combatGrid.GetTile(nextTile.X, nextTile.Y).SetOccupant(character);
                character.CombatPosition = nextTile;
            }

            if (_movePath == null || _movePath.Count == 0)
            {
                NextTurn();
            }
        }
    }
}