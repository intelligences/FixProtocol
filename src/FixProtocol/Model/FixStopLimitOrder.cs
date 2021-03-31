using Intelligences.FixProtocol.Enum;

namespace Intelligences.FixProtocol.Model
{
    public class FixStopLimitOrder : FixOrder
    {
        public FixStopLimitOrder(decimal price, decimal stopPrice, decimal volume, Direction direction, string portfolioId, string securityId) : base(volume, direction, portfolioId, securityId, FixOrderType.StopLimit)
        {
            this.Price = price;
            this.StopPrice = stopPrice;
        }
    }
}
