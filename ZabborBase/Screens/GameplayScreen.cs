// Screens/GameplayScreen.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using Zabbor.Core;
using Zabbor.Managers;
using Zabbor.ZabborBase;
using Zabbor.ZabborBase.Entities;
using Zabbor.ZabborBase.Enums;
using Zabbor.ZabborBase.Interfaces;
using Zabbor.ZabborBase.Managers;
using Zabbor.ZabborBase.Models;
using Zabbor.ZabborBase.UI;
using Zabbor.ZabborBase.World;

namespace Zabbor.Screens // Użyj swojej przestrzeni nazw
{
    public class GameplayScreen
    {
        // ---- Pola dla stanu Gameplay ----
        private Player _player;
        private Camera _camera;
        private DialogBox _activeDialog;
        private GraphicsDevice _graphicsDevice; // <-- POLE DO PRZECHOWYWANIA GraphicsDevice
        
        // ---- Wspólne zasoby i stałe ----
        public const int TILE_SIZE = 32;
        private const int MAP_WIDTH = 50;
        private const int MAP_HEIGHT = 40;
        private SpriteFont _dialogFont;
        private Dictionary<string, IGameMap> _maps;
        private string _currentMapId;
        private InventoryScreen _inventoryScreen;
        private bool _isInventoryOpen = false;
        private Dictionary<string, List<Point>> _removedItemsByMap = new Dictionary<string, List<Point>>();

        public GameplayScreen() { }

        public void Initialize(SaveData saveData, GraphicsDevice graphicsDevice, SpriteFont font)
        {
            _graphicsDevice = graphicsDevice; // <-- ZAPISUJEMY REFERENCJĘ
            _dialogFont = font;
            Placeholder.Create(graphicsDevice);
            DialogueManager.LoadDialogues();
            _camera = new Camera(graphicsDevice.Viewport);
            _inventoryScreen = new InventoryScreen(_dialogFont, graphicsDevice);

            _maps = new Dictionary<string, IGameMap>
            {
                { "board1", new GameMap("board1") },
                { "board2", new GameMap("board2") }
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
                _currentMapId = saveData.CurrentMapId;
                var playerTilePos = new Point(saveData.PlayerTilePosition.X, saveData.PlayerTilePosition.Y);
                var playerPosition = new Vector2(playerTilePos.X * TILE_SIZE, playerTilePos.Y * TILE_SIZE);
                _player = new Player(playerPosition, _maps[_currentMapId]);
                _player.Inventory.SetItems(saveData.PlayerInventory);
            }
            else
            {
                _currentMapId = "board1";
                var playerPosition = new Vector2(12 * TILE_SIZE, 9 * TILE_SIZE);
                _player = new Player(playerPosition, _maps[_currentMapId]);
            }
        }

        public GameState Update(GameTime gameTime)
        {
            if (_isInventoryOpen)
            {
                if (InputManager.WasKeyPressed(Keys.E) || InputManager.WasKeyPressed(Keys.Q))
                    _isInventoryOpen = false;
                return GameState.Gameplay;
            }

            if (_activeDialog != null)
            {
                if (InputManager.WasKeyPressed(Keys.Q))
                    _activeDialog = null;
                return GameState.Gameplay;
            }
            
            if (InputManager.WasKeyPressed(Keys.E))
                _isInventoryOpen = true;
            else if (InputManager.WasKeyPressed(Keys.Escape))
                return GameState.MainMenu;
            else if (InputManager.WasKeyPressed(Keys.F5))
                return GameState.ShowSaveScreen;
            else
            {
                var playerResult = _player.Update(gameTime);
                if (playerResult is string dialog)
                    _activeDialog = new DialogBox(dialog, _dialogFont, _graphicsDevice); // <-- POPRAWKA
                else if (playerResult is Warp warp)
                    ChangeMap(warp);
                else if (playerResult is WorldItem pickedItem)
                {
                    _player.Inventory.AddItem(pickedItem.ItemId);
                    _maps[_currentMapId].RemoveWorldItemAt(pickedItem.TilePosition);
                    if (!_removedItemsByMap.ContainsKey(_currentMapId))
                        _removedItemsByMap[_currentMapId] = new List<Point>();
                    _removedItemsByMap[_currentMapId].Add(pickedItem.TilePosition);
                    _activeDialog = new DialogBox($"{ItemManager.GetItem(pickedItem.ItemId).Name} został podniesiony.", _dialogFont, _graphicsDevice); // <-- POPRAWKA
                }
            }
            
            var mapSizeInPixels = new Point(MAP_WIDTH * TILE_SIZE, MAP_HEIGHT * TILE_SIZE);
            _camera.Follow(_player.Position, mapSizeInPixels);

            return GameState.Gameplay;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(transformMatrix: _camera.Transform);
            _maps[_currentMapId].Draw(spriteBatch);
            _player.Draw(spriteBatch);
            spriteBatch.End();

            spriteBatch.Begin();
            if (_activeDialog != null) _activeDialog.Draw(spriteBatch);
            if (_isInventoryOpen) _inventoryScreen.Draw(spriteBatch, _player.Inventory.GetItems());
            spriteBatch.End();
        }
        
        public SaveData GetSaveData()
        {
            var serializableRemovedItems = _removedItemsByMap.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Select(p => new SerializablePoint(p.X, p.Y)).ToList()
            );
            return new SaveData
            {
                SaveName = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                CurrentMapId = _currentMapId,
                PlayerTilePosition = new SerializablePoint((int)_player.Position.X / TILE_SIZE, (int)_player.Position.Y / TILE_SIZE),
                PlayerInventory = _player.Inventory.GetItems(),
                RemovedWorldItems = serializableRemovedItems // <-- POPRAWKA
            };
        }
        
        private void ChangeMap(Warp warp)
        {
            _currentMapId = warp.DestinationMapId;
            var newMap = _maps[_currentMapId];
            var newPosition = new Vector2(warp.DestinationTile.X * TILE_SIZE, warp.DestinationTile.Y * TILE_SIZE);
            _player.SetPosition(newPosition, newMap);
        }
    }
}