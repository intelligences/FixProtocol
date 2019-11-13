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
        private Dictionary<decimal, decimal> asks = new Dictionary<decimal, decimal>();

        /// <summary>
        /// Аски
        /// </summary>
        private Dictionary<decimal, decimal> bids = new Dictionary<decimal, decimal>();

        /// <summary>
        /// Глубина котировок
        /// </summary>
        private int depth;

        /// <summary>
        /// Конструктор.
        /// </summary>
        public MarketDepth(Security security, int depth)
        {
            this.security = security;
            this.depth = depth;
        }

        /// <summary>
        /// Получить инструмент стакана
        /// </summary>
        /// <returns></returns>
        public Security GetSecurity()
        {
            return this.security;
        }

        /// <summary>
        /// Получить глубину
        /// </summary>
        /// <returns></returns>
        public int GetDepth()
        {
            return this.depth;
        }

        /// <summary>
        /// Получить биды
        /// </summary>
        /// <returns></returns>
        public Dictionary<decimal, decimal> GetBids()
        {
            return this.bids;
        }

        /// <summary>
        /// Установить биды
        /// </summary>
        public void SetBids(Dictionary<decimal, decimal> bids)
        {
            this.bids = bids.OrderByDescending(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
        }

        /// <summary>
        /// Получить аски
        /// </summary>
        /// <returns></returns>
        public Dictionary<decimal, decimal> GetAsks()
        {
            return this.asks;
        }

        /// <summary>
        /// Установить аски
        /// </summary>
        public void SetAsks(Dictionary<decimal, decimal> asks)
        {
            this.asks = asks.OrderByDescending(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
        }
    }
}
