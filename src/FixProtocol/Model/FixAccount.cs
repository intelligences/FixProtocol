namespace Intelligences.FixProtocol.Model
{
    /// <summary>
    /// Account of balance
    /// </summary>
    public class FixAccount
    {
        /// <summary>
        /// Account used for the request.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Currency for converted values.
        /// </summary>
        public string Currency { get; private set; }

        /// <summary>
        /// Amount of currency used for margin.
        /// </summary>
        public decimal UsedMargin { get; internal set; }

        /// <summary>
        /// Total Net Asset Value for account positions.
        /// </summary>
        public decimal TotalNewValue { get; internal set; }

        public FixAccount(string name, string currency, decimal currentValue)
        {
            this.Name = name;
            this.Currency = currency;
            this.TotalNewValue = currentValue;
        }
    }
}
