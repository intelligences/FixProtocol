using System;

namespace Intelligences.FixProtocol.Exceptions
{
    public class SecurityException : Exception
    {
        public SecurityException()
        {

        }

        public SecurityException(string message) : base(message)
        {
        }

        public SecurityException(string message, System.Exception innerException) : base(message, innerException)
        {
        }
    }
}
