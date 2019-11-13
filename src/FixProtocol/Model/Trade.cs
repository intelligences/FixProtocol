namespace Intelligences.FixProtocol.Model
{
    public class Trade
    {
        public Security security;
        public decimal price;
        public decimal volume;

        public Trade(Security security, decimal price, decimal volume)
        {
            this.security = security;
            this.price = price;
            this.volume = volume;
        }

        public Security GetSecurity()
        {
            return this.security;
        }

        public decimal GetPrice()
        {
            return this.price;
        }

        public decimal GetVolume()
        {
            return this.volume;
        }
    }
}
