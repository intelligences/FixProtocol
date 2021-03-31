using Intelligences.FixProtocol.Enum;
using System;

namespace Intelligences.FixProtocol.Model
{
    public class FixOrder
    {
        public string ClientOrderId { get; set; }
        public string OrderId { get; internal set; }
        public decimal Quantity { get; set; }
        public string AccountId { get; private set; }
        public string SecurityId { get; internal set; }
        public FixDirection Direction { get; private set; }
        public FixOrderType Type { get; private set; }
        public decimal Price { get; set; }
        public decimal StopPrice { get; set; }
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
