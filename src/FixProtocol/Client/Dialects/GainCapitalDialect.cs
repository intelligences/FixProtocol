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
        /// New account event
        /// </summary>
        public event Action<FixAccount> NewAccount;

        /// <summary>
        /// Account changed event
        /// </summary>
        public event Action<FixAccount> AccountChanged;

        /// <summary>
        /// New position event
        /// </summary>
        public event Action<FixPosition> NewPosition;

        /// <summary>
        /// Position changed event
        /// </summary>
        public event Action<FixPosition> PositionChanged;

        /// <summary>
        /// New order event
        /// </summary>
        public event Action<FixOrder> NewOrder;

        /// <summary>
        /// Order changed event
        /// </summary>
        public event Action<FixOrder> OrderChanged;

        /// <summary>
        /// Order place failed event
        /// </summary>
        public event Action<FixOrderFail> OrderPlaceFailed;

        /// <summary>
        /// New my trade event
        /// </summary>
        public event Action<FixMyTrade> NewMyTrade;

        /// <summary>
        /// New security event
        /// </summary>
        public event Action<FixSecurity> NewSecurity;

        /// <summary>
        /// New Trade event
        /// </summary>
        public event Action<FixTrade> NewTrade;

        /// <summary>
        /// Trades unsubscribed event
        /// </summary>
        public event Action<string> TradesUnSubscribed;

        /// <summary>
        /// Market depth changed
        /// </summary>
        public event Action<FixMarketDepth> MarketDepthChanged;

        /// <summary>
        /// Market depth unsubscribed
        /// </summary>
        public event Action<string> MarketDepthUnsubscribed;

        private Model.FixSettings settings;

        public GainCapitalDialect(Model.FixSettings settings)
        {
            this.settings = settings;
        }

        /// <summary>
        /// Set session
        /// </summary>
        public void SetSession(Session session)
        {

        }

        public void CancelOrder(FixOrder order)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Request list of all securities
        /// </summary>
        /// <remarks>If cache enabled, load from cache and async reload</remarks>
        public void RequestSecurities() 
        {
            throw new NotImplementedException();
        }

        public void OrderMassStatusRequest()
        {
            throw new NotImplementedException();
        }

        public void AccountSummaryRequest()
        {
            throw new NotImplementedException();
        }

        public void FindSecurities(FixSecurityFilter securityFilter)
        {
            throw new NotImplementedException();
        }

        public void ModifyOrder(FixOrder order)
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

        public void PlaceOrder(FixOrder order)
        {
            throw new NotImplementedException();
        }

        public void PreOutputMessageAction(QuickFix.Message message)
        {
            throw new NotImplementedException();
        }

        public void SubscribeMarketDepth(string securityId)
        {
            throw new NotImplementedException();
        }

        public void SubscribeTrades(string securityId)
        {
            throw new NotImplementedException();
        }

        public void UnsubscribeMarketDepth(string securityId)
        {
            throw new NotImplementedException();
        }

        public void UnSubscribeTrades(string securityId)
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
