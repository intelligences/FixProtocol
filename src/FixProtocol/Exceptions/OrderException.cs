using System;

namespace Intelligences.FixProtocol.Exceptions
{
    public class OrderException : Exception
    {
        public OrderException(string message) : base(message)
        {
        }

        public OrderException(string message, System.Exception innerException) : base(message, innerException)
        {
        }

        public OrderException()
        {
        }
    }
}
