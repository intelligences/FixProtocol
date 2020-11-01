using System;

namespace Intelligences.FixProtocol.Model
{
    public class OrderFail
    {
        private readonly Order order;
        private readonly Exception exception;

        public OrderFail(Order order, Exception exception)
        {
            this.order = order;
            this.exception = exception;
        }

        public Order GetOrder()
        {
            return this.order;
        }

        public Exception GetError()
        {
            return this.exception;
        }
    }
}
