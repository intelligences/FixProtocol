using System;

namespace Intelligences.FixProtocol.Model
{
    public class FixInvalidSettingsException : Exception
    {
        public FixInvalidSettingsException()
        {
        }

        public FixInvalidSettingsException(string message)
            : base(message)
        {
        }

        public FixInvalidSettingsException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
