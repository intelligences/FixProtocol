using System;
using System.Collections.Generic;
using System.Linq;

namespace Intelligences.FixProtocol.Model
{
    public class Portfolio
    {
        private readonly string name;
        private readonly decimal beginValue;
        private decimal usedMargin;
        private decimal currentValue;


        private readonly Dictionary<Security, Position> positions = new Dictionary<Security, Position>();

        public Portfolio(string name, decimal beginValue)
        {
            this.name = name;
            this.beginValue = beginValue;
        }

        public string GetName()
        {
            return this.name;
        }

        public decimal GetBeginValue()
        {
            return this.beginValue;
        }

        public decimal GetUsedMargin()
        {
            return this.usedMargin;
        }

        internal void SetUsedMargin(decimal usedMargin)
        {
            this.usedMargin = usedMargin;
        }

        public decimal GetCurrentValue()
        {
            return this.currentValue;
        }

        internal void SetCurrentValue(decimal currentValue)
        {
            this.currentValue = currentValue;
        }

        public List<Position> GetPositions()
        {
            return this.positions.Values.ToList();
        }

        internal Position GetPosition(Security security)
        {
            if (!this.positions.ContainsKey(security))
            {
                return null;
            }

            return this.positions[security];
        }

        internal void AddPosition(Position position)
        {
            if (position == null)
            {
                throw new ArgumentNullException("Argument is null");
            }

            Security security = position.GetSecurity();

            if (this.positions.ContainsKey(security))
            {
                throw new InvalidOperationException("Position already exists");
            }

            this.positions.Add(security, position);
        }
    }
}
