using Intelligences.FixProtocol.Enum;

namespace Intelligences.FixProtocol.Model
{
    public class FixLimitOrder : FixOrder
    {
        public FixLimitOrder(decimal price, decimal volume, Direction direction, string portfolioId, string securityId) : base(volume, direction, portfolioId, securityId, FixOrderType.Limit)
        {
            this.Price = price;
        }
    }
}
