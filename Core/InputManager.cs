using Microsoft.Xna.Framework.Input;

namespace Zabbor.Core // Użyj swojej przestrzeni nazw
{
    public static class InputManager
    {
        private static KeyboardState _currentKeyboardState;
        private static KeyboardState _previousKeyboardState;

        // Ta metoda musi być wywołana raz na klatkę, na samym początku pętli Update
        public static void Update()
        {
            _previousKeyboardState = _currentKeyboardState;
            _currentKeyboardState = Keyboard.GetState();
        }

        // Sprawdza, czy klawisz jest wciśnięty (przytrzymany)
        public static bool IsKeyDown(Keys key)
        {
            return _currentKeyboardState.IsKeyDown(key);
        }

        // Sprawdza, czy klawisz został WŁAŚNIE wciśnięty (w tej klatce)
        public static bool WasKeyPressed(Keys key)
        {
            return _currentKeyboardState.IsKeyDown(key) && _previousKeyboardState.IsKeyUp(key);
        }
    }
}