namespace Intelligences.FixProtocol.Model.Conditions
{
    /// <summary>
    /// Stop market order condition
    /// </summary>
    public class StopMarket : IOrderCondition
    {
        private readonly decimal stopPrice;

        public StopMarket(decimal stopPrice)
        {
            this.stopPrice = stopPrice;
        }

        public decimal GetPrice()
        {
            return this.stopPrice;
        }
    }
}
