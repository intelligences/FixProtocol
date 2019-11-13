using Intelligences.FixProtocol.Enum;

namespace Intelligences.FixProtocol.Model
{
    public class MarketOrder : Order
    {
        public MarketOrder(decimal volume, Direction direction, Portfolio portfolio, Security security) : base(volume, direction, portfolio, security, OrderType.Market)
        {
 
        }
    }
}
