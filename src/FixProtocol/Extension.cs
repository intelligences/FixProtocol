using Intelligences.FixProtocol.Enum;
using Intelligences.FixProtocol.Model;
using QuickFix.Fields;
using System;
using System.Globalization;
using System.Linq;
using SecurityType = Intelligences.FixProtocol.Enum.SecurityType;
using TimeInForce = QuickFix.Fields.TimeInForce;
using TimeInForceEnum = Intelligences.FixProtocol.Enum.TimeInForce;

namespace Intelligences.FixProtocol
{
    public static class Extension
    {
        private const string fastFormatDate = "yyyyMMddHHmmss";

        /// <summary>
        /// Convert date time to FAST date format
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns>FAST date time</returns>
        public static long ToFastDateTime(this DateTime dateTime)
        {
            string stringDate = dateTime.ToString(Extension.fastFormatDate, CultureInfo.CreateSpecificCulture("en-US"));

            return long.Parse(stringDate, CultureInfo.CreateSpecificCulture("en-US"));
        }

        /// <summary>
        /// Получить идентификатр инструмента
        /// </summary>
        /// <param name="security"></param>
        /// <returns></returns>
        public static string GetSecurityId(this Security security)
        {
            if (security == null)
            {
                return null;
            }

            return security.GetCode() + "." + security.GetBoard();
        }

        /// <summary>
        /// Тип инструмента в CFI Code
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string ToCFICode(this SecurityType? type)
        {
            string code = "";

            switch (type)
            {
                case SecurityType.Forex:
                    code = "MRCXXX";
                    break;
                case SecurityType.Future:
                    code = "FXXXXX";
                    break;
                case SecurityType.Stock:
                    code = "EXXXXX";
                    break;
                case SecurityType.CallOption:
                    code = "OCXXXX";
                    break;
                case SecurityType.PutOption:
                    code = "OPXXXX";
                    break;
                case SecurityType.Fond:
                    code = "EUXXXX";
                    break;
                case SecurityType.Bond:
                    code = "DBXXXX";
                    break;
            }

            return code;
        }

        /// <summary>
        /// Тип инструмента в CFI Code
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static SecurityType ToSecurityType(this string code)
        {
            SecurityType? type = null;

            switch (code)
            {
                case "MRCXXX":
                    type = SecurityType.Forex;
                    break;
                case "FXXXXX":
                    type = SecurityType.Future;
                    break;
                case "EXXXXX":
                    type = SecurityType.Stock;
                    break;
                case "OCXXXX":
                    type = SecurityType.CallOption;
                    break;
                case "OPXXXX":
                    type = SecurityType.PutOption;
                    break;
                case "EUXXXX":
                    type = SecurityType.Fond;
                    break;
                case "DBXXXX":
                    type = SecurityType.Bond;
                    break;
            }

            return (SecurityType) type;
        }

        public static int PriceStepToNumderOfDigits(this decimal digits)
        {
            return digits.ToString().Split(',').Last().Length;
        }

        public static char ToFixOrderSide(this Direction direction)
        {
            return direction == Direction.Buy ? Side.BUY : Side.SELL;
        }

        public static Direction ToDirection(this Side side)
        {
            return side.getValue() == Side.BUY ? Direction.Buy : Direction.Sell;
        }

        public static OrderType ToOrderType(this OrdType ordType)
        {
            OrderType type = OrderType.Limit;
            
            switch (ordType.getValue())
            {
                case OrdType.MARKET:
                    type = OrderType.Market;
                    break;
                case OrdType.LIMIT:
                    type = OrderType.Limit;
                    break;
                default:
                    type = OrderType.Conditional;
                    break;
            }

            return type;
        }

        public static OrderState ToOrderState(this OrdStatus ordStatus)
        {
            OrderState type = OrderState.Active;

            switch (ordStatus.getValue())
            {
                case OrdStatus.CANCELED:
                    type = OrderState.Canceled;
                    break;
                case OrdStatus.PARTIALLY_FILLED:
                    type = OrderState.PartialFilled;
                    break;
                case OrdStatus.FILLED:
                    type = OrderState.Filled;
                    break;
                case OrdStatus.REJECTED:
                    type = OrderState.Failed;
                    break;
            }

            return type;
        }
        


        public static char ToFixOrderType(this OrderType orderType)
        {
            char ordType = OrdType.LIMIT;

            switch (orderType)
            {
                case OrderType.Market:
                    ordType = OrdType.MARKET;
                    break;
                case OrderType.Limit:
                    ordType = OrdType.LIMIT;
                    break;
                //case OrderType.Stop:
                //    ordType = new OrdType(OrdType.STOP);
                //    break;
            }

            return ordType;
        }

        public static char ToFixTimeInForce(this TimeInForceEnum timeInForce)
        {
            char type = TimeInForce.GOOD_TILL_CANCEL;

            switch (timeInForce)
            {
                case TimeInForceEnum.GoodTillCancel:
                    type = TimeInForce.GOOD_TILL_CANCEL;
                    break;
                case TimeInForceEnum.Day:
                    type = TimeInForce.DAY;
                    break;
                case TimeInForceEnum.FillOrKill:
                    type = TimeInForce.FILL_OR_KILL;
                    break;
                case TimeInForceEnum.AtTheClose:
                    type = TimeInForce.AT_THE_CLOSE;
                    break;
            }

            return type;
        }
    }
}
