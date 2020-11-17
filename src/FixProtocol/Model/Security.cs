using Intelligences.FixProtocol.Enum;
using System;

namespace Intelligences.FixProtocol.Model
{
    /// <summary>
    /// Модель инструмента
    /// </summary>
    public class Security
    {
        /// <summary>
        /// Security identifier
        /// </summary>
        private string id;

        /// <summary>
        /// Security code / symbol
        /// </summary>
        private string code;

        /// <summary>
        /// Exchange board
        /// </summary>
        private string board;

        /// <summary>
        /// Security type
        /// </summary>
        private SecurityType type;

        /// <summary>
        /// Base security currency
        /// </summary>
        private string currency;

        /// <summary>
        /// Minimal price step
        /// </summary>
        private decimal priceStep;

        /// <summary>
        /// Minimal step price
        /// </summary>
        private decimal stepPrice;

        /// <summary>
        /// Minimal step of volume
        /// </summary>
        private decimal volumeStep;

        /// <summary>
        /// Expiration date
        /// </summary>
        private DateTimeOffset? expiryDate;

        /// <summary>
        /// Type of option (only for <see cref="SecurityType.Option"/> type of security)
        /// </summary>
        private OptionType? optionType;

        /// <summary>
        /// Num of digits in price
        /// </summary>
        private int digits;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="id"></param>
        public Security(string id)
        {
            this.id = id;
        }

        public string GetId()
        {
            return this.id;
        }

        public string GetCode()
        {
            return this.code;
        }

        public string GetBoard()
        {
            return this.board;
        }

        public SecurityType GetSecurityType()
        {
            return this.type;
        }

        public string GetCurrency()
        {
            return this.currency;
        }

        public decimal GetPriceStep()
        {
            return this.priceStep;
        }

        public decimal GetStepPrice()
        {
            return this.stepPrice;
        }

        public int GetDigits()
        {
            return this.digits;
        }

        public decimal GetVolumeStep()
        {
            return this.volumeStep;
        }

        public DateTimeOffset? GetExpiryDate()
        {
            return this.expiryDate;
        }

        public void SetExpiryDate(DateTimeOffset? dateTime)
        {
            this.expiryDate = dateTime;
        }

        public void SetOptionType(OptionType? optionType)
        {
            this.optionType = optionType;
        }

        public OptionType? GetOptionType()
        {
            return this.optionType;
        }

        internal void SetCode(string code)
        {
            this.code = code;
        }

        internal void SetBoard(string board)
        {
            this.board = board;
        }

        internal void SetCurrency(string currency)
        {
            this.currency = currency;
        }

        internal void SetSecurityType(SecurityType type)
        {
            this.type = type;
        }

        internal void SetPriceStep(decimal priceStep)
        {
            this.priceStep = priceStep;
        }

        internal void SetStepPrice(decimal stepPrice)
        {
            this.stepPrice = stepPrice;
        }

        internal void SetDigits(int digits)
        {
            this.digits = digits;
        }

        internal void SetVolumeStep(decimal volumeStep)
        {
            this.volumeStep = volumeStep;
        }
    }
}
