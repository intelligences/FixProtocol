using System;

namespace Intelligences.FixProtocol.Model
{
    public class FixOrderFail
    {
        public FixOrder Order { get; private set; }
        public Exception Exception { get; private set; }

        public FixOrderFail(FixOrder order, Exception exception)
        {
            this.Order = order;
            this.Exception = exception;
        }
    }
}
