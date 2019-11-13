namespace Intelligences.FixProtocol.Model.ConditionalOrders
{
    /// <summary>
    /// Stop limit order condition
    /// </summary>
    public class StopLimit : IOrderCondition
    {
        private readonly decimal stopPrice;

        public StopLimit(decimal stopPrice)
        {
            this.stopPrice = stopPrice;
        }

        public decimal GetPrice()
        {
            return this.stopPrice;
        }
    }
}
