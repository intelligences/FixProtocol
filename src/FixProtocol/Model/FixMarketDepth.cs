using System;
using System.Collections.Generic;
using System.Linq;

namespace Intelligences.FixProtocol.Model
{
    /// <summary>
    /// Стакан котировок
    /// </summary>
    public class FixMarketDepth
    {
        /// <summary>
        /// Инструмент стакана
        /// </summary>
        public string SecurityId { get; private set; }

        public List<FixQuote> asks = new List<FixQuote>();

        /// <summary>
        /// Аски
        /// </summary>
        public List<FixQuote> Asks
        {
            get
            {
                return this.asks;
            }
            set
            {
                this.asks = asks.OrderByDescending(x => x.GetPrice()).ToList();

                this.BestAsk = this.asks.LastOrDefault();
            }
        }

        public List<FixQuote> bids = new List<FixQuote>();

        /// <summary>
        /// Биды
        /// </summary>
        public List<FixQuote> Bids
        {
            get
            {
                return this.bids;
            }
            internal set
            {
                this.bids = value.OrderByDescending(x => x.GetPrice()).ToList();
                this.BestBid = this.bids.LastOrDefault();
            }
        }

        /// <summary>
        /// Best Bid
        /// </summary>
        public FixQuote BestBid { get; internal set; }

        /// <summary>
        /// Best Ask
        /// </summary>
        public FixQuote BestAsk { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTimeOffset UpdatedAt { get; internal set; }

        /// <summary>
        /// Конструктор.
        /// </summary>
        public FixMarketDepth(string securityId, DateTimeOffset updatedAt)
        {
            this.SecurityId = securityId;
            this.UpdatedAt = updatedAt;
        }
    }
}
