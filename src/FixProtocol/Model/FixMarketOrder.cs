using Intelligences.FixProtocol.Enum;

namespace Intelligences.FixProtocol.Model
{
    public class FixMarketOrder : FixOrder
    {
        public FixMarketOrder(decimal volume, Direction direction, string portfolioId, string securityId) : base(volume, direction, portfolioId, securityId, FixOrderType.Market)
        {
 
        }
    }
}
