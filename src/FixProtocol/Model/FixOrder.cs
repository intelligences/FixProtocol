using Intelligences.FixProtocol.Enum;
using System;

namespace Intelligences.FixProtocol.Model
{
    public class FixOrder
    {
        /// <summary>
        /// Internal Unique ID for order
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Unique identifier for Order as assigned by the client
        /// </summary>
        public string ClientOrderId { get; set; }

        /// <summary>
        /// Unique identifier for Order as assigned by broker.
        /// </summary>
        public string OrderId { get; internal set; }

        /// <summary>
        /// Order quantity
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// Client account, default one will be used if not specified
        /// </summary>
        public string AccountId { get; private set; }

        /// <summary>
        /// Symbol
        /// </summary>
        public string SecurityId { get; internal set; }

        /// <summary>
        /// Order direction
        /// </summary>
        public FixDirection Direction { get; private set; }

        /// <summary>
        /// Type of order
        /// </summary>
        public FixOrderType Type { get; private set; }

        /// <summary>
        /// Price
        /// </summary>
        public decimal? Price { get; set; }

        /// <summary>
        /// Stop price, required for STOP, STOP LIMIT.
        /// </summary>
        public decimal? StopPrice { get; set; }

        public DateTimeOffset CreatedAt { get; internal set; }
        public DateTimeOffset? UpdatedAt { get; internal set; }
        public FixTimeInForce TimeInForce { get; internal set; }
        public FixOrderState State { get; internal set; }
        public decimal FilledQty { get; internal set; }
        public decimal FilledAveragePrice { get; internal set; }

        public FixOrder(
            decimal quantity,
            FixDirection direction,
            string accountId,
            string securityId,
            FixOrderType orderType
        ) {

            this.Quantity = quantity;
            this.Direction = direction;
            this.AccountId = accountId;
            this.SecurityId = securityId;
            this.Type = orderType;
            this.TimeInForce = FixTimeInForce.GoodTillCancel;
            this.State = FixOrderState.None;
        }
    }
}
