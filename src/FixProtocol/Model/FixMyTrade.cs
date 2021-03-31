namespace Intelligences.FixProtocol.Model
{
    public class FixMyTrade
    {
        public FixTrade Trade { get; private set; }
        public FixOrder Order { get; private set; }

        public FixMyTrade(FixTrade trade, FixOrder order)
        {
            this.Trade = trade;
            this.Order = order;
        }
    }
}
