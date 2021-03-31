using Intelligences.FixProtocol.Enum;

namespace Intelligences.FixProtocol.Model
{
    public class FixQuote
    {
        public decimal Price { get; private set; }
        public decimal Volume { get; private set; }
        public FixDirection Direction { get; private set; }

        public FixQuote(decimal price, decimal volume, FixDirection direction)
        {
            this.Price = price;
            this.Volume = volume;
            this.Direction = direction;
        }
    }
}
