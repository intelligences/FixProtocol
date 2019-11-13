namespace Intelligences.FixProtocol.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class Quote
    {
        public decimal price;
        public decimal volume;
        public Quote(decimal price, decimal volume)
        {
            this.price = price;
            this.volume = volume;
        }

        public decimal GetPrice()
        {
            return this.price;
        }

        public decimal GetVolume()
        {
            return this.volume;
        }
    }
}
