using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Zabbor.Core;
using Zabbor.Managers;
using Zabbor.ZabborBase;
using Zabbor.ZabborBase.Interfaces;
using Zabbor.ZabborBase.UI;
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
        private const int MAP_WIDTH = 50;
        private const int MAP_HEIGHT = 40;
        private SpriteFont _dialogFont;
        private DialogBox _activeDialog;
        private Camera _camera;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.ApplyChanges();
            base.Initialize();
        }
        
        public void ShowDialog(string text)
        {
            _activeDialog = new DialogBox(text, _dialogFont, GraphicsDevice);
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Placeholder.Create(GraphicsDevice);
            _dialogFont = Content.Load<SpriteFont>("dialog_font");

            DialogueManager.LoadDialogues();
            _currentBoard = new Board1(MAP_WIDTH, MAP_HEIGHT);

            _camera = new Camera(GraphicsDevice.Viewport);

            var playerPosition = new Vector2(12 * TILE_SIZE, 9 * TILE_SIZE);
            _player = new Player(playerPosition, _currentBoard);
        }

        protected override void Update(GameTime gameTime)
        {
            if (_activeDialog != null)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Q))
                {
                    _activeDialog = null;
                }
                return;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            string dialogToShow = _player.Update(gameTime);
            if (dialogToShow != null)
            {
                ShowDialog(dialogToShow);
            }

            var mapSizeInPixels = new Point(MAP_WIDTH * TILE_SIZE, MAP_HEIGHT * TILE_SIZE);
            _camera.Follow(_player.Position, mapSizeInPixels);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin(transformMatrix: _camera.Transform);
            _currentBoard.Draw(_spriteBatch);
            _player.Draw(_spriteBatch);
            _spriteBatch.End();
            _spriteBatch.Begin();
            if (_activeDialog != null)
            {
                _activeDialog.Draw(_spriteBatch);
            }
            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
