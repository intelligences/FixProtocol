using Intelligences.FixProtocol.Enum;

namespace Intelligences.FixProtocol.Model
{
    public class FixStopMarketOrder : FixOrder
    {
        public FixStopMarketOrder(decimal price, decimal stopPrice, decimal volume, FixDirection direction, string portfolioId, string securityId) : base(volume, direction, portfolioId, securityId, FixOrderType.StopMarket)
        {
            this.Price = price;
            this.StopPrice = stopPrice;
        }
    }
}
