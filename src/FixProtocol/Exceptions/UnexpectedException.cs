using System;

namespace Intelligences.FixProtocol.Exceptions
{
    public class UnexpectedException : Exception
    {
        public UnexpectedException()
        {
        }

        public UnexpectedException(string message) : base(message)
        {
        }

        public UnexpectedException(string message, System.Exception innerException) : base(message, innerException)
        {
        }
    }
}
