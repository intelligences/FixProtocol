using Intelligences.FixProtocol.Enum;

namespace Intelligences.FixProtocol.Filter
{
    /// <summary>
    /// Security filter
    /// </summary>
    public class SecurityFilter
    {
        /// <summary>
        /// Security Code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Security type
        /// </summary>
        public SecurityType? Type { get; set; }
    }
}
