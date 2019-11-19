using Intelligences.FixProtocol.Enum;

namespace Intelligences.FixProtocol.Model
{
    public class Quote
    {
        private decimal price;
        private decimal volume;
        private Direction direction;

        public Quote(decimal price, decimal volume, Direction direction)
        {
            this.price = price;
            this.volume = volume;
            this.direction = direction;
        }

        public decimal GetPrice()
        {
            return this.price;
        }

        public decimal GetVolume()
        {
            return this.volume;
        }

        public Direction GetDirection()
        {
            return this.direction;
        }
    }
}
