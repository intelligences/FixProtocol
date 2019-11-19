using System;

namespace Intelligences.FixProtocol.Model
{
    public class Trade
    {
        public Security security;
        public decimal price;
        public decimal volume;
        public DateTimeOffset dateTime;

        public Trade(Security security, decimal price, decimal volume, DateTimeOffset dateTime)
        {
            this.security = security;
            this.price = price;
            this.volume = volume;
            this.dateTime = dateTime;
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

        public DateTimeOffset GetDateTime()
        {
            return this.dateTime;
        }
    }
}
