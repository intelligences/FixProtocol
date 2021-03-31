using Intelligences.FixProtocol.Enum;
using Newtonsoft.Json;
using System;

namespace Intelligences.FixProtocol.Model
{
    /// <summary>
    /// Модель инструмента
    /// </summary>
    [Serializable]
    public class FixSecurity
    {
        /// <summary>
        /// Security identifier
        /// </summary>
        [JsonProperty]
        public string Id { get; internal set; }

        /// <summary>
        /// Security code / symbol
        /// </summary>
        [JsonProperty]
        public string Code { get; internal set; }

        /// <summary>
        /// Exchange board
        /// </summary>
        [JsonProperty]
        public string Board { get; internal set; }

        /// <summary>
        /// Security type
        /// </summary>
        [JsonProperty]
        public SecurityType Type { get; internal set; }

        /// <summary>
        /// Base security currency
        /// </summary>
        [JsonProperty]
        public string Currency { get; internal set; }

        /// <summary>
        /// Minimal price step
        /// </summary>
        [JsonProperty] 
        public decimal PriceStep { get; internal set; }

        /// <summary>
        /// Minimal step price
        /// </summary>
        [JsonProperty] 
        public decimal StepPrice { get; internal set; }

        /// <summary>
        /// Minimal step of volume
        /// </summary>
        [JsonProperty] 
        public decimal VolumeStep { get; internal set; }

        /// <summary>
        /// Expiration date
        /// </summary>
        [JsonProperty] 
        public DateTimeOffset? ExpiryDate { get; internal set; }

        /// <summary>
        /// Type of option (only for <see cref="SecurityType.Option"/> type of security)
        /// </summary>
        [JsonProperty]
        public FixOptionType? OptionType { get; internal set; }

        /// <summary>
        /// Strike price
        /// </summary>
        [JsonProperty]
        public decimal? StrikePrice { get; internal set; }

        public FixSecurity(string id)
        {
            this.Id = id;
        }
    }
}
