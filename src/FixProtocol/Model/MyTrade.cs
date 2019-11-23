namespace Intelligences.FixProtocol.Model
{
    public class MyTrade
    {
        private readonly Trade trade;
        private readonly Order order;

        public MyTrade(Trade trade, Order order)
        {
            this.trade = trade;
            this.order = order;
        }

        public Trade GetTrade()
        {
            return this.trade;
        }

        public Order GetOrder()
        {
            return this.order;
        }
    }
}
