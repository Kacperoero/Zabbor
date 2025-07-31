using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Zabbor.Core;
using Zabbor.ZabborBase;
using Zabbor.ZabborBase.Interfaces;
using Zabbor.ZabborBase.UI;
using Zabbor.ZabborBase.World;
using Zabbor.ZabborBase.Enums;
using System.Collections.Generic;
using Zabbor.ZabborBase.Managers;
using Zabbor.ZabborBase.Models;
using Zabbor.ZabborBase.Entities;
using System.Linq;
using System;
using Zabbor.Managers;

namespace Zabbor
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // ---- Pola dla stanów gry ----
        private GameState _currentState;
        private MainMenuScreen _mainMenuScreen;
        private SaveLoadScreen _saveLoadScreen;
        
        // ---- Pola dla stanu Gameplay ----
        private Player _player;
        private Camera _camera;
        private DialogBox _activeDialog;
        
        // ---- Wspólne zasoby i stałe ----
        public const int TILE_SIZE = 32;
        private const int MAP_WIDTH = 50;
        private const int MAP_HEIGHT = 40;
        private SpriteFont _dialogFont;
        private Dictionary<string, IGameMap> _maps;
        private string _currentMapId;
        private InventoryScreen _inventoryScreen;
        private bool _isInventoryOpen = false;
        private KeyboardState _previousKeyboardState;
        private Dictionary<string, List<Point>> _removedItemsByMap = new Dictionary<string, List<Point>>();

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
            _mainMenuScreen = new MainMenuScreen(_dialogFont, GraphicsDevice.Viewport);
            _inventoryScreen = new InventoryScreen(_dialogFont, GraphicsDevice);
        }

        private void StartNewGame()
        {
            _removedItemsByMap.Clear();
            InitializeGameplay(null);
        }

        // POPRAWKA: Ta metoda przyjmuje teraz slotIndex
        private void LoadGameFromSlot(int slotIndex)
        {
            var saveData = SaveManager.LoadGame(slotIndex);
            InitializeGameplay(saveData);
        }

        private void InitializeGameplay(SaveData saveData)
        {
            Placeholder.Create(GraphicsDevice);
            DialogueManager.LoadDialogues();
            _camera = new Camera(GraphicsDevice.Viewport);
            _maps = new Dictionary<string, IGameMap>
            {
                { "Board1", new Board1(MAP_WIDTH, MAP_HEIGHT) },
                { "Board2", new Board2(MAP_WIDTH, MAP_HEIGHT) }
            };

            if (saveData != null)
            {
                var loadedRemovedItems = saveData.RemovedWorldItems ?? new Dictionary<string, List<SerializablePoint>>();
                _removedItemsByMap = loadedRemovedItems.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Select(p => new Point(p.X, p.Y)).ToList()
                );

                foreach (var mapEntry in _removedItemsByMap)
                {
                    if (_maps.ContainsKey(mapEntry.Key))
                        _maps[mapEntry.Key].RemoveItems(mapEntry.Value);
                }
            }

            if (saveData != null)
            {
                _currentMapId = saveData.CurrentMapId;
                var playerTilePos = new Point(saveData.PlayerTilePosition.X, saveData.PlayerTilePosition.Y);
                var playerPosition = new Vector2(playerTilePos.X * TILE_SIZE, playerTilePos.Y * TILE_SIZE);
                _player = new Player(playerPosition, _maps[_currentMapId]);
                _player.Inventory.SetItems(saveData.PlayerInventory);
            }
            else
            {
                _currentMapId = "Board1";
                var playerPosition = new Vector2(12 * TILE_SIZE, 9 * TILE_SIZE);
                _player = new Player(playerPosition, _maps[_currentMapId]);
            }
        }
        
        // POPRAWKA: Ta metoda przyjmuje teraz slotIndex
        private void SaveCurrentGame(int slotIndex)
        {
            var serializableRemovedItems = new Dictionary<string, List<SerializablePoint>>();
            foreach (var mapEntry in _removedItemsByMap)
            {
                serializableRemovedItems.Add(mapEntry.Key, mapEntry.Value.Select(p => new SerializablePoint(p.X, p.Y)).ToList());
            }

            var saveData = new SaveData
            {
                SaveName = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                CurrentMapId = _currentMapId,
                PlayerTilePosition = new SerializablePoint((int)_player.Position.X / TILE_SIZE, (int)_player.Position.Y / TILE_SIZE),
                PlayerInventory = _player.Inventory.GetItems(),
                RemovedWorldItems = serializableRemovedItems
            };
            
            SaveManager.SaveGame(saveData, slotIndex);
            ShowDialog("Gra zapisana!");
        }

        protected override void Update(GameTime gameTime)
        {
            switch (_currentState)
            {
                case GameState.MainMenu:
                    var nextState = _mainMenuScreen.Update();
                    if (nextState != GameState.MainMenu)
                    {
                        // POPRAWKA: Uzupełniono logikę dla NewGame
                        if (nextState == GameState.NewGame)
                        {
                            StartNewGame();
                            _currentState = GameState.Gameplay;
                        }
                        else if (nextState == GameState.ShowLoadScreen)
                        {
                            _saveLoadScreen = new SaveLoadScreen(_dialogFont, GraphicsDevice.Viewport, SaveLoadMode.Load, Keyboard.GetState());
                            _currentState = nextState;
                        }
                        else if (nextState == GameState.Exit) Exit();
                        
                        // Odśwież menu (np. żeby pojawiła się opcja "Wczytaj"), jeśli wrócimy do niego później
                         _mainMenuScreen = new MainMenuScreen(_dialogFont, GraphicsDevice.Viewport);
                    }
                    break;

                case GameState.ShowLoadScreen:
                case GameState.ShowSaveScreen:
                    int selectedSlot = _saveLoadScreen.Update();
                    if (selectedSlot >= 0)
                    {
                        if (_currentState == GameState.ShowLoadScreen)
                            LoadGameFromSlot(selectedSlot);
                        else
                            SaveCurrentGame(selectedSlot);
                        
                        _currentState = GameState.Gameplay;
                    }
                    else if (selectedSlot == -1)
                    {
                        _currentState = (_currentState == GameState.ShowSaveScreen) ? GameState.Gameplay : GameState.MainMenu;
                    }
                    break;

                case GameState.Gameplay:
                    UpdateGameplay(gameTime);
                    break;
            }
            base.Update(gameTime);
        }

        private void UpdateGameplay(GameTime gameTime)
        {
            var kState = Keyboard.GetState();

            if (_isInventoryOpen)
            {
                if (kState.IsKeyDown(Keys.I) && _previousKeyboardState.IsKeyUp(Keys.I))
                    _isInventoryOpen = false;
                
                _previousKeyboardState = kState;
                return;
            }

            if (_activeDialog != null)
            {
                if (kState.IsKeyDown(Keys.Q) && _previousKeyboardState.IsKeyUp(Keys.Q))
                    _activeDialog = null;

                _previousKeyboardState = kState;
                return;
            }
            
            if (kState.IsKeyDown(Keys.I) && _previousKeyboardState.IsKeyUp(Keys.I))
            {
                _isInventoryOpen = true;
            }
            if (kState.IsKeyDown(Keys.Escape))
            {
                _currentState = GameState.MainMenu;
                // TWORZYMY MENU NA NOWO, ABY ODŚWIEŻYĆ LISTĘ OPCJI
                _mainMenuScreen = new MainMenuScreen(_dialogFont, GraphicsDevice.Viewport);
            }
            else if (kState.IsKeyDown(Keys.F5) && _previousKeyboardState.IsKeyUp(Keys.F5))
            {
                _saveLoadScreen = new SaveLoadScreen(_dialogFont, GraphicsDevice.Viewport, SaveLoadMode.Save, kState);
                _currentState = GameState.ShowSaveScreen;
            }
            else
            {
                var playerResult = _player.Update(gameTime);
                if (playerResult is string dialog)
                    ShowDialog(dialog);
                else if (playerResult is Warp warp)
                    ChangeMap(warp);
                else if (playerResult is WorldItem pickedItem)
                {
                    _player.Inventory.AddItem(pickedItem.ItemId);
                    _maps[_currentMapId].RemoveWorldItemAt(pickedItem.TilePosition);

                    if (!_removedItemsByMap.ContainsKey(_currentMapId))
                        _removedItemsByMap[_currentMapId] = new List<Point>();
                    
                    _removedItemsByMap[_currentMapId].Add(pickedItem.TilePosition);
                    ShowDialog($"{ItemManager.GetItem(pickedItem.ItemId).Name} został podniesiony.");
                }
            }
            
            var mapSizeInPixels = new Point(MAP_WIDTH * TILE_SIZE, MAP_HEIGHT * TILE_SIZE);
            _camera.Follow(_player.Position, mapSizeInPixels);
            
            _previousKeyboardState = kState;
        }

        private void ChangeMap(Warp warp)
        {
            _currentMapId = warp.DestinationMapId;
            var newMap = _maps[_currentMapId];
            var newPosition = new Vector2(warp.DestinationTile.X * TILE_SIZE, warp.DestinationTile.Y * TILE_SIZE);
            _player.SetPosition(newPosition, newMap);
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
                
                case GameState.ShowLoadScreen:
                case GameState.ShowSaveScreen:
                    GraphicsDevice.Clear(Color.Black);
                    _spriteBatch.Begin();
                    _saveLoadScreen.Draw(_spriteBatch);
                    _spriteBatch.End();
                    break;

                case GameState.Gameplay:
                    DrawGameplay(gameTime);
                    break;
            }

            base.Draw(gameTime);
        }

        private void DrawGameplay(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin(transformMatrix: _camera.Transform);
            _maps[_currentMapId].Draw(_spriteBatch);
            _player.Draw(_spriteBatch);
            _spriteBatch.End();

            _spriteBatch.Begin();
            if (_activeDialog != null) _activeDialog.Draw(_spriteBatch);
            if (_isInventoryOpen) _inventoryScreen.Draw(_spriteBatch, _player.Inventory.GetItems());
            _spriteBatch.End();
        }
    }
}