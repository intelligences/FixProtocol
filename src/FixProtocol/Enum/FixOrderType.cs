namespace Intelligences.FixProtocol.Enum
{
    /// <summary>
    /// Типы заявок
    /// </summary>
    public enum FixOrderType
    {
        /// <summary>
        /// Market order
        /// </summary>
        Market,

        /// <summary>
        /// Limit order
        /// </summary>
        Limit,

        /// <summary>
        /// Stop Limit order
        /// </summary>
        StopLimit,

        /// <summary>
        /// Stop market order
        /// </summary>
        StopMarket,
    }
}
