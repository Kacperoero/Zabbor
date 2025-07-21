using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using Zabbor.ZabborBase.Inerfaces;

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

        public Player(Vector2 position, IGameMap map)
        {
            Position = position;
            _targetPosition = position;
            _graphics = new Placeholder(position, new Point(Game1.TILE_SIZE), Color.White);
            _map = map; 
        }

        public void Update(GameTime gameTime)
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
                }
            }
            else
            {
                KeyboardState kState = Keyboard.GetState();
                Vector2 moveDirection = Vector2.Zero;

                if (kState.IsKeyDown(Keys.W) || kState.IsKeyDown(Keys.Up)) moveDirection.Y = -1;
                else if (kState.IsKeyDown(Keys.S) || kState.IsKeyDown(Keys.Down)) moveDirection.Y = 1;
                else if (kState.IsKeyDown(Keys.A) || kState.IsKeyDown(Keys.Left)) moveDirection.X = -1;
                else if (kState.IsKeyDown(Keys.D) || kState.IsKeyDown(Keys.Right)) moveDirection.X = 1;

                if (moveDirection != Vector2.Zero)
                {
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
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _graphics.Draw(spriteBatch);
        }
    }
}