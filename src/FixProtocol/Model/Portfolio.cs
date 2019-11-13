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


        private readonly Dictionary<string, Position> positions = new Dictionary<string, Position>();

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

        internal Position GetPosition(string securityId)
        {
            if (!this.positions.ContainsKey(securityId))
            {
                return null;
            }

            return this.positions[securityId];
        }

        internal void AddPosition(Position position)
        {
            if (position == null)
            {
                throw new ArgumentNullException("Argument is null");
            }

            string code = position.GetSecurityCode();

            if (this.positions.ContainsKey(code))
            {
                throw new ArgumentException("Invalid argument");
            }

            this.positions.Add(code, position);
        }
    }
}
