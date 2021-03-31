using Intelligences.FixProtocol.Enum;
using Intelligences.FixProtocol.Model;
using QuickFix;
using QuickFix.Fields;
using System;
using System.Globalization;
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
        public static string GetSecurityId(this FixSecurity security)
        {
            if (security == null)
            {
                return null;
            }

            return security.Code + "." + security.Board;
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
                case "FMXXXX":
                    type = SecurityType.CalendarSpread;
                    break;
                case "MMXXXX":
                    type = SecurityType.Swap;
                    break;
            }

            return (SecurityType) type;
        }

        public static char ToFixOrderSide(this Direction direction)
        {
            return direction == Direction.Buy ? Side.BUY : Side.SELL;
        }

        public static Direction ToDirection(this Side side)
        {
            return side.getValue() == Side.BUY ? Direction.Buy : Direction.Sell;
        }

        public static FixOrderType ToOrderType(this OrdType ordType)
        {
            FixOrderType type = FixOrderType.Limit;
            
            switch (ordType.getValue())
            {
                case OrdType.MARKET:
                    type = FixOrderType.Market;
                    break;
                case OrdType.LIMIT:
                    type = FixOrderType.Limit;
                    break;
                case OrdType.STOP_LIMIT:
                    type = FixOrderType.StopLimit;
                    break;
                case OrdType.STOP:
                    type = FixOrderType.StopMarket;
                    break;
                default:
                    throw new UnsupportedMessageType();
                    break;
            }

            return type;
        }

        public static FixOrderState ToOrderState(this OrdStatus ordStatus)
        {
            switch (ordStatus.getValue())
            {
                case OrdStatus.PENDING_NEW:
                    return FixOrderState.PendingRegistration;
                case OrdStatus.NEW:
                    return FixOrderState.New;
                case OrdStatus.PARTIALLY_FILLED:
                    return FixOrderState.PartialFilled;
                case OrdStatus.FILLED:
                    return FixOrderState.Filled;
                case OrdStatus.PENDING_CANCEL:
                    return FixOrderState.PendingCancel;
                case OrdStatus.CANCELED:
                    return FixOrderState.Canceled;
                case OrdStatus.REJECTED:
                    return FixOrderState.Rejected;

                default:
                    throw new ArgumentException("Invalid order type for fix protocol");
            }
        }

        public static char ToFixOrderType(this FixOrder order)
        {
            switch (order.Type)
            {
                case FixOrderType.Market:
                    return OrdType.MARKET;
                case FixOrderType.Limit:
                    return OrdType.LIMIT;
                case FixOrderType.StopMarket:
                    return OrdType.STOP;
                case FixOrderType.StopLimit:
                    return OrdType.STOP_LIMIT;
                default:
                    throw new ArgumentException("Invalid order type for fix protocol");
            }
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
