namespace Zabbor.ZabborBase.Models
{
    public class SerializablePoint
    {
        public int X { get; set; }
        public int Y { get; set; }

        // Dodajemy pusty konstruktor, którego wymaga deserializator
        public SerializablePoint() { } 

        public SerializablePoint(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}