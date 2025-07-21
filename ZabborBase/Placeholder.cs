using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Zabbor.ZabborBase
{
    // Ta klasa reprezentuje prosty, jednokolorowy prostokąt
    public class Placeholder
    {
        // Statyczna tekstura 1x1 piksela, współdzielona przez wszystkie obiekty
        public static Texture2D Texture { get; private set; }
        
        public Vector2 Position { get; set; }
        public Point Size { get; set; }
        public Color Color { get; set; }

        public Placeholder(Vector2 position, Point size, Color color)
        {
            Position = position;
            Size = size;
            Color = color;
        }

        // Metoda do jednorazowego stworzenia tekstury 1x1
        public static void Create(GraphicsDevice graphicsDevice)
        {
            Texture = new Texture2D(graphicsDevice, 1, 1);
            Texture.SetData([Color.White]);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Rysujemy, rozciągając teksturę 1x1 do docelowego rozmiaru i nadając jej kolor
            spriteBatch.Draw(Texture, new Rectangle(Position.ToPoint(), Size), Color);
        }
    }
}