using Intelligences.FixProtocol.Enum;
using Intelligences.FixProtocol.Model;
using System;

namespace Intelligences.FixProtocol.Sample.Exante.View
{
    public class OrderView
    {
        private Order order;

        public string TransactionId { get; }
        public string OrderId { get; private set; }
        public string PortfolioName { get; }
        public string SecurityId { get; }
        public DateTimeOffset CreatedAt { get; }
        public DateTimeOffset? UpdatedAt { get; private set; }
        public Direction Direction { get; }
        public OrderState OrderState { get; private set; }
        public OrderType OrderType { get; }
        public decimal Price { get; private set; }
        public decimal Quantity { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="order"></param>
        public OrderView(Order order)
        {
            this.order = order;
            this.TransactionId = order.GetTransactionId();
            this.PortfolioName = order.GetPortfolio().GetName();
            this.SecurityId = order.GetSecurity().GetId();
            this.CreatedAt = order.GetCreatedAt();
            this.Direction = order.GetDirection();
            this.OrderType = order.GetOrderType();

            this.UpdateView(order);
        }

        internal void UpdateView(Order order)
        {
            this.OrderId = order.GetOrderId();
            this.UpdatedAt = order.GetUpdatedAt();
            this.OrderState = order.GetState();
            this.Price = order.GetPrice();
            this.Quantity = order.GetQuantity();
        }

        internal Order GetOrder()
        {
            return this.order;
        }
    }
}
