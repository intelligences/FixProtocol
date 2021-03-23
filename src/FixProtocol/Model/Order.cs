using Intelligences.FixProtocol.Enum;
using System;

namespace Intelligences.FixProtocol.Model
{
    public class Order
    {
        private string transactionId;
        private string clientOrderId;
        private string orderId;
        private decimal quantity;
        private readonly Portfolio portfolio;
        private readonly Security security;
        private readonly Direction direction;
        private readonly OrderType orderType;
        private decimal price;
        private IOrderCondition condition;
        private DateTimeOffset createdAt;
        private DateTimeOffset? updatedAt;
        private TimeInForce timeInForce;
        private OrderState state;
        private decimal filledQty;

        public Order(
            decimal quantity,
            Direction direction,
            Portfolio portfolio,
            Security security,
            OrderType orderType
        ) {

            this.quantity = quantity;
            this.direction = direction;
            this.portfolio = portfolio;
            this.security = security;
            this.orderType = orderType;
            this.timeInForce = TimeInForce.GoodTillCancel;
            this.state = OrderState.None;
        }

        public string GetTransactionId()
        {
            return this.transactionId;
        }

        public string GetOrderId()
        {
            return this.orderId;
        }

        public decimal GetQuantity()
        {
            return this.quantity;
        }

        public void SetQuantity(decimal quantity)
        {
            this.quantity = quantity;
        }

        public Portfolio GetPortfolio()
        {
            return this.portfolio;
        }

        public Security GetSecurity()
        {
            return this.security;
        }

        public OrderType GetOrderType()
        {
            return this.orderType;
        }

        public TimeInForce GetTimeInForce()
        {
            return this.timeInForce;
        }

        public void SetTimeInForce(TimeInForce timeInForce)
        {
            this.timeInForce = timeInForce;
        }

        public Direction GetDirection()
        {
            return this.direction;
        }

        public decimal GetPrice()
        {
            return this.price;
        }

        public decimal GetFilledQty()
        {
            return this.filledQty;
        }

        public void SetPrice(decimal price)
        {
            this.price = price;
        }

        public IOrderCondition GetCondition()
        {
            return this.condition;
        }

        public void SetСondition(IOrderCondition condition)
        {
            this.condition = condition;
        }

        public DateTimeOffset GetCreatedAt()
        {
            return this.createdAt;
        }

        public DateTimeOffset? GetUpdatedAt()
        {
            return this.updatedAt;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public OrderState GetState()
        {
            return this.state;
        }

        internal void SetOrderId(string orderId)
        {
            this.orderId = orderId;
        }

        internal void SetState(OrderState orderState)
        {
            this.state = orderState;
        }

        internal void SetTransactionId(string transactionId)
        {
            this.transactionId = transactionId;
        }

        internal string GetClientOrderId()
        {
            return this.clientOrderId;
        }

        internal void SetClientOrderId(string clientOrderId)
        {
            this.clientOrderId = clientOrderId;
        }

        internal void SetCreatedAt(DateTimeOffset createdAt)
        {
            this.createdAt = createdAt;
        }

        internal void SetUpdatedAt(DateTimeOffset updatedAt)
        {
            this.updatedAt = updatedAt;
        }

        internal void SetFilledQty(decimal filledQty)
        {
            this.filledQty = filledQty;
        }
    }
}
