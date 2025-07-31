using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using Zabbor.ZabborBase.Interfaces;
using Zabbor.Managers;
using Zabbor.ZabborBase.Systems;
using Zabbor.ZabborBase.Managers;

namespace Zabbor.ZabborBase
{
    public class Player
    {
        public Vector2 Position { get; private set; }

        // Gracz ma teraz w sobie obiekt, który go reprezentuje graficznie
        private Placeholder _graphics;
        // Zmienne do obsługi ruchu po siatce
        private bool _isMoving = false;
        private Vector2 _targetPosition; // Pozycja docelowa na siatce
        private float _timeToMove = 0.15f; // Czas na przejście jednego kafelka (w sekundach)
        private float _moveTimer = 0f;
        private IGameMap _map;
        private Vector2 _facingDirection = new Vector2(0, 1);
        public Inventory Inventory { get; private set; }

        public Player(Vector2 position, IGameMap map)
        {
            Position = position;
            _targetPosition = position;
            _graphics = new Placeholder(position, new Point(Game1.TILE_SIZE), Color.White);
            _map = map; 
            Inventory = new Inventory();
        }

        public void SetPosition(Vector2 newPosition, IGameMap newMap)
        {
            Position = newPosition;
            _targetPosition = newPosition;
            _graphics.Position = newPosition;
            _map = newMap;
            _isMoving = false;
        }

        public object Update(GameTime gameTime)
        {
            if (_isMoving)
            {
                _moveTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                float progress = Math.Min(_moveTimer / _timeToMove, 1.0f);
                Position = Vector2.Lerp(Position, _targetPosition, progress);
                
                if (progress >= 1.0f)
                {
                    _isMoving = false;
                    _moveTimer = 0f;
                    Position = _targetPosition;

                    // Sprawdzamy tylko portale po zakończeniu ruchu
                    var currentTile = new Point((int)(Position.X / Game1.TILE_SIZE), (int)(Position.Y / Game1.TILE_SIZE));
                    var warp = _map.GetWarpAt(currentTile);
                    if (warp != null) return warp;
                }
            }
            else
            {
                var kState = Keyboard.GetState();
                
                // Klawisz Spacji obsługuje teraz obie interakcje
                if (kState.IsKeyDown(Keys.Space))
                {
                    return Interact();
                }

                // Logika ruchu pozostaje bez zmian
                Vector2 moveDirection = Vector2.Zero;
                if (kState.IsKeyDown(Keys.W) || kState.IsKeyDown(Keys.Up)) moveDirection.Y = -1;
                else if (kState.IsKeyDown(Keys.S) || kState.IsKeyDown(Keys.Down)) moveDirection.Y = 1;
                else if (kState.IsKeyDown(Keys.A) || kState.IsKeyDown(Keys.Left)) moveDirection.X = -1;
                else if (kState.IsKeyDown(Keys.D) || kState.IsKeyDown(Keys.Right)) moveDirection.X = 1;

                if (moveDirection != Vector2.Zero)
                {
                    _facingDirection = moveDirection;
                    Point currentTile = new Point((int)(Position.X / Game1.TILE_SIZE), (int)(Position.Y / Game1.TILE_SIZE));
                    Point targetTile = new Point(currentTile.X + (int)moveDirection.X, currentTile.Y + (int)moveDirection.Y);

                    if (_map.IsTileWalkable(targetTile))
                    {
                        _targetPosition = new Vector2(targetTile.X * Game1.TILE_SIZE, targetTile.Y * Game1.TILE_SIZE);
                        _isMoving = true;
                    }
                }
            }
            
            _graphics.Position = Position;
            return null;
        }
        
        private object Interact()
        {
            var currentTile = new Point((int)(Position.X / Game1.TILE_SIZE), (int)(Position.Y / Game1.TILE_SIZE));
            
            // PRIORYTET 1: Sprawdź, czy na polu, na którym STOIMY, jest przedmiot
            var item = _map.GetWorldItemAt(currentTile);
            if (item != null)
            {
                Inventory.AddItem(item.ItemId);
                _map.RemoveWorldItemAt(currentTile);
                return $"{ItemManager.GetItem(item.ItemId).Name} został podniesiony.";
            }

            // PRIORYTET 2: Jeśli nie, sprawdź, czy PRZED nami jest NPC
            var targetTile = new Point(currentTile.X + (int)_facingDirection.X, currentTile.Y + (int)_facingDirection.Y);
            var npc = _map.GetNpcAt(targetTile);
            if (npc != null)
            {
                return DialogueManager.GetDialogue(npc.DialogueId);
            }
            
            return null;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _graphics.Draw(spriteBatch);
        }
    }
}