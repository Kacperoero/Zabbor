using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using Zabbor.ZabborBase.Interfaces;
using Zabbor.ZabborBase.Managers;
using Zabbor.ZabborBase.Systems;
using Zabbor.Managers;
using Zabbor.Core; // Dodany dla WorldItem

namespace Zabbor.ZabborBase
{
    public class Player
    {
        public Vector2 Position { get; private set; }
        public Inventory Inventory { get; private set; }
        private Placeholder _graphics;
        private bool _isMoving = false;
        private Vector2 _targetPosition;
        private float _timeToMove = 0.15f;
        private float _moveTimer = 0f;
        private IGameMap _map;
        private Vector2 _facingDirection = new Vector2(0, 1);


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
            var kState = Keyboard.GetState();

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

                    var currentTile = new Point((int)(Position.X / Game1.TILE_SIZE), (int)(Position.Y / Game1.TILE_SIZE));
                    var warp = _map.GetWarpAt(currentTile);
                    if (warp != null)
                    {
                        return warp;
                    }
                }
            }
            else
            {
                if (InputManager.WasKeyPressed(Keys.Space))
                {
                    var interactionResult = Interact();
                    if (interactionResult != null)
                    {
                        return interactionResult;
                    }
                }

                Vector2 moveDirection = Vector2.Zero;
                if (InputManager.IsKeyDown(Keys.W) || InputManager.IsKeyDown(Keys.Up)) moveDirection.Y = -1;
                else if (InputManager.IsKeyDown(Keys.S) || InputManager.IsKeyDown(Keys.Down)) moveDirection.Y = 1;
                else if (InputManager.IsKeyDown(Keys.A) || InputManager.IsKeyDown(Keys.Left)) moveDirection.X = -1;
                else if (InputManager.IsKeyDown(Keys.D) || InputManager.IsKeyDown(Keys.Right)) moveDirection.X = 1;

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
            
            var item = _map.GetWorldItemAt(currentTile);
            if (item != null)
            {
                return item; 
            }

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