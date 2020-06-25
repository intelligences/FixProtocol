using ExecutionReport = QuickFix.FIX44.ExecutionReport;
using Intelligences.FixProtocol.Client.Dialects;
using Intelligences.FixProtocol.Enum;
using Intelligences.FixProtocol.Fields;
using Intelligences.FixProtocol.Filter;
using Intelligences.FixProtocol.Model;
using MarketDepth = Intelligences.FixProtocol.Model.MarketDepth;
using QuickFix;
using QuickFix.Fields;
using System;
using System.Threading;
using Tags = QuickFix.Fields.Tags;
using System.Threading.Tasks;
using Intelligences.FixProtocol.DTO;

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
        /// New portfolio event
        /// </summary>
        internal event Action<Portfolio> NewPortfolio;

        /// <summary>
        /// Portfolio changed event
        /// </summary>
        internal event Action<Portfolio> PortfolioChanged;

        /// <summary>
        /// New position event
        /// </summary>
        internal event Action<Position> NewPosition;

        /// <summary>
        /// Position changed event
        /// </summary>
        internal event Action<Position> PositionChanged;

        /// <summary>
        /// New order event
        /// </summary>
        internal event Action<Order> NewOrder;

        /// <summary>
        /// Order changed event
        /// </summary>
        internal event Action<Order> OrderChanged;

        /// <summary>
        /// New my trade event
        /// </summary>
        internal event Action<MyTrade> NewMyTrade;

        /// <summary>
        /// New security event
        /// </summary>
        internal event Action<Security> NewSecurity;

        /// <summary>
        /// New Trade event
        /// </summary>
        internal event Action<Trade> NewTrade;

        /// <summary>
        /// Trades unsubscribed event
        /// </summary>
        internal event Action<Security> TradesUnSubscribed;

        /// <summary>
        /// Market depth changed
        /// </summary>
        internal event Action<MarketDepth> MarketDepthChanged;

        /// <summary>
        /// Market depth unsubscribed
        /// </summary>
        internal event Action<Security> MarketDepthUnsubscribed;

        /// <summary>
        /// Событие ошибок
        /// </summary>
        internal event Action<Exception> OrderCancelFailed;

        /// <summary>
        /// Событие ошибок
        /// </summary>
        internal event Action<Exception> OrderModifyFailed;

        /// <summary>
        /// Настройки сессии
        /// </summary>
        private Model.Settings settings;

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

        public FIXClient(Model.Settings settings)
        {
            this.settings = settings;

            Dialect dialect = this.settings.GetDialect();

            switch(dialect)
            {
                case Dialect.Exante:
                    this.client = new ExanteDialect(settings);
                    break;
                case Dialect.GainCapital:
                    this.client = new GainCapitalDialect(settings);
                    break;
            }

            this.client.NewPosition += this.newPosition;
            this.client.PositionChanged += this.positionChanged;
            this.client.NewPortfolio += this.newPortfolio;
            this.client.PortfolioChanged += this.portfolioChanged;
            this.client.NewOrder += this.newOrder;
            this.client.OrderChanged += this.orderChanged;
            this.client.NewMyTrade += this.newMyTrade;
            this.client.NewSecurity += this.newSecurity;
            this.client.NewTrade += this.newTrade;
            this.client.TradesUnSubscribed += this.tradesUnSubscribed;
            this.client.MarketDepthChanged += this.marketDepthChanged;
            this.client.MarketDepthUnsubscribed += this.marketDepthUnsubscribed;
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
            Dialect dialect = this.settings.GetDialect();

            if (dialect == Dialect.GainCapital)
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
            if (this.settings.GetDialect() == Dialect.Exante)
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

        internal void CreateSecurity(SecurityData securityData)
        {
            this.client.CreateSecurity(securityData);
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

            Dialect dialect = this.settings.GetDialect();
            if (dialect == Dialect.GainCapital)
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
            string status = message.GetField(560);

            if (status != "0")
            {
                return;
            }

            this.client.ParseSecuritiesList(message);

            switch (this.settings.GetDialect())
            {
                case Dialect.GainCapital:
                    this.onGainRequest(message);
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
            this.findSecuritiesRequest(securityFilter);
        }

        /// <summary>
        /// Load all securities
        /// </summary>
        internal void LoadAllSecurities()
        {
            this.findSecuritiesRequest(new SecurityFilter());
        }

        private void findSecuritiesRequest(SecurityFilter securityFilter)
        {
            this.client.FindSecurities(securityFilter);
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
            OrdStatus ordStatus = new OrdStatus();
            message.GetField(ordStatus);

            switch(ordStatus.ToOrderState())
            {
                case OrderState.Filled:
                    this.OrderCancelFailed(new Exception("Order already filled"));
                    break;
                case OrderState.Canceled:
                    this.OrderCancelFailed(new Exception("Order already canceled"));
                    break;
            }
        }

        public void OnMessage(QuickFix.FIX44.OrderCancelReplaceRequest message, SessionID sessionID)
        {
            OrdStatus ordStatus = new OrdStatus();
            message.GetField(ordStatus);

            switch (ordStatus.ToOrderState())
            {
                case OrderState.Filled:
                    this.OrderModifyFailed(new Exception("Order already filled"));
                    break;
                case OrderState.Canceled:
                    this.OrderModifyFailed(new Exception("Order already canceled"));
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
        /// <param name="security">Security</param>
        internal void SubscribeMarketDepth(Security security)
        {
            this.client.SubscribeMarketDepth(security);
        }

        /// <summary>
        /// Отписка от стакана
        /// </summary>
        /// <param name="security"></param>
        internal void UnsubscribeMarketDepth(Security security)
        {
            this.client.SubscribeMarketDepth(security);
        }

        /// <summary>
        /// Place new order on the exchange
        /// </summary>
        /// <param name="order">New order</param>
        internal void PlaceOrder(Order order)
        {
            this.client.PlaceOrder(order);
        }

        /// <summary>
        /// Modify order
        /// </summary>
        /// <param name="order">Order <see cref="Order"/></param>
        internal void ModifyOrder(Order order)
        {
            this.client.ModifyOrder(order);
        }

        /// <summary>
        /// Cancel order
        /// </summary>
        /// <param name="order">Order <see cref="Order"/></param>
        internal void CancelOrder(Order order)
        {
            this.client.CancelOrder(order);
        }

        /// <summary>
        /// Subscribe Trades
        /// </summary>
        /// <param name="security">Security <see cref="Security"/></param>
        internal void SubscribeTrades(Security security)
        {
            this.client.SubscribeTrades(security);
        }

        /// <summary>
        /// UnSubscribe Trades
        /// </summary>
        /// <param name="security">Security <see cref="Security"/></param>
        internal void UnSubscribeTrades(Security security)
        {
            this.client.UnSubscribeTrades(security);
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

        private void newPosition(Position position)
        {
            this.NewPosition?.Invoke(position);
        }

        private void positionChanged(Position position)
        {
            this.PositionChanged?.Invoke(position);
        }

        private void newPortfolio(Portfolio portfolio)
        {
            this.NewPortfolio?.Invoke(portfolio);
        }

        private void portfolioChanged(Portfolio portfolio)
        {
            this.PortfolioChanged?.Invoke(portfolio);
        }

        private void newOrder(Order order)
        {
            this.NewOrder?.Invoke(order);
        }

        private void orderChanged(Order order)
        {
            this.OrderChanged?.Invoke(order);
        }

        private void newMyTrade(MyTrade myTrade)
        {
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(1500);
                this.client.AccountSummaryRequest();
            });

            this.NewMyTrade?.Invoke(myTrade);
        }

        private void newSecurity(Security security)
        {
            this.NewSecurity?.Invoke(security);
        }

        private void newTrade(Trade trade)
        {
            this.NewTrade?.Invoke(trade);
        }

        private void tradesUnSubscribed(Security security)
        {
            this.TradesUnSubscribed?.Invoke(security);
        }

        private void marketDepthChanged(MarketDepth marketDepth)
        {
            this.MarketDepthChanged?.Invoke(marketDepth);
        }

        private void marketDepthUnsubscribed(Security security)
        {
            this.MarketDepthUnsubscribed?.Invoke(security);
        }
    }
}
