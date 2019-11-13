using Intelligences.FixProtocol.Enum;

namespace Intelligences.FixProtocol.Model
{
    public class ConditionalOrder : Order
    {
        public ConditionalOrder(IOrderCondition conditionalOrder, decimal volume, Direction direction, Portfolio portfolio, Security security) : base(volume, direction, portfolio, security, OrderType.Conditional)
        {
            this.SetСondition(conditionalOrder);
        }
    }
}
