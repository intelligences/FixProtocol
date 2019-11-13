using System;

namespace Intelligences.FixProtocol.Exceptions
{
    public class PositionException : Exception
    {
        public PositionException()
        {
        }

        public PositionException(string message) : base(message)
        {
        }

        public PositionException(string message, System.Exception innerException) : base(message, innerException)
        {
        }
    }
}
