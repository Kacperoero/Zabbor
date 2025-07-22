using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Zabbor.Core;
using Zabbor.Managers;
using Zabbor.ZabborBase;
using Zabbor.ZabborBase.Interfaces;
using Zabbor.ZabborBase.UI;
using Zabbor.ZabborBase.World;
using Zabbor.ZabborBase.Enums;

namespace Zabbor
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // ---- Pola dla stanów gry ----
        private GameState _currentState;
        private MainMenuScreen _mainMenuScreen;
        
        // ---- Pola dla stanu Gameplay ----
        private Player _player;
        private IGameMap _currentBoard; 
        private Camera _camera;
        private DialogBox _activeDialog;
        
        // ---- Wspólne zasoby i stałe ----
        public const int TILE_SIZE = 32;
        private const int MAP_WIDTH = 50;
        private const int MAP_HEIGHT = 40;
        private SpriteFont _dialogFont;

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
            
            _currentState = GameState.MainMenu; // Zaczynamy od menu głównego

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            
            // Ładujemy tylko zasoby potrzebne od razu (dla menu)
            _dialogFont = Content.Load<SpriteFont>("dialog_font");
            _mainMenuScreen = new MainMenuScreen(_dialogFont, GraphicsDevice.Viewport);
        }

        // Nowa metoda, która inicjalizuje wszystko potrzebne do rozgrywki
        private void StartGameplay()
        {
            // Ładujemy zasoby potrzebne tylko do gry
            Placeholder.Create(GraphicsDevice);
            DialogueManager.LoadDialogues();

            _camera = new Camera(GraphicsDevice.Viewport);
            _currentBoard = new Board1(MAP_WIDTH, MAP_HEIGHT);
            var playerPosition = new Vector2(25 * TILE_SIZE, 20 * TILE_SIZE); 
            _player = new Player(playerPosition, _currentBoard);
        }

        protected override void Update(GameTime gameTime)
        {
            // Używamy switcha do zarządzania logiką w zależności od stanu
            switch (_currentState)
            {
                case GameState.MainMenu:
                    var nextState = _mainMenuScreen.Update();
                    if (nextState != GameState.MainMenu)
                    {
                        if (nextState == GameState.Gameplay)
                        {
                            StartGameplay(); // Inicjalizujemy grę
                            _currentState = GameState.Gameplay; // Zmieniamy stan
                        }
                        else if (nextState == GameState.Exit)
                        {
                            Exit();
                        }
                    }
                    break;

                case GameState.Gameplay:
                    UpdateGameplay(gameTime);
                    break;
            }

            base.Update(gameTime);
        }

        // Logika aktualizacji gry wydzielona do osobnej metody
        private void UpdateGameplay(GameTime gameTime)
        {
            if (_activeDialog != null)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Q)) _activeDialog = null;
                return;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Escape)) _currentState = GameState.MainMenu; // Wróć do menu

            string dialogToShow = _player.Update(gameTime);
            if (dialogToShow != null) ShowDialog(dialogToShow);

            var mapSizeInPixels = new Point(MAP_WIDTH * TILE_SIZE, MAP_HEIGHT * TILE_SIZE);
            _camera.Follow(_player.Position, mapSizeInPixels);
        }

        public void ShowDialog(string text)
        {
            _activeDialog = new DialogBox(text, _dialogFont, GraphicsDevice);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Używamy switcha do zarządzania rysowaniem w zależności od stanu
            switch (_currentState)
            {
                case GameState.MainMenu:
                    GraphicsDevice.Clear(Color.Black);
                    _spriteBatch.Begin();
                    _mainMenuScreen.Draw(_spriteBatch);
                    _spriteBatch.End();
                    break;
                
                case GameState.Gameplay:
                    DrawGameplay(gameTime);
                    break;
            }

            base.Draw(gameTime);
        }

        // Logika rysowania gry wydzielona do osobnej metody
        private void DrawGameplay(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin(transformMatrix: _camera.Transform);
            _currentBoard.Draw(_spriteBatch);
            _player.Draw(_spriteBatch);
            _spriteBatch.End();

            _spriteBatch.Begin();
            if (_activeDialog != null) _activeDialog.Draw(_spriteBatch);
            _spriteBatch.End();
        }
    }
}