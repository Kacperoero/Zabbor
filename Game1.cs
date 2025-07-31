using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Zabbor.ZabborBase.Enums;
using Zabbor.ZabborBase.Managers;
using Zabbor.ZabborBase.UI;
using Zabbor.Screens;
using Zabbor.Core;

namespace Zabbor
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        
        // ---- Główne pola ----
        private GameState _currentState;
        private SpriteFont _dialogFont;
        
        // ---- Ekrany Gry ----
        private MainMenuScreen _mainMenuScreen;
        private SaveLoadScreen _saveLoadScreen;
        private GameplayScreen _gameplayScreen;

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
            _currentState = GameState.MainMenu;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _dialogFont = Content.Load<SpriteFont>("dialog_font");
            _mainMenuScreen = new MainMenuScreen(_dialogFont, GraphicsDevice.Viewport, false);
        }

        protected override void Update(GameTime gameTime)
        {
            InputManager.Update();

            switch (_currentState)
            {
                case GameState.MainMenu:
                    var nextState = _mainMenuScreen.Update();
                    if (nextState != GameState.MainMenu) HandleMenuSelection(nextState);
                    break;
                
                case GameState.ShowLoadScreen:
                case GameState.ShowSaveScreen:
                    int selectedSlot = _saveLoadScreen.Update();
                    HandleSaveLoadSelection(selectedSlot);
                    break;

                case GameState.Gameplay:
                    var gameplayResult = _gameplayScreen.Update(gameTime);
                    if (gameplayResult != GameState.Gameplay) HandleMenuSelection(gameplayResult);
                    break;
            }
            base.Update(gameTime);
        }

        private void HandleMenuSelection(GameState nextState)
        {
            _currentState = nextState;
            if (nextState == GameState.NewGame)
            {
                _gameplayScreen = new GameplayScreen();
                _gameplayScreen.Initialize(null, GraphicsDevice, _dialogFont);
                _currentState = GameState.Gameplay;
            }
            else if (nextState == GameState.ShowLoadScreen)
            {
                _saveLoadScreen = new SaveLoadScreen(_dialogFont, GraphicsDevice.Viewport, SaveLoadMode.Load);
            }
            else if (nextState == GameState.Exit) Exit();
            else if (nextState == GameState.ResumeGame)
            {
                _currentState = GameState.Gameplay;
            }
            
            if (_currentState == GameState.MainMenu)
            {
                _mainMenuScreen = new MainMenuScreen(_dialogFont, GraphicsDevice.Viewport, _gameplayScreen != null);
            }
        }
        
        private void HandleSaveLoadSelection(int selectedSlot)
        {
            if (selectedSlot >= 0)
            {
                if (_currentState == GameState.ShowLoadScreen)
                {
                    var saveData = SaveManager.LoadGame(selectedSlot);
                    _gameplayScreen = new GameplayScreen();
                    _gameplayScreen.Initialize(saveData, GraphicsDevice, _dialogFont);
                }
                else // Tryb zapisu
                {
                    var saveData = _gameplayScreen.GetSaveData();
                    SaveManager.SaveGame(saveData, selectedSlot);
                }
                _currentState = GameState.Gameplay;
            }
            else if (selectedSlot == -1) // Anulowano
            {
                _currentState = (_currentState == GameState.ShowSaveScreen) ? GameState.Gameplay : GameState.MainMenu;
                if (_currentState == GameState.MainMenu)
                    _mainMenuScreen = new MainMenuScreen(_dialogFont, GraphicsDevice.Viewport, _gameplayScreen != null);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            
            switch (_currentState)
            {
                case GameState.MainMenu:
                    _spriteBatch.Begin();
                    _mainMenuScreen.Draw(_spriteBatch);
                    _spriteBatch.End();
                    break;
                
                case GameState.ShowLoadScreen:
                case GameState.ShowSaveScreen:
                    _spriteBatch.Begin();
                    _saveLoadScreen.Draw(_spriteBatch);
                    _spriteBatch.End();
                    break;

                case GameState.Gameplay:
                    _gameplayScreen.Draw(_spriteBatch);
                    break;
            }
            base.Draw(gameTime);
        }
    }
}