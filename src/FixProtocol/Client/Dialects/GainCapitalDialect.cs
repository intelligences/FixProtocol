using Intelligences.FixProtocol.DTO;
using Intelligences.FixProtocol.Filter;
using Intelligences.FixProtocol.Model;
using QuickFix;
using QuickFix.FIX44;
using System;

namespace Intelligences.FixProtocol.Client.Dialects
{
    internal class GainCapitalDialect : IDialectClient
    {
        /// <summary>
        /// New portfolio event
        /// </summary>
        public event Action<Portfolio> NewPortfolio;

        /// <summary>
        /// Portfolio changed event
        /// </summary>
        public event Action<Portfolio> PortfolioChanged;

        /// <summary>
        /// New position event
        /// </summary>
        public event Action<Position> NewPosition;

        /// <summary>
        /// Position changed event
        /// </summary>
        public event Action<Position> PositionChanged;

        /// <summary>
        /// New order event
        /// </summary>
        public event Action<Order> NewOrder;

        /// <summary>
        /// Order changed event
        /// </summary>
        public event Action<Order> OrderChanged;

        /// <summary>
        /// Order place failed event
        /// </summary>
        public event Action<OrderFail> OrderPlaceFailed;

        /// <summary>
        /// New my trade event
        /// </summary>
        public event Action<MyTrade> NewMyTrade;

        /// <summary>
        /// New security event
        /// </summary>
        public event Action<Security> NewSecurity;

        /// <summary>
        /// New Trade event
        /// </summary>
        public event Action<Trade> NewTrade;

        /// <summary>
        /// Trades unsubscribed event
        /// </summary>
        public event Action<Security> TradesUnSubscribed;

        /// <summary>
        /// Market depth changed
        /// </summary>
        public event Action<MarketDepth> MarketDepthChanged;

        /// <summary>
        /// Market depth unsubscribed
        /// </summary>
        public event Action<Security> MarketDepthUnsubscribed;

        private Model.Settings settings;

        public GainCapitalDialect(Model.Settings settings)
        {
            this.settings = settings;
        }

        /// <summary>
        /// Set session
        /// </summary>
        public void SetSession(Session session)
        {

        }

        public void AccountSummaryRequest()
        {
            throw new NotImplementedException();
        }

        public void CancelOrder(Order order)
        {
            throw new NotImplementedException();
        }

        public void FindSecurities(SecurityFilter securityFilter)
        {
            throw new NotImplementedException();
        }

        public void CreateSecurity(SecurityData securityData)
        {
            throw new NotImplementedException();
        }

        public void ModifyOrder(Order order)
        {
            throw new NotImplementedException();
        }

        public void OrderMassStatusRequest()
        {
            throw new NotImplementedException();
        }

        public void ParseAccountSummaryResponse(QuickFix.Message message)
        {
            throw new NotImplementedException();
        }

        public void ParseMarketDataRequestReject(MarketDataRequestReject message)
        {
            throw new NotImplementedException();
        }

        public void ParseMarketDataSnapshotFullRefresh(MarketDataSnapshotFullRefresh message)
        {
            throw new NotImplementedException();
        }

        public void ParseOrdersExecutionReport(ExecutionReport message)
        {
            throw new NotImplementedException();
        }

        public void ParseSecuritiesList(SecurityList message)
        {
            throw new NotImplementedException();
        }

        public void PlaceOrder(Order order)
        {
            throw new NotImplementedException();
        }

        public void PreOutputMessageAction(QuickFix.Message message)
        {
            throw new NotImplementedException();
        }

        public void SubscribeMarketDepth(Security security)
        {
            throw new NotImplementedException();
        }

        public void SubscribeTrades(Security security)
        {
            throw new NotImplementedException();
        }

        public void UnsubscribeMarketDepth(Security security)
        {
            throw new NotImplementedException();
        }

        public void UnSubscribeTrades(Security security)
        {
            throw new NotImplementedException();
        }

        public void OrderCancelReject(QuickFix.FIX44.OrderCancelReject rejectResponse)
        {

        }

        public void OrderCancelReplaceRequest(QuickFix.FIX44.OrderCancelReplaceRequest rejectResponse)
        {

        }
    }
}
