using Intelligences.FixProtocol.Enum;
using System;

namespace Intelligences.FixProtocol.DTO
{
    public class SecurityData
    {
        public string Id { get; set; }

        public string Code { get; set; }

        public string Board { get; set; }

        public SecurityType Type { get; set; }

        public string Currency { get; set; }

        public decimal PriceStep { get; set; }

        public decimal StepPrice { get; set; }

        public DateTimeOffset? ExpiryDate { get; set; }

        public int Decimals { get; set; }
    }
}
