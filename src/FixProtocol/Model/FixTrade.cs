using System;

namespace Intelligences.FixProtocol.Model
{
    public class FixTrade
    {
        public string SecurityId { get; private set; }
        public decimal Price { get; private set; }
        public decimal Volume { get; private set; }
        public DateTimeOffset DateTime { get; private set; }

        public FixTrade(string securityId, decimal price, decimal volume, DateTimeOffset dateTime)
        {
            this.SecurityId = securityId;
            this.Price = price;
            this.Volume = volume;
            this.DateTime = dateTime;
        }
    }
}
