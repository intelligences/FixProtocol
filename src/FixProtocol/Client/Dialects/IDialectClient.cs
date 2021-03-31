using ExecutionReport = QuickFix.FIX44.ExecutionReport;
using Intelligences.FixProtocol.Model;
using Intelligences.FixProtocol.Filter;
using MarketDataSnapshotFullRefresh = QuickFix.FIX44.MarketDataSnapshotFullRefresh;
using MarketDataRequestReject = QuickFix.FIX44.MarketDataRequestReject;
using System;
using SecurityList = QuickFix.FIX44.SecurityList;
using QuickFix;

namespace Intelligences.FixProtocol.Client.Dialects
{
    internal interface IDialectClient
    {
        /// <summary>
        /// New Account
        /// </summary>
        event Action<FixAccount> NewAccount;

        /// <summary>
        /// Account changed
        /// </summary>
        event Action<FixAccount> AccountChanged;

        /// <summary>
        /// New position event
        /// </summary>
        event Action<FixPosition> NewPosition;

        /// <summary>
        /// Position changed event
        /// </summary>
        event Action<FixPosition> PositionChanged;

        /// <summary>
        /// New order event
        /// </summary>
        event Action<FixOrder> NewOrder;

        /// <summary>
        /// Order changed event
        /// </summary>
        event Action<FixOrder> OrderChanged;

        /// <summary>
        /// Order place failed event
        /// </summary>
        event Action<FixOrderFail> OrderPlaceFailed;

        /// <summary>
        /// New my trade event
        /// </summary>
        event Action<FixMyTrade> NewMyTrade;

        /// <summary>
        /// New security event
        /// </summary>
        event Action<FixSecurity> NewSecurity;

        /// <summary>
        /// New Trade event
        /// </summary>
        event Action<FixTrade> NewTrade;

        /// <summary>
        /// Trades unsubscribed event
        /// </summary>
        event Action<string> TradesUnSubscribed;

        /// <summary>
        /// Market depth changed
        /// </summary>
        event Action<FixMarketDepth> MarketDepthChanged;

        /// <summary>
        /// Market depth unsubscribed
        /// </summary>
        event Action<string> MarketDepthUnsubscribed;

        /// <summary>
        /// Set session
        /// </summary>
        void SetSession(Session session);

        /// <summary>
        /// Subscribe on market depth
        /// </summary>
        /// <param name="securityId">Security</param>
        void SubscribeMarketDepth(string securityId);

        /// <summary>
        /// Unsubscribe from Market Depth
        /// </summary>
        /// <param name="securityId"></param>
        void UnsubscribeMarketDepth(string securityId);

        /// <summary>
        /// Place new order on the exchange
        /// </summary>
        /// <param name="order">New order</param>
        void PlaceOrder(FixOrder order);

        /// <summary>
        /// Find securities on the exchange
        /// </summary>
        /// <param name="securityFilter">Security filter <see cref="SecurityFilter"/></param>
        void FindSecurities(SecurityFilter securityFilter);

        /// <summary>
        /// Request list of all securities
        /// </summary>
        /// <remarks>If cache enabled, load from cache and async reload</remarks>
        void RequestSecurities();

        /// <summary>
        /// Subscribe Trades
        /// </summary>
        /// <param name="securityId">Security <see cref="FixSecurity"/></param>
        void SubscribeTrades(string securityId);

        /// <summary>
        /// UnSubscribe Trades
        /// </summary>
        /// <param name="security">Security <see cref="FixSecurity"/></param>
        void UnSubscribeTrades(string securityId);

        /// <summary>
        /// Modify order
        /// </summary>
        /// <param name="order">Order <see cref="FixOrder"/></param>
        void ModifyOrder(FixOrder order);

        /// <summary>
        /// Cancel order
        /// </summary>
        /// <param name="order">Order <see cref="FixOrder"/></param>
        void CancelOrder(FixOrder order);

        /// <summary>
        /// Request to get orders
        /// </summary>
        void OrderMassStatusRequest();

        /// <summary>
        /// Account summary request
        /// </summary>
        void AccountSummaryRequest();

        /// <summary>
        /// Parse account info about portfolio and positions
        /// </summary>
        /// <param name="message">Fix message</param>
        void ParseAccountSummaryResponse(QuickFix.Message message);

        /// <summary>
        /// Parse orders execution info
        /// </summary>
        /// <param name="message">Fix message</param>
        void ParseOrdersExecutionReport(ExecutionReport message);

        /// <summary>
        /// Parse securities list
        /// </summary>
        /// <param name="message">Fix message</param>
        void ParseSecuritiesList(SecurityList message);

        /// <summary>
        /// Actions before sending message to exchange
        /// </summary>
        /// <param name="message">FIX message</param>
        void PreOutputMessageAction(QuickFix.Message message);

        /// <summary>
        /// Parse market data snapshot
        /// </summary>
        /// <param name="message">FIX message</param>
        void ParseMarketDataSnapshotFullRefresh(MarketDataSnapshotFullRefresh message);

        /// <summary>
        /// Parse market data request reject
        /// </summary>
        /// <param name="message">FIX message</param>
        void ParseMarketDataRequestReject(MarketDataRequestReject message);

        void OrderCancelReject(QuickFix.FIX44.OrderCancelReject rejectResponse);

        void OrderCancelReplaceRequest(QuickFix.FIX44.OrderCancelReplaceRequest rejectResponse);
    }
}