using System;
using System.Collections.Generic;
using System.Linq;

namespace Intelligences.FixProtocol.Model
{
    /// <summary>
    /// Стакан котировок
    /// </summary>
    public class MarketDepth
    {
        /// <summary>
        /// Инструмент стакана
        /// </summary>
        private Security security;

        /// <summary>
        /// Биды
        /// </summary>
        private List<Quote> asks = new List<Quote>();

        /// <summary>
        /// Аски
        /// </summary>
        private List<Quote> bids = new List<Quote>();

        /// <summary>
        /// 
        /// </summary>
        private Quote bestBid;

        /// <summary>
        /// 
        /// </summary>
        private Quote bestAsk;

        /// <summary>
        /// 
        /// </summary>
        private DateTimeOffset updatedAt;

        /// <summary>
        /// Конструктор.
        /// </summary>
        public MarketDepth(Security security, DateTimeOffset updatedAt)
        {
            this.security = security;
            this.updatedAt = updatedAt;
        }

        /// <summary>
        /// Получить инструмент стакана
        /// </summary>
        /// <returns></returns>
        public Security GetSecurity()
        {
            return this.security;
        }

        public DateTimeOffset GetUpdatedAt()
        {
            return this.updatedAt;
        }

        internal void SetUpdatedAt(DateTimeOffset updatedAt)
        {
            this.updatedAt = updatedAt;
        }

        /// <summary>
        /// Получить биды
        /// </summary>
        /// <returns></returns>
        public List<Quote> GetBids()
        {
            return this.bids;
        }

        /// <summary>
        /// Установить биды
        /// </summary>
        public void SetBids(List<Quote> bids)
        {
            this.bids = bids.OrderByDescending(x => x.GetPrice()).ToList();

            this.bestBid = this.bids.LastOrDefault();
        }

        /// <summary>
        /// Получить аски
        /// </summary>
        /// <returns></returns>
        public List<Quote> GetAsks()
        {
            return this.asks;
        }

        /// <summary>
        /// Установить аски
        /// </summary>
        public void SetAsks(List<Quote> asks)
        {
            this.asks = asks.OrderByDescending(x => x.GetPrice()).ToList();

            this.bestAsk = this.asks.LastOrDefault();
        }
    }
}
