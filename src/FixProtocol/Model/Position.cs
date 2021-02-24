using System;

namespace Intelligences.FixProtocol.Model
{
    public class Position
    {
        private readonly Portfolio portfolio;
        private readonly Security security;
        private readonly decimal beginValue;
        private readonly string currency;
        private decimal currentValue;
        private DateTimeOffset updatedAt;
        private decimal? profitAndLoss;

        public Position(Portfolio portfolio, Security security, decimal beginValue, string currency)
        {
            this.portfolio = portfolio;
            this.security = security;
            this.beginValue = beginValue;
            this.currentValue = beginValue;
            this.currency = currency;
            this.updatedAt = DateTimeOffset.UtcNow;
        }

        public Portfolio GetPortfolio()
        {
            return this.portfolio;
        }

        public Security GetSecurity()
        {
            return this.security;
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

        public decimal? GetProfitAndLoss()
        {
            return this.profitAndLoss;
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

        internal void SetProfitAndLoss(decimal profitAndLoss)
        {
            this.profitAndLoss = profitAndLoss;
        }
    }
}
