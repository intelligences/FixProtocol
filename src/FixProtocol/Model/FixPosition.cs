using System;

namespace Intelligences.FixProtocol.Model
{
    /// <summary>
    /// Fix Position
    /// </summary>
    public class FixPosition
    {
        /// <summary>
        /// Portfolio identifier / name
        /// </summary>
        public string PortfolioId { get; private set; }

        /// <summary>
        /// Security identifier
        /// </summary>
        public string SecurityId { get; private set; }

        /// <summary>
        /// Currency
        /// </summary>
        public string Currency { get; private set; }

        /// <summary>
        /// Current value
        /// </summary>
        public decimal CurrentValue { get; internal set; }

        /// <summary>
        /// Position average price
        /// </summary>
        public decimal AveragePrice { get; internal set; }

        /// <summary>
        /// Updated at date time
        /// </summary>
        public DateTimeOffset UpdatedAt { get; internal set; }

        /// <summary>
        /// Profit and loss
        /// </summary>
        public decimal? ProfitAndLoss { get; internal set; }

        public FixPosition(string portfolioId, string securityId, string currency, decimal currentValue)
        {
            this.PortfolioId = portfolioId;
            this.SecurityId = securityId;
            this.CurrentValue = currentValue;
            this.Currency = currency;
            this.UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}
