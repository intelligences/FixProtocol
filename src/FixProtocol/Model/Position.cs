using System;

namespace Intelligences.FixProtocol.Model
{
    public class Position
    {
        private readonly Portfolio portfolio;
        private readonly string securityCode;
        private readonly decimal beginValue;
        private readonly string currency;
        private decimal currentValue;
        private DateTimeOffset updatedAt;

        public Position(Portfolio portfolio, string securityCode, decimal beginValue, string currency)
        {
            this.portfolio = portfolio;
            this.securityCode = securityCode;
            this.beginValue = beginValue;
            this.currency = currency;
            this.updatedAt = DateTimeOffset.UtcNow;
        }

        public Portfolio GetPortfolio()
        {
            return this.portfolio;
        }

        public string GetSecurityCode()
        {
            return this.securityCode;
        }

        public decimal GetBeginValue()
        {
            return this.beginValue;
        }

        public string GetCurrency()
        {
            return this.currency;
        }

        public decimal GetCurrentValue()
        {
            return this.currentValue;
        }

        public DateTimeOffset GetUpdatedAt()
        {
            return this.updatedAt;
        }

        internal void SetCurrentValue(decimal currentValue)
        {
            this.currentValue = currentValue;
        }

        internal void SetUpdatedAt(DateTimeOffset updatedAt)
        {
            this.updatedAt = updatedAt;
        }
    }
}
