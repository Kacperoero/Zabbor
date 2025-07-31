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
using System.Collections.Generic;
using Zabbor.ZabborBase.Managers;
using Zabbor.ZabborBase.Models;
using Zabbor.ZabborBase.Entities;
using System.Linq;

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
            
            _currentState = GameState.MainMenu; // Zaczynamy od menu głównego

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            
            // Ładujemy tylko zasoby potrzebne od razu (dla menu)
            _dialogFont = Content.Load<SpriteFont>("dialog_font");
            _mainMenuScreen = new MainMenuScreen(_dialogFont, GraphicsDevice.Viewport);
            _inventoryScreen = new InventoryScreen(_dialogFont, GraphicsDevice);
        }

        private void StartNewGame()
        {
            _removedItemsByMap.Clear(); // Czyścimy stan usuniętych przedmiotów
            InitializeGameplay(null); // Inicjalizujemy grę bez danych zapisu
        }

        private void LoadGameFromSave()
        {
            var saveData = SaveManager.LoadGame();
            if (saveData != null)
            {
                InitializeGameplay(saveData); // Inicjalizujemy grę Z danymi zapisu
            }
            else // Jeśli plik jest uszkodzony lub nie istnieje, zacznij nową grę
            {
                StartNewGame();
            }
        }

        private void InitializeGameplay(SaveData saveData)
        {
            // Ten fragment ładuje zasoby, które nie były potrzebne w menu
            Placeholder.Create(GraphicsDevice);
            DialogueManager.LoadDialogues();
            
            _camera = new Camera(GraphicsDevice.Viewport);

            _maps = new Dictionary<string, IGameMap>
            {
                { "Board1", new Board1(MAP_WIDTH, MAP_HEIGHT) },
                { "Board2", new Board2(MAP_WIDTH, MAP_HEIGHT) }
            };
            
            // Jeśli wczytujemy grę, musimy najpierw odtworzyć stan usuniętych przedmiotów
            if (saveData != null)
            {
                // Konwertujemy wczytany słownik na taki z normalnymi Pointami
                var loadedRemovedItems = saveData.RemovedWorldItems ?? new Dictionary<string, List<SerializablePoint>>();
                _removedItemsByMap = loadedRemovedItems.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Select(p => new Point(p.X, p.Y)).ToList()
                );

                foreach (var mapEntry in _removedItemsByMap)
                {
                    if (_maps.ContainsKey(mapEntry.Key))
                    {
                        _maps[mapEntry.Key].RemoveItems(mapEntry.Value);
                    }
                }
            }

            // Teraz konfigurujemy gracza i mapę
            if (saveData != null)
            {
                _currentMapId = saveData.CurrentMapId;
                var playerTilePos = new Point(saveData.PlayerTilePosition.X, saveData.PlayerTilePosition.Y);
                var playerPosition = new Vector2(playerTilePos.X * TILE_SIZE, playerTilePos.Y * TILE_SIZE);
                _player = new Player(playerPosition, _maps[_currentMapId]);
                _player.Inventory.SetItems(saveData.PlayerInventory);
            }
            else // To jest kod dla nowej gry
            {
                _currentMapId = "Board1";
                var playerPosition = new Vector2(12 * TILE_SIZE, 9 * TILE_SIZE);
                _player = new Player(playerPosition, _maps[_currentMapId]);
            }
        }

        private void SaveCurrentGame()
        {
            // Ręcznie tworzymy słownik z poprawnymi typami
            var serializableRemovedItems = new Dictionary<string, List<SerializablePoint>>();
            foreach (var mapEntry in _removedItemsByMap)
            {
                var serializableList = new List<SerializablePoint>();
                foreach (var point in mapEntry.Value)
                {
                    serializableList.Add(new SerializablePoint(point.X, point.Y));
                }
                serializableRemovedItems.Add(mapEntry.Key, serializableList);
            }

            var saveData = new SaveData
            {
                CurrentMapId = _currentMapId,
                PlayerTilePosition = new SerializablePoint((int)_player.Position.X / TILE_SIZE, (int)_player.Position.Y / TILE_SIZE),
                PlayerInventory = _player.Inventory.GetItems(),
                RemovedWorldItems = serializableRemovedItems // Przypisujemy ręcznie stworzony słownik
            };
            
            SaveManager.SaveGame(saveData);
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
                        if (nextState == GameState.NewGame)
                        {
                            StartNewGame();
                            _currentState = GameState.Gameplay;
                        }
                        else if (nextState == GameState.LoadGame)
                        {
                            LoadGameFromSave();
                            _currentState = GameState.Gameplay;
                        }
                        else if (nextState == GameState.Exit) Exit();
                        _mainMenuScreen = new MainMenuScreen(_dialogFont, GraphicsDevice.Viewport); // Odśwież menu
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

            // PRIORYTET 1: Jeśli otwarty jest ekwipunek
            if (_isInventoryOpen)
            {
                // Zamykamy go po ponownym wciśnięciu "I"
                if (kState.IsKeyDown(Keys.I) && _previousKeyboardState.IsKeyUp(Keys.I))
                {
                    _isInventoryOpen = false;
                }
                _previousKeyboardState = kState;
                return; // Zatrzymujemy resztę logiki
            }

            // PRIORYTET 2: Jeśli otwarty jest dialog
            if (_activeDialog != null)
            {
                if (kState.IsKeyDown(Keys.Q))
                {
                    _activeDialog = null;
                }
                _previousKeyboardState = kState;
                return; // Zatrzymujemy resztę logiki
            }

            // --- Poniższy kod wykona się tylko, jeśli żadne okno nie jest otwarte ---

            // Otwieranie ekwipunku
            if (kState.IsKeyDown(Keys.I) && _previousKeyboardState.IsKeyUp(Keys.I))
            {
                _isInventoryOpen = true;
            }

            // Powrót do menu głównego
            if (kState.IsKeyDown(Keys.Escape))
            {
                _currentState = GameState.MainMenu;
            }

            if (kState.IsKeyDown(Keys.F5) && _previousKeyboardState.IsKeyUp(Keys.F5))
            {
                SaveCurrentGame();
            }

            // Aktualizacja gracza i jego akcji
            var playerResult = _player.Update(gameTime);
            if (playerResult is string dialog)
            {
                ShowDialog(dialog);
            }
            else if (playerResult is Warp warp)
            {
                ChangeMap(warp);
            }
            else if (playerResult is WorldItem pickedItem)
            {
                // 1. Dodaj do ekwipunku gracza
                _player.Inventory.AddItem(pickedItem.ItemId);
                
                // 2. Usuń z mapy
                _maps[_currentMapId].RemoveWorldItemAt(pickedItem.TilePosition);

                // 3. Zarejestruj usunięcie na potrzeby zapisu gry
                if (!_removedItemsByMap.ContainsKey(_currentMapId))
                {
                    _removedItemsByMap[_currentMapId] = new List<Point>();
                }
                _removedItemsByMap[_currentMapId].Add(pickedItem.TilePosition);

                // 4. Pokaż komunikat
                ShowDialog($"{ItemManager.GetItem(pickedItem.ItemId).Name} został podniesiony.");
            }

            // Aktualizacja kamery
            var mapSizeInPixels = new Point(MAP_WIDTH * TILE_SIZE, MAP_HEIGHT * TILE_SIZE);
            _camera.Follow(_player.Position, mapSizeInPixels);

            // Zawsze na końcu zapisujemy stan klawiatury
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