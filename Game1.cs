using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Zabbor.ZabborBase;
using Zabbor.ZabborBase.Inerfaces;
using Zabbor.ZabborBase.World;

namespace Zabbor
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Player _player;
        private IGameMap _currentBoard; 
        public const int TILE_SIZE = 32;
        private const int MAP_WIDTH = 25;
        private const int MAP_HEIGHT = 19;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = MAP_WIDTH * TILE_SIZE;
            _graphics.PreferredBackBufferHeight = MAP_HEIGHT * TILE_SIZE;
            _graphics.ApplyChanges();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Placeholder.Create(GraphicsDevice);
    
            // Ładujemy konkretną planszę - "Board1"
            _currentBoard = new Board1(MAP_WIDTH, MAP_HEIGHT);
    
            var playerPosition = new Vector2(12 * TILE_SIZE, 9 * TILE_SIZE);
            _player = new Player(playerPosition, _currentBoard);
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            _player.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin();
            _currentBoard.Draw(_spriteBatch);
            _player.Draw(_spriteBatch);
            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
