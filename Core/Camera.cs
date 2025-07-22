// Core/Camera.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Zabbor.Core // Użyj swojej przestrzeni nazw
{
    public class Camera
    {
        public Matrix Transform { get; private set; }
        private Vector2 _position;
        private readonly Viewport _viewport;

        public Camera(Viewport viewport)
        {
            _viewport = viewport;
            _position = Vector2.Zero;
        }

        public void Follow(Vector2 target, Point mapSizeInPixels)
        {
            // Obliczamy pozycję kamery, centrując ją na celu (graczu)
            // Odejmujemy połowę ekranu, aby cel był na środku, a nie w lewym górnym rogu
            var position = target - new Vector2(_viewport.Width / 2, _viewport.Height / 2);

            // Ograniczamy ruch kamery, aby nie pokazywała pustki za mapą
            // Używamy Clamp do "zakleszczenia" pozycji w dozwolonym obszarze
            _position.X = MathHelper.Clamp(position.X, 0, mapSizeInPixels.X - _viewport.Width);
            _position.Y = MathHelper.Clamp(position.Y, 0, mapSizeInPixels.Y - _viewport.Height);

            // Tworzymy macierz, która przesuwa cały świat gry w przeciwnym kierunku niż kamera
            // To daje iluzję, że kamera się porusza
            Transform = Matrix.CreateTranslation(-_position.X, -_position.Y, 0);
        }
    }
}