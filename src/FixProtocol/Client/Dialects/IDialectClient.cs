using ExecutionReport = QuickFix.FIX44.ExecutionReport;
using Intelligences.FixProtocol.Model;
using Intelligences.FixProtocol.Filter;
using MarketDataSnapshotFullRefresh = QuickFix.FIX44.MarketDataSnapshotFullRefresh;
using MarketDataRequestReject = QuickFix.FIX44.MarketDataRequestReject;
using System;
using SecurityList = QuickFix.FIX44.SecurityList;
using QuickFix;
using Intelligences.FixProtocol.DTO;

namespace Intelligences.FixProtocol.Client.Dialects
{
    internal interface IDialectClient
    {
        /// <summary>
        /// New portfolio event
        /// </summary>
        event Action<Portfolio> NewPortfolio;

        /// <summary>
        /// Portfolio changed event
        /// </summary>
        event Action<Portfolio> PortfolioChanged;

        /// <summary>
        /// New position event
        /// </summary>
        event Action<Position> NewPosition;

        /// <summary>
        /// Position changed event
        /// </summary>
        event Action<Position> PositionChanged;

        /// <summary>
        /// New order event
        /// </summary>
        event Action<Order> NewOrder;

        /// <summary>
        /// Order changed event
        /// </summary>
        event Action<Order> OrderChanged;

        /// <summary>
        /// Order place failed event
        /// </summary>
        event Action<OrderFail> OrderPlaceFailed;

        /// <summary>
        /// New my trade event
        /// </summary>
        event Action<MyTrade> NewMyTrade;

        /// <summary>
        /// New security event
        /// </summary>
        event Action<Security> NewSecurity;

        /// <summary>
        /// New Trade event
        /// </summary>
        event Action<Trade> NewTrade;

        /// <summary>
        /// Trades unsubscribed event
        /// </summary>
        event Action<Security> TradesUnSubscribed;

        /// <summary>
        /// Market depth changed
        /// </summary>
        event Action<MarketDepth> MarketDepthChanged;

        /// <summary>
        /// Market depth unsubscribed
        /// </summary>
        event Action<Security> MarketDepthUnsubscribed;

        /// <summary>
        /// Set session
        /// </summary>
        void SetSession(Session session);

        /// <summary>
        /// Subscribe on market depth
        /// </summary>
        /// <param name="security">Security</param>
        void SubscribeMarketDepth(Security security);

        /// <summary>
        /// Unsubscribe from Market Depth
        /// </summary>
        /// <param name="security"></param>
        void UnsubscribeMarketDepth(Security security);

        /// <summary>
        /// Place new order on the exchange
        /// </summary>
        /// <param name="order">New order</param>
        void PlaceOrder(Order order);

        /// <summary>
        /// Find securities on the exchange
        /// </summary>
        /// <param name="securityFilter">Security filter <see cref="SecurityFilter"/></param>
        void FindSecurities(SecurityFilter securityFilter);

        void CreateSecurity(SecurityData securityData);

        /// <summary>
        /// Subscribe Trades
        /// </summary>
        /// <param name="security">Security <see cref="Security"/></param>
        void SubscribeTrades(Security security);

        /// <summary>
        /// UnSubscribe Trades
        /// </summary>
        /// <param name="security">Security <see cref="Security"/></param>
        void UnSubscribeTrades(Security security);

        /// <summary>
        /// Modify order
        /// </summary>
        /// <param name="order">Order <see cref="Order"/></param>
        void ModifyOrder(Order order);

        /// <summary>
        /// Cancel order
        /// </summary>
        /// <param name="order">Order <see cref="Order"/></param>
        void CancelOrder(Order order);

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