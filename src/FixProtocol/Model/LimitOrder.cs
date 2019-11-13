using Intelligences.FixProtocol.Enum;

namespace Intelligences.FixProtocol.Model
{
    public class LimitOrder : Order
    {
        public LimitOrder(decimal price, decimal volume, Direction direction, Portfolio portfolio, Security security) : base(volume, direction, portfolio, security, OrderType.Limit)
        {
            this.SetPrice(price);
        }
    }
}
