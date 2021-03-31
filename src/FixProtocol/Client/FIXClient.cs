using ExecutionReport = QuickFix.FIX44.ExecutionReport;
using Intelligences.FixProtocol.Client.Dialects;
using Intelligences.FixProtocol.Enum;
using Intelligences.FixProtocol.Fields;
using Intelligences.FixProtocol.Filter;
using Intelligences.FixProtocol.Model;
using QuickFix;
using QuickFix.Fields;
using System;
using System.Threading;
using Tags = QuickFix.Fields.Tags;

namespace Intelligences.FixProtocol.Client
{
    internal class FIXClient : MessageCracker, IApplication, IDisposable
    {
        /// <summary>
        /// Событие подключения к потоку для торговли
        /// </summary>
        internal event Action Connected;

        /// <summary>
        /// Собыие отключениия от потока для торговли
        /// </summary>
        internal event Action Disconnected;

        /// <summary>
        /// New account of balance
        /// </summary>
        internal event Action<FixAccount> NewAccount;

        /// <summary>
        /// Account of balance changed
        /// </summary>
        internal event Action<FixAccount> AccountChanged;

        /// <summary>
        /// New position event
        /// </summary>
        internal event Action<FixPosition> NewPosition;

        /// <summary>
        /// Position changed event
        /// </summary>
        internal event Action<FixPosition> PositionChanged;

        /// <summary>
        /// New order event
        /// </summary>
        internal event Action<FixOrder> NewOrder;

        /// <summary>
        /// Order changed event
        /// </summary>
        internal event Action<FixOrder> OrderChanged;

        /// <summary>
        /// New my trade event
        /// </summary>
        internal event Action<FixMyTrade> NewMyTrade;

        /// <summary>
        /// New security event
        /// </summary>
        internal event Action<FixSecurity> NewSecurity;

        /// <summary>
        /// New Trade event
        /// </summary>
        internal event Action<FixTrade> NewTrade;

        /// <summary>
        /// Trades unsubscribed event
        /// </summary>
        internal event Action<string> TradesUnSubscribed;

        /// <summary>
        /// Market depth changed
        /// </summary>
        internal event Action<FixMarketDepth> MarketDepthChanged;

        /// <summary>
        /// Market depth unsubscribed
        /// </summary>
        internal event Action<string> MarketDepthUnsubscribed;

        /// <summary>
        /// Событие ошибок
        /// </summary>
        internal event Action<FixOrderFail> OrderPlaceFailed;

        /// <summary>
        /// Событие ошибок
        /// </summary>
        internal event Action<FixOrderFail> OrderCancelFailed;

        /// <summary>
        /// Событие ошибок
        /// </summary>
        internal event Action<FixOrderFail> OrderModifyFailed;

        /// <summary>
        /// Настройки сессии
        /// </summary>
        private Model.FixSettings settings;

        /// <summary>
        /// Сессия потока
        /// </summary>
        private Session session;

        /// <summary>
        /// Идентификатор сессии
        /// </summary>
        private SessionID sessionID;

        /// <summary>
        /// Interval to update positions info
        /// </summary>
        private int portfolioUpdateInterval;

        /// <summary>
        /// Interval to update positions info
        /// </summary>
        private int ordersUpdateInterval;

        private Timer portfolioUpdateTimer;
        private Timer ordersUpdateTimer;

        private readonly IDialectClient client;

        public FIXClient(Model.FixSettings settings)
        {
            this.settings = settings;

            FixDialect dialect = this.settings.GetDialect();

            switch(dialect)
            {
                case FixDialect.Exante:
                case FixDialect.Gozo:
                    this.client = new ExanteDialect(settings);
                    break;
                case FixDialect.GainCapital:
                    this.client = new GainCapitalDialect(settings);
                    break;
            }

            this.client.NewPosition += this.newPosition;
            this.client.PositionChanged += this.positionChanged;
            this.client.NewAccount += this.newAccount;
            this.client.AccountChanged += this.accountChanged;
            this.client.NewOrder += this.newOrder;
            this.client.OrderChanged += this.orderChanged;
            this.client.NewMyTrade += this.newMyTrade;
            this.client.NewSecurity += this.newSecurity;
            this.client.NewTrade += this.newTrade;
            this.client.TradesUnSubscribed += this.tradesUnSubscribed;
            this.client.MarketDepthChanged += this.marketDepthChanged;
            this.client.MarketDepthUnsubscribed += this.marketDepthUnsubscribed;
            this.client.OrderPlaceFailed += this.orderPlaceFailed;
        }

        /// <summary>
        /// Освобождаем ресурсы
        /// </summary>
        public void Dispose()
        {
            this.session.Logout("user requested");
        }

        public void FromAdmin(QuickFix.Message message, SessionID sessionID)
        {
            FixDialect dialect = this.settings.GetDialect();

            if (dialect == FixDialect.GainCapital)
            {
                if (message.Header.GetField(Tags.MsgType) == MsgType.LOGON)
                {
                    this.settings.SetProperty("FastHashCode", message.GetString(12004));
                }
            }
        }

        /// <summary>
        /// Input messages execution (Messages router)
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sessionID"></param>
        public void FromApp(QuickFix.Message message, SessionID sessionID)
        {
            var dialect = this.settings.GetDialect();
            if (dialect == FixDialect.Exante || dialect == FixDialect.Gozo)
            {
                MsgType msgType = new MsgType();
                message.Header.GetField(msgType);

                if (msgType.getValue() == new MsgType("UASR").getValue())
                {
                    this.client.ParseAccountSummaryResponse(message);
                    return;
                }
                else if (msgType.getValue() == new MsgType("UASJ").getValue())
                {
                    return;
                }
            }

            try
            {
                Crack(message, sessionID);
            }
            catch (UnsupportedMessageType e)
            {
                throw new UnsupportedMessageType();
            }
        }

        public void OnCreate(SessionID sessionID)
        {
            this.session = Session.LookupSession(sessionID);

            this.client.SetSession(this.session);
        }

        /// <summary>
        /// Событие возникающее при входе
        /// </summary>
        /// <param name="sessionID"></param>
        public void OnLogon(SessionID sessionID)
        {
            this.sessionID = sessionID;

            this.Connected();
        }

        public void OnLogout(SessionID sessionID)
        {
            this.Disconnected();
        }

        /// <summary>
        /// Is connected
        /// </summary>
        /// <returns></returns>
        internal bool IsConnected()
        {
            return this.session != null && this.session.IsLoggedOn;
        }

        /// <summary>
        /// Output messages
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sessionID"></param>
        public void ToAdmin(QuickFix.Message message, SessionID sessionID)
        {
            this.client.PreOutputMessageAction(message);

            FixDialect dialect = this.settings.GetDialect();
            if (dialect == FixDialect.GainCapital)
            {
                if (message.Header.GetField(Tags.MsgType) == MsgType.LOGON)
                {
                    string password = this.settings.GetProperty("Password");

                    if (password != null)
                    {
                        message.SetField(new Password(this.settings.GetProperty("Password")));
                    }

                    string uuid = this.settings.GetProperty("UUID");

                    if (uuid != null)
                    {
                        message.SetField(new UUIDField(uuid));
                    }
                }
            }
        }

        public void ToApp(QuickFix.Message message, SessionID sessionId)
        {
            Console.WriteLine(message);
        }

        /// <summary>
        /// Parse securities list
        /// </summary>
        /// <param name="message">FIX message</param>
        /// <param name="sessionID">Session ID</param>
        public void OnMessage(QuickFix.FIX44.SecurityList message, SessionID sessionID)
        {
            switch (this.settings.GetDialect())
            {
                case FixDialect.GainCapital:
                    this.onGainRequest(message);
                    break;
                case FixDialect.Exante:
                case FixDialect.Gozo:
                    this.client.ParseSecuritiesList(message);
                    break;
            }
        }

        private void onGainRequest(QuickFix.FIX44.SecurityList message)
        {
            //NoRelatedSym numberOfSymbols = new NoRelatedSym();
            //SecurityList.NoRelatedSymGroup securityListGroup = new SecurityList.NoRelatedSymGroup();

            //message.GetField(numberOfSymbols);

            //for (int i = 1; i <= numberOfSymbols.getValue(); i++)
            //{
            //    message.GetGroup(i, securityListGroup);

            //    string baseSymbol = securityListGroup.Symbol.getValue(); // "ES"
            //    string securityExchange = securityListGroup.SecurityExchange.getValue(); // "CME"
            //    string securityDesc = securityListGroup.SecurityDesc.getValue(); // "E-Mini S&P"
            //    decimal contractSize = securityListGroup.Factor.getValue(); // 50
            //    string currency = securityListGroup.Currency.getValue(); // "USD"
            //    string cficode = securityListGroup.CFICode.getValue(); // "FXXXXS"
            //    string securityCode = securityListGroup.GetString(12059); // "ESZ19"
            //    int expirationMonth = securityListGroup.GetInt(12071); // 12
            //    decimal priceStep = securityListGroup.GetDecimal(969); // 0.25
            //    decimal stepPrice = contractSize * priceStep; // 12.5
            //    int digits = securityListGroup.GetInt(12063); // 2
            //    DateTime expireTime = securityListGroup.GetDateTime(126); // 20200320-21:00:00.000

            //    string securityId = securityCode + "@" + securityExchange;

            //    int countMargins = securityListGroup.GetInt(1643);

            //    decimal initialMargin = 0;
            //    decimal liquidatingMargin = 0;

            //    for (int j = 1; j <= countMargins; j++)
            //    {
            //        Group noMarginAmtGroup = securityListGroup.GetGroup(j, 1643);

            //        int marginAmtType = noMarginAmtGroup.GetInt(1644);
            //        decimal marginAmtValue = noMarginAmtGroup.GetDecimal(1645);

            //        switch (marginAmtType)
            //        {
            //            case 11:
            //                initialMargin = marginAmtValue;
            //                break;
            //            case 12:
            //                liquidatingMargin = marginAmtValue;
            //                break;
            //        }
            //    }

            //    Security security = new Security(
            //        securityId
            //    );

            //    security.SetCode(baseSymbol);
            //    security.SetBoard(securityExchange);
            //    security.SetSecurityType(cficode.ToSecurityType());
            //    security.SetCurrency(currency);
            //    security.SetPriceStep(priceStep);
            //    security.SetStepPrice(stepPrice);
            //    security.SetDigits(digits);

            //    if (expireTime != null)
            //    {
            //        security.SetExpiryDate(expireTime);
            //    }

            //    this.securities.Add(securityId, security);

            //    if (this.internalSecurityRequests.Contains("24"))
            //    {

            //    }
            //    this.NewSecurity(security);
            //}
        }

        /// <summary>
        /// Найти инструменты
        /// </summary>
        internal void FindSecurities(SecurityFilter securityFilter)
        {
            this.client.FindSecurities(securityFilter);
        }

        /// <summary>
        /// Load all securities
        /// </summary>
        internal void RequestSecurities()
        {
            this.client.RequestSecurities();
        }

        /// <summary>
        /// Load account summary info
        /// </summary>
        internal void AccountSummaryRequest()
        {
            this.client.AccountSummaryRequest();
        }

        /// <summary>
        /// Load orders info
        /// </summary>
        internal void OrderMassStatusRequest()
        {
            this.client.OrderMassStatusRequest();
        }

        /// <summary>
        /// Parse market data
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sessionID"></param>
        public void OnMessage(QuickFix.FIX44.MarketDataSnapshotFullRefresh message, SessionID sessionID)
        {
            this.client.ParseMarketDataSnapshotFullRefresh(message);
        }

        public void OnMessage(QuickFix.FIX44.MarketDataRequest message, SessionID sessionID)
        {
            Console.WriteLine(message);
        }

        public void OnMessage(QuickFix.FIX44.MarketDataRequestReject message, SessionID sessionID)
        {
            this.client.ParseMarketDataRequestReject(message);
        }

        public void OnMessage(QuickFix.FIX44.OrderCancelReject message, SessionID sessionID)
        {
            switch (this.settings.GetDialect())
            {
                //case Dialect.GainCapital:
                //    this.client.OrderCancelReject(message);
                //    break;
                case FixDialect.Exante:
                case FixDialect.Gozo:
                    this.client.OrderCancelReject(message);
                    break;
            }
        }

        public void OnMessage(QuickFix.FIX44.OrderCancelReplaceRequest message, SessionID sessionID)
        {
            switch (this.settings.GetDialect())
            {
                //case Dialect.GainCapital:
                //    this.OrderCancelRejectForGain(message);
                //    break;
                case FixDialect.Exante:
                case FixDialect.Gozo:
                    this.client.OrderCancelReplaceRequest(message);
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="execution"></param>
        public void OnMessage(ExecutionReport message, SessionID sessionID)
        {
            this.client.ParseOrdersExecutionReport(message);
        }

        public void OnMessage(QuickFix.FIX44.NewOrderSingle message, SessionID sessionID)
        {

        }

        /// <summary>
        /// Subscribe on MarketDepth
        /// </summary>
        /// <param name="securityId">Security</param>
        internal void SubscribeMarketDepth(string securityId)
        {
            this.client.SubscribeMarketDepth(securityId);
        }

        /// <summary>
        /// Отписка от стакана
        /// </summary>
        /// <param name="securityId"></param>
        internal void UnsubscribeMarketDepth(string securityId)
        {
            this.client.SubscribeMarketDepth(securityId);
        }

        /// <summary>
        /// Place new order on the exchange
        /// </summary>
        /// <param name="order">New order</param>
        internal void PlaceOrder(FixOrder order)
        {
            this.client.PlaceOrder(order);
        }

        /// <summary>
        /// Modify order
        /// </summary>
        /// <param name="order">Order <see cref="FixOrder"/></param>
        internal void ModifyOrder(FixOrder order)
        {
            this.client.ModifyOrder(order);
        }

        /// <summary>
        /// Cancel order
        /// </summary>
        /// <param name="order">Order <see cref="FixOrder"/></param>
        internal void CancelOrder(FixOrder order)
        {
            this.client.CancelOrder(order);
        }

        /// <summary>
        /// Subscribe Trades
        /// </summary>
        /// <param name="securityId">Security identifier</param>
        internal void SubscribeTrades(string securityId)
        {
            this.client.SubscribeTrades(securityId);
        }

        /// <summary>
        /// UnSubscribe Trades
        /// </summary>
        /// <param name="securityId">Security identifier</param>
        internal void UnSubscribeTrades(string securityId)
        {
            this.client.UnSubscribeTrades(securityId);
        }

        private void ordersUpdateRequest()
        {
            this.client.OrderMassStatusRequest();
        }

        internal void SubscribePortfolioUpdates()
        {
            if (this.settings.IsTradeStream() == false)
            {
                return;
            }

            this.portfolioUpdateTimer = new Timer(
                new TimerCallback((state) => this.client.AccountSummaryRequest()),
                null,
                0,
                this.settings.GetPortfolioUpdateInterval() * 1000
            );
        }

        internal void SubscribeOrdersUpdates()
        {
            if (this.settings.IsTradeStream() == false)
            {
                return;
            }

            this.ordersUpdateTimer = new Timer(
                new TimerCallback((state) => this.client.OrderMassStatusRequest()),
                null,
                0,
                this.settings.GetOrdersUpdateInterval() * 1000
            );
        }

        private void newPosition(FixPosition position)
        {
            this.NewPosition?.Invoke(position);
        }

        private void positionChanged(FixPosition position)
        {
            this.PositionChanged?.Invoke(position);
        }

        private void newAccount(FixAccount portfolio)
        {
            this.NewAccount?.Invoke(portfolio);
        }

        private void accountChanged(FixAccount portfolio)
        {
            this.AccountChanged?.Invoke(portfolio);
        }

        private void newOrder(FixOrder order)
        {
            this.NewOrder?.Invoke(order);
        }

        private void orderChanged(FixOrder order)
        {
            this.OrderChanged?.Invoke(order);
        }

        private void newMyTrade(FixMyTrade myTrade)
        {
            this.NewMyTrade?.Invoke(myTrade);
        }

        private void newSecurity(FixSecurity security)
        {
            this.NewSecurity?.Invoke(security);
        }

        private void newTrade(FixTrade trade)
        {
            this.NewTrade?.Invoke(trade);
        }

        private void tradesUnSubscribed(string securityId)
        {
            this.TradesUnSubscribed?.Invoke(securityId);
        }

        private void marketDepthChanged(FixMarketDepth marketDepth)
        {
            this.MarketDepthChanged?.Invoke(marketDepth);
        }

        private void marketDepthUnsubscribed(string securityId)
        {
            this.MarketDepthUnsubscribed?.Invoke(securityId);
        }

        private void orderPlaceFailed(FixOrderFail orderFail)
        {
            this.OrderPlaceFailed.Invoke(orderFail);
        }
    }
}
