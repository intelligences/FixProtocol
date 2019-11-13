using System;

namespace Intelligences.FixProtocol.Model
{
    public class InvalidSettingsException : Exception
    {
        public InvalidSettingsException()
        {
        }

        public InvalidSettingsException(string message)
            : base(message)
        {
        }

        public InvalidSettingsException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
