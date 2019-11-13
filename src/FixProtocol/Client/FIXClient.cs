using Intelligences.FixProtocol.Enum;
using Intelligences.FixProtocol.Fields;
using Intelligences.FixProtocol.Filter;
using Intelligences.FixProtocol.Model;
using Intelligences.FixProtocol.Model.ConditionalOrders;
using QuickFix;
using QuickFix.Fields;
using QuickFix.FIX44;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using SecurityType = Intelligences.FixProtocol.Enum.SecurityType;
using Tags = QuickFix.Fields.Tags;
using TimeInForce = QuickFix.Fields.TimeInForce;

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
        /// Событие ошибок
        /// </summary>
        internal event Action<Exception> OrderCancelFailed;

        /// <summary>
        /// Событие ошибок
        /// </summary>
        internal event Action<Exception> OrderModifyFailed;

        /// <summary>
        /// Событие изменения стакана
        /// </summary>
        internal event Action<Model.MarketDepth> MarketDepthChanged;

        internal event Action<Model.Security> MarketDepthUnsubscribed;

        /// <summary>
        /// Событие получения новой сделки
        /// </summary>
        internal event Action<Model.Trade> NewTrade;

        /// <summary>
        /// Событие получения нового инструмента
        /// </summary>
        internal event Action<Security> NewSecurity;

        internal event Action<Portfolio> NewPortfolio;
        internal event Action<Portfolio> PortfolioChanged;

        internal event Action<Position> NewPosition;
        internal event Action<Position> PositionChanged;

        internal event Action<Order> NewOrder;
        internal event Action<Order> OrderChanged;

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

        private readonly Dictionary<string, Model.MarketDepth> marketDepths = new Dictionary<string, Model.MarketDepth>();
        private readonly Dictionary<string, Portfolio> portfolios = new Dictionary<string, Portfolio>();
        private readonly Dictionary<string, int> loadedPositionsCount = new Dictionary<string, int>();
        private readonly Dictionary<string, bool> isNewPortfolioInitialized = new Dictionary<string, bool>();
        private readonly Dictionary<string, Order> pendingOrders = new Dictionary<string, Order>();
        private readonly Dictionary<string, Order> orders = new Dictionary<string, Order>();

        //private readonly Dictionary<string, Model.MarketDepth> marketDepths = new Dictionary<string, Model.MarketDepth>();

        /// <summary>
        /// Список инструментов
        /// </summary>
        private readonly Dictionary<string, Security> securities = new Dictionary<string, Security>();

        private readonly string accountRequestId;

        private Timer portfolioUpdateTimer;
        private Timer ordersUpdateTimer;
        private bool securityEventsAllowed = false;

        public FIXClient(Model.Settings settings)
        {
            this.settings = settings;
            this.accountRequestId = Guid.NewGuid().ToString();
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
        /// Входящие сообещения
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
                    this.parseAccountSummaryResponse(message);
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

        /// <summary>
        /// Событие возникающее при создании нового подключения
        /// </summary>
        /// <param name="sessionID"></param>
        public void OnCreate(SessionID sessionID)
        {
            this.session = Session.LookupSession(sessionID);
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
        /// Исходящие сообщения
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sessionID"></param>
        public void ToAdmin(QuickFix.Message message, SessionID sessionID)
        {
            Dialect dialect = this.settings.GetDialect();

            if (message.Header.GetField(Tags.MsgType) == MsgType.LOGON)
            {
                string password = this.settings.GetProperty("Password");
                
                if (password != null)
                {
                    message.SetField(new Password(this.settings.GetProperty("Password")));
                }

                if (dialect == Dialect.GainCapital)
                {
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
        }

        /// <summary>
        /// Поллучение списка инструментов
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sessionID"></param>
        public void OnMessage(QuickFix.FIX44.SecurityList message, SessionID sessionID)
        {
            string status = message.GetField(560);

            if (status != "0")
            {
                return;
            }

            switch(this.settings.GetDialect())
            {
                case Dialect.Exante:
                    this.onExanteRequest(message);
                    break;
                case Dialect.GainCapital:
                    this.onGainRequest(message);
                    break;
            }
        }

        private void onGainRequest(QuickFix.FIX44.SecurityList message)
        {
            NoRelatedSym numberOfSymbols = new NoRelatedSym();
            SecurityList.NoRelatedSymGroup securityListGroup = new SecurityList.NoRelatedSymGroup();

            message.GetField(numberOfSymbols);

            for (int i = 1; i <= numberOfSymbols.getValue(); i++)
            {
                message.GetGroup(i, securityListGroup);

                string baseSymbol = securityListGroup.Symbol.getValue(); // "ES"
                string securityExchange = securityListGroup.SecurityExchange.getValue(); // "CME"
                string securityDesc = securityListGroup.SecurityDesc.getValue(); // "E-Mini S&P"
                decimal contractSize = securityListGroup.Factor.getValue(); // 50
                string currency = securityListGroup.Currency.getValue(); // "USD"
                string cficode = securityListGroup.CFICode.getValue(); // "FXXXXS"
                string securityCode = securityListGroup.GetString(12059); // "ESZ19"
                int expirationMonth = securityListGroup.GetInt(12071); // 12
                decimal priceStep = securityListGroup.GetDecimal(969); // 0.25
                decimal stepPrice = contractSize * priceStep; // 12.5
                int digits = securityListGroup.GetInt(12063); // 2
                DateTime expireTime = securityListGroup.GetDateTime(126); // 20200320-21:00:00.000

                string securityId = securityCode + "@" + securityExchange;

                int countMargins = securityListGroup.GetInt(1643);

                decimal initialMargin = 0;
                decimal liquidatingMargin = 0;

                for (int j = 1; j <= countMargins; j++)
                {
                    Group noMarginAmtGroup = securityListGroup.GetGroup(j, 1643);

                    int marginAmtType = noMarginAmtGroup.GetInt(1644);
                    decimal marginAmtValue = noMarginAmtGroup.GetDecimal(1645);

                    switch (marginAmtType)
                    {
                        case 11:
                            initialMargin = marginAmtValue;
                            break;
                        case 12:
                            liquidatingMargin = marginAmtValue;
                            break;
                    }
                }

                Security security = new Security(
                    securityId,
                    baseSymbol,
                    securityExchange,
                    cficode.ToSecurityType(),
                    currency,
                    priceStep,
                    stepPrice,
                    digits
                );

                if (expireTime != null)
                {
                    security.SetExpiryDate(expireTime);
                }

                this.securities.Add(securityId, security);

                this.NewSecurity(security);
            }
        }

        private void onExanteRequest(QuickFix.FIX44.SecurityList message)
        {
            NoRelatedSym numberOfSymbols = new NoRelatedSym();
            SecurityList.NoRelatedSymGroup securityListGroup = new SecurityList.NoRelatedSymGroup();

            message.GetField(numberOfSymbols);

            for (int i = 1; i <= numberOfSymbols.getValue(); i++)
            {
                message.GetGroup(i, securityListGroup);

                string securityId = securityListGroup.SecurityID.getValue(); // "ES.CME.H2020"

                if (!this.securities.ContainsKey(securityId))
                {
                    string symbol = securityListGroup.Symbol.getValue(); // "ES"
                    string securityExchange = securityListGroup.SecurityExchange.getValue(); // "CME"
                    string currency = securityListGroup.Currency.getValue(); // "USD"
                    string cficode = securityListGroup.CFICode.getValue(); // "FXXXXX"
                    decimal contractSize = securityListGroup.ContractMultiplier.getValue(); // 50M
                    decimal priceStep = 0; // 0.25M
                    decimal minOrderPrice = 0; // 0.25M
                    decimal lotSize = 0; // 1.0M
                    DateTime? expiryDate = null; // {20.03.2020 0:00:00}
                    decimal initialMargin = 0; // 6300.0M
                    decimal maintenanceMargin = 0; // 6300.0M

                    int attributesCount = securityListGroup.NoInstrAttrib.getValue();
                    AllocationInstruction.NoInstrAttribGroup attributesGroup = new AllocationInstruction.NoInstrAttribGroup();

                    for (int j = 1; j <= attributesCount; j++)
                    {
                        securityListGroup.GetGroup(j, attributesGroup);

                        int attrKey = attributesGroup.InstrAttribType.getValue();
                        string attrValue = attributesGroup.InstrAttribValue.getValue();

                        switch (attrKey)
                        {
                            case 500:
                                priceStep = decimal.Parse(attrValue, CultureInfo.InvariantCulture);
                                break;
                            case 501:
                                initialMargin = decimal.Parse(attrValue, CultureInfo.InvariantCulture);
                                break;
                            case 502:
                                maintenanceMargin = decimal.Parse(attrValue, CultureInfo.InvariantCulture);
                                break;
                            case 503:
                                minOrderPrice = decimal.Parse(attrValue, CultureInfo.InvariantCulture);
                                break;
                            case 504:
                                lotSize = decimal.Parse(attrValue, CultureInfo.InvariantCulture);
                                break;
                            case 505:
                                expiryDate = DateTime.ParseExact(attrValue,
                                    "yyyyMd",
                                    CultureInfo.InvariantCulture,
                                    DateTimeStyles.AssumeLocal
                                );
                                break;
                        }
                    }
                    
                    decimal stepPrice = contractSize * priceStep; // 12.5
                    int digits = priceStep.PriceStepToNumderOfDigits();

                    Security security = new Security(
                        securityId, // "ES.CME.H2020"
                        symbol, // "ES"
                        securityExchange, // "CME"
                        cficode.ToSecurityType(), // "FXXXXX" | security type
                        currency, // "USD"
                        priceStep, // 0.25M
                        stepPrice,
                        digits // 2
                    );

                    if (expiryDate != null)
                    {
                        security.SetExpiryDate(expiryDate);
                    }

                    this.securities.Add(securityId, security);
                }

                if (this.securities.TryGetValue(securityId, out Security output))
                {
                    this.NewSecurity(output);
                }
            }
        }

        /// <summary>
        /// Найти инструменты
        /// </summary>
        internal void FindSecurities(SecurityFilter securityFilter)
        {
            int requestType = SecurityListRequestType.ALL_SECURITIES;
            string securityCode = securityFilter.Code;
            SecurityType? securityType = securityFilter.Type;

            if (securityCode != null)
            {
                requestType = SecurityListRequestType.SYMBOL;
            }

            if (securityType != null)
            {
                requestType = SecurityListRequestType.SECURITYTYPE_AND_OR_CFICODE;
            }

            QuickFix.FIX44.SecurityListRequest message = new QuickFix.FIX44.SecurityListRequest(
                new SecurityReqID(Guid.NewGuid().ToString()),
                new SecurityListRequestType(requestType)
            );

            if (securityCode != null)
            {
                message.Symbol = new Symbol(securityCode);
            }

            if (securityType != null)
            {
                message.CFICode = new CFICode(securityType.ToCFICode());
            }

            Session.SendToTarget(message, sessionID);
        }

        /// <summary>
        /// Получение маркет данных
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sessionID"></param>
        public void OnMessage(QuickFix.FIX44.MarketDataSnapshotFullRefresh message, SessionID sessionID)
        {
            String mdKey = message.MDReqID.ToString();

            if (!this.marketDepths.ContainsKey(mdKey))
            {
                return;
            }

            Model.MarketDepth marketDepth = this.marketDepths[mdKey];

            int noMDEntries = message.NoMDEntries.getValue();
            QuickFix.FIX44.MarketDataSnapshotFullRefresh.NoMDEntriesGroup group = new QuickFix.FIX44.MarketDataSnapshotFullRefresh.NoMDEntriesGroup();
            MDEntryType mdEntryType = new MDEntryType();
            MDEntryPx mdEntryPx = new MDEntryPx();
            MDEntrySize mDEntrySize = new MDEntrySize();

            Dictionary<decimal, decimal> bids = new Dictionary<decimal, decimal>();
            Dictionary<decimal, decimal> asks = new Dictionary<decimal, decimal>();

            for (int i = 1; i <= noMDEntries; i++)
            {
                message.GetGroup(i, group);
                group.Get(mdEntryType);

                try
                {
                    group.Get(mdEntryPx);
                }
                catch (Exception e) { }

                try {
                    group.Get(mDEntrySize);
                } catch(Exception e) {  }

                switch (mdEntryType.getValue())
                {
                    case MDEntryType.BID:
                        decimal bid = mdEntryPx.getValue();
                        decimal bidVolume = mDEntrySize.getValue();
                        bids[bid] = bidVolume;
                        break;
                    case MDEntryType.OFFER:
                        decimal ask = mdEntryPx.getValue();
                        decimal askVolume = mDEntrySize.getValue();
                        asks[ask] = askVolume;
                        break;
                    case MDEntryType.TRADE:
                        this.NewTrade(new Trade(marketDepth.GetSecurity(), mdEntryPx.getValue(), mDEntrySize.getValue()));
                        break;
                    //case MDEntryType.OPENING_PRICE:
                    //    Debug.WriteLine(mdEntryPx.getValue());
                    //    break;
                    //case MDEntryType.CLOSING_PRICE:
                    //    Debug.WriteLine(mdEntryPx.getValue());
                    //    break;
                    //case MDEntryType.TRADE_VOLUME:
                    //    Debug.WriteLine(mDEntrySize.getValue());
                    //    break;
                    //case MDEntryType.EMPTY_BOOK:
                    //    Debug.WriteLine(mdEntryPx.getValue());
                    //    break;
                    //case 'x':
                    //    Debug.WriteLine(mdEntryPx.getValue());
                    //    break;
                    //case 'y':
                    //    Debug.WriteLine(mdEntryPx.getValue());
                    //    break;
                    //case 'z':
                    //    Debug.WriteLine(mdEntryPx.getValue());
                    //    break;
                }
            }

            marketDepth.SetAsks(asks);
            marketDepth.SetBids(bids);

            if (asks.Count > 0 && bids.Count > 0)
            {
                this.MarketDepthChanged(marketDepth);
            }
        }

        public void OnMessage(QuickFix.FIX44.MarketDataRequest message, SessionID sessionID)
        {
            Console.WriteLine(message);
        }

        /// <summary>
        /// Сообщение об отклоненном запросе маркет данных
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sessionID"></param>
        public void OnMessage(QuickFix.FIX44.MarketDataRequestReject message, SessionID sessionID)
        {
            string securityId = message.MDReqID.ToString();

            if (this.marketDepths.ContainsKey(securityId))
            {
                this.marketDepths.Remove(securityId);

                if (this.securities.TryGetValue(securityId, out Security output))
                {
                    this.MarketDepthUnsubscribed(output);
                }
            }
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
        public void OnMessage(QuickFix.FIX44.ExecutionReport message, SessionID sessionID)
        {
            OrderID orderID = new OrderID();
            message.GetField(orderID);

            string orderId = orderID.getValue();

            if (orderId != "NONE")
            {
                Order order;

                ClOrdID clOrdID = new ClOrdID();
                OrderQty orderQty = new OrderQty();
                Side side = new Side();
                Account account = new Account();
                SecurityID securityID = new SecurityID();
                OrdStatus ordStatus = new OrdStatus();
                OrdType ordType = new OrdType();
                Price priceField = new Price();

                message.GetField(clOrdID);
                message.GetField(orderQty);
                message.GetField(side);
                message.GetField(account);
                message.GetField(securityID);
                message.GetField(ordStatus);
                message.GetField(ordType);

                try
                {
                    message.GetField(priceField);
                } catch (Exception e){}
                
                string clientOrderid = clOrdID.getValue();
                string accountName = account.getValue();
                string securityId = securityID.getValue();
                OrderType orderType = ordType.ToOrderType();
                OrderState orderState = ordStatus.ToOrderState();
                decimal quantity = orderQty.getValue();
                decimal price = priceField.getValue();

                if (!this.portfolios.TryGetValue(accountName, out Portfolio portfolio))
                {
                    return;
                }

                if (!this.securities.TryGetValue(securityId, out Security security))
                {
                    //this.FindSecurities(new SecurityFilter() {
                    //    Code = "ES"
                    //});

                    return;
                }

                order = this.findOrder(clientOrderid, orderId);

                DateTime dateTime = message.Header.GetDateTime(52);

                if (order == null)
                {
                    if (orderType == OrderType.Market)
                    {
                        order = new MarketOrder(
                            quantity,
                            side.ToDirection(),
                            portfolio,
                            security
                        );
                    }
                    else
                    {
                        order = new LimitOrder(
                            price,
                            quantity,
                            side.ToDirection(),
                            portfolio,
                            security
                        );
                    }

                    order.SetTransactionId(Guid.NewGuid().ToString());
                    order.SetClientOrderId(clientOrderid);
                    order.SetState(OrderState.Pending);
                }

                order.SetOrderId(orderId);
                order.SetQuantity(quantity);
                order.SetPrice(price);
                order.SetClientOrderId(clientOrderid);

                if (!this.orders.ContainsKey(orderId))
                {
                    this.orders.Add(orderId, order);

                    order.SetState(orderState);
                    order.SetCreatedAt(dateTime);

                    this.NewOrder(order);
                }
                else
                {
                    order.SetState(orderState);
                    order.SetUpdatedAt(DateTimeOffset.UtcNow);

                    this.OrderChanged(order);
                }
            }
        }

        private Order findOrder(string clientOrderId, string orderId)
        {
            Order order;

            if (this.pendingOrders.TryGetValue(clientOrderId, out order))
            {
                this.pendingOrders.Remove(clientOrderId);

                return order;
            }

            if (this.orders.TryGetValue(orderId, out order))
            {
                return order;
            }

            return null;
        }

        public void OnMessage(QuickFix.FIX44.NewOrderSingle ord, SessionID sessionID)
        {

        }

        /// <summary>
        /// Подписаться на стакан
        /// </summary>
        /// <param name="security"></param>
        internal void SubscribeMarketDepth(Security security)
        {
            string securityId = security.GetId();

            if (this.marketDepths.ContainsKey(securityId))
            {
                return;
            }

            QuickFix.FIX44.MarketDataRequest marketDataRequest = new QuickFix.FIX44.MarketDataRequest(
                new MDReqID(securityId),
                new SubscriptionRequestType(SubscriptionRequestType.SNAPSHOT_PLUS_UPDATES),
                new QuickFix.Fields.MarketDepth()
            );

            marketDataRequest.Set(new MDUpdateType(MDUpdateType.FULL_REFRESH));
            marketDataRequest.Set(new AggregatedBook(true));

            QuickFix.FIX44.MarketDataRequest.NoMDEntryTypesGroup typesGroup = new QuickFix.FIX44.MarketDataRequest.NoMDEntryTypesGroup();
            typesGroup.Set(new MDEntryType(MDEntryType.BID));
            marketDataRequest.AddGroup(typesGroup);
            typesGroup.Set(new MDEntryType(MDEntryType.OFFER));
            marketDataRequest.AddGroup(typesGroup);
            typesGroup.Set(new MDEntryType(MDEntryType.TRADE));
            marketDataRequest.AddGroup(typesGroup);
            //typesGroup.Set(new MDEntryType(MDEntryType.OPENING_PRICE));
            //marketDataRequest.AddGroup(typesGroup);
            //typesGroup.Set(new MDEntryType(MDEntryType.CLOSING_PRICE));
            //marketDataRequest.AddGroup(typesGroup);
            //typesGroup.Set(new MDEntryType(MDEntryType.TRADE_VOLUME));
            //marketDataRequest.AddGroup(typesGroup);
            //typesGroup.Set(new MDEntryType(MDEntryType.EMPTY_BOOK));
            //marketDataRequest.AddGroup(typesGroup);
            //typesGroup.Set(new MDEntryType('x')); // Price limit low
            //marketDataRequest.AddGroup(typesGroup);
            //typesGroup.Set(new MDEntryType('y')); // Price limit high
            //marketDataRequest.AddGroup(typesGroup);
            //No permissions for option data
            //typesGroup.Set(new MDEntryType('z')); // Option data 
            //marketDataRequest.AddGroup(typesGroup);

            QuickFix.FIX44.MarketDataRequest.NoRelatedSymGroup symGroup = new QuickFix.FIX44.MarketDataRequest.NoRelatedSymGroup();
            symGroup.Set(new SecurityIDSource("111"));
            symGroup.Set(new SecurityID(securityId));
            symGroup.Set(new Symbol(securityId));

            marketDataRequest.AddGroup(symGroup);

            this.marketDepths.Add(securityId, new Model.MarketDepth(security, 1));

            Session.SendToTarget(marketDataRequest, this.sessionID);
        }

        /// <summary>
        /// Отписка от стакана
        /// </summary>
        /// <param name="security"></param>
        internal void UnsubscribeMarketDepth(Security security)
        {
            string securityId = security.GetId();

            if (!this.marketDepths.ContainsKey(securityId))
            {
                return;
            }

            QuickFix.FIX44.MarketDataRequest marketDataRequest = new QuickFix.FIX44.MarketDataRequest(
                new MDReqID(securityId),
                new SubscriptionRequestType(SubscriptionRequestType.DISABLE_PREVIOUS),
                new QuickFix.Fields.MarketDepth()
            );

            QuickFix.FIX44.MarketDataRequest.NoMDEntryTypesGroup typesGroup = new QuickFix.FIX44.MarketDataRequest.NoMDEntryTypesGroup();
            typesGroup.Set(new MDEntryType(MDEntryType.BID));
            marketDataRequest.AddGroup(typesGroup);
            typesGroup.Set(new MDEntryType(MDEntryType.OFFER));
            marketDataRequest.AddGroup(typesGroup);
            typesGroup.Set(new MDEntryType(MDEntryType.TRADE));
            marketDataRequest.AddGroup(typesGroup);

            QuickFix.FIX44.MarketDataRequest.NoRelatedSymGroup symGroup = new QuickFix.FIX44.MarketDataRequest.NoRelatedSymGroup();
            symGroup.Set(new Symbol(securityId));

            marketDataRequest.AddGroup(symGroup);

            Session.SendToTarget(marketDataRequest, sessionID);
        }

        internal void PlaceOrder(Order order)
        {
            if (order is null)
            {
                throw new ArgumentNullException("Order can't be null");
            }

            Security security = order.GetSecurity();
            string clientOrderId = order.GetClientOrderId();

            NewOrderSingle newFixOrder = new NewOrderSingle(
                new ClOrdID(clientOrderId),
                new Symbol(security.GetId()),
                new Side(order.GetDirection().ToFixOrderSide()),
                new TransactTime(order.GetCreatedAt().DateTime),
                new OrdType(order.GetOrderType().ToFixOrderType())
            );

            this.FillOrder(newFixOrder, order);

            this.pendingOrders.Add(clientOrderId, order);

            Session.SendToTarget(newFixOrder, this.sessionID);
        }

        internal void ModifyOrder(Order order)
        {
            if (order is null)
            {
                throw new ArgumentNullException("Order can't be null");
            }

            string clientOrderId = order.GetClientOrderId();
            Security security = order.GetSecurity();
            string securityId = security.GetId();

            QuickFix.FIX44.OrderCancelReplaceRequest request = new QuickFix.FIX44.OrderCancelReplaceRequest(
                new OrigClOrdID(clientOrderId),
                new ClOrdID(Guid.NewGuid().ToString()),
                new Symbol(securityId),
                new Side(order.GetDirection().ToFixOrderSide()),
                new TransactTime(order.GetCreatedAt().DateTime),
                new OrdType(order.GetOrderType().ToFixOrderType())
            );

            FillOrder(request, order);

            QuickFix.Session.SendToTarget(request, sessionID);
        }

        private void FillOrder(QuickFix.FIX44.Message newFixOrder, Order order)
        {
            Security security = order.GetSecurity();
            string securityId = security.GetId();
            Portfolio portfolio = order.GetPortfolio();

            if (typeof(NewOrderSingle) == newFixOrder.GetType())
            {
                newFixOrder.SetField(new Account(portfolio.GetName()));
                newFixOrder.SetField(new OrdType(order.GetOrderType().ToFixOrderType()));
            }

            newFixOrder.SetField(new OrderQty(order.GetQuantity()));

            newFixOrder.SetField(new SecurityIDSource("111")); // TODO exante only
            newFixOrder.SetField(new SecurityID(securityId));

            if (order.GetPrice() != 0)
            {
                newFixOrder.SetField(new Price(order.GetPrice()));
            }

            if (order.GetOrderType() == OrderType.Limit)
            {
                newFixOrder.SetField(new Price(order.GetPrice()));
            }
            else if (order.GetOrderType() == OrderType.Conditional)
            {
                IOrderCondition condition = order.GetCondition();

                if (condition.GetType() == typeof(StopLimit))
                {
                    StopLimit stopLimitCondition = (StopLimit)condition;
                    newFixOrder.SetField(new StopPx(stopLimitCondition.GetPrice()));
                }
            }

            newFixOrder.SetField(new TimeInForce(order.GetTimeInForce().ToFixTimeInForce())); ;
        }

        internal void CancelOrder(Order order)
        {
            string clientOrderId = order.GetClientOrderId();
            Security security = order.GetSecurity();
            string securityId = security.GetId();

            OrderCancelRequest cancelOrder = new OrderCancelRequest(
                new OrigClOrdID(clientOrderId),
                new ClOrdID(Guid.NewGuid().ToString()),
                new Symbol(securityId),
                new Side(order.GetDirection().ToFixOrderSide()),
                new TransactTime(order.GetCreatedAt().DateTime)
            );

            cancelOrder.SetField(new SecurityIDSource("111")); // TODO exante only
            cancelOrder.SetField(new SecurityID(securityId));
            cancelOrder.SetField(new OrderQty(order.GetQuantity()));

            Session.SendToTarget(cancelOrder, sessionID);
        }

        private void requestPortfolio()
        {
            if (!this.IsConnected())
            {
                return;
            }

            if (this.settings.GetDialect() == Dialect.Exante)
            {
                QuickFix.Message message = new QuickFix.Message();
                message.Header.SetField(new MsgType("UASQ"));

                message.SetField(new AccSumReqID(this.accountRequestId));

                Session.SendToTarget(message, sessionID);
            }
        }

        private void ordersUpdateRequest()
        {
            if (!this.IsConnected())
            {
                return;
            }

            OrderMassStatusRequest message = new OrderMassStatusRequest(
                new MassStatusReqID(Guid.NewGuid().ToString()),
                new MassStatusReqType(MassStatusReqType.STATUS_FOR_ALL_ORDERS)
            );

            Session.SendToTarget(message, sessionID);
        }

        /// <summary>
        /// Как только ордер будет изменен, кидаем его в событие
        /// </summary>
        private void onOrderChanged()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        private void parseAccountSummaryResponse(QuickFix.Message message)
        {
            if (this.settings.GetDialect() == Dialect.Exante)
            {
                SecurityID securityIdField = new SecurityID();
                LongQty longQtyField = new LongQty();
                ShortQty shortQtyField = new ShortQty();
                SecurityExchange securityExchangeField = new SecurityExchange();
                Account accountField = new Account();
                Symbol symbolField = new Symbol();
                CFICode cfiCodeField = new CFICode();
                TotalNetValue totalNetValueField = new TotalNetValue();

                decimal profitAndLoss = 0;

                string securityExchange = null;

                try
                {
                    securityExchange = message.GetField(securityExchangeField).getValue();
                }
                catch (Exception e) { }

                if (!String.IsNullOrEmpty(securityExchange))
                {
                    profitAndLoss = message.GetDecimal(20030);
                }

                int positionsCount = message.GetInt(20021);
                string currency = message.GetString(20023);
                decimal positionValue = message.GetDecimal(20032);
                decimal usedMargin = message.GetDecimal(20040);


                message.GetField(securityIdField);
                message.GetField(longQtyField);
                message.GetField(shortQtyField);
                message.GetField(accountField);
                message.GetField(symbolField);
                message.GetField(cfiCodeField);
                message.GetField(totalNetValueField);

                string account = accountField.getValue();
                decimal totalNetValue = totalNetValueField.getValue();
                string securityId = securityIdField.getValue();
                string symbol = symbolField.getValue();
                string cfiCode = cfiCodeField.getValue();

                if (!this.portfolios.ContainsKey(account))
                {
                    this.portfolios.Add(account, new Portfolio(account, totalNetValue));
                    this.loadedPositionsCount.Add(account, 0);
                    this.isNewPortfolioInitialized.Add(account, false);
                }

                //if (!this.securities.ContainsKey(securityId))
                //{
                //    this.FindSecurities(new SecurityFilter()
                //    {
                //        Code = securityId,
                //        Type = cfiCode.ToSecurityType()
                //    });
                //}

                this.loadedPositionsCount[account] += 1;

                Portfolio portfolio = this.portfolios[account];

                portfolio.SetUsedMargin(usedMargin);
                portfolio.SetCurrentValue(totalNetValue);

                decimal currentValue = 0;
                decimal longQty = longQtyField.getValue();
                decimal shortQty = shortQtyField.getValue();

                if (longQty > 0)
                {
                    currentValue = longQty;
                }

                if (shortQty > 0)
                {
                    currentValue = -shortQty;
                }

                Position position = portfolio.GetPosition(securityId);

                bool isNewPosition = false;
                if (position == null)
                {
                    isNewPosition = true;
                    position = new Position(portfolio, securityId, currentValue, currency);

                    portfolio.AddPosition(position);

                    this.NewPosition(position);
                }

                position = portfolio.GetPosition(securityId);

                position.SetUpdatedAt(DateTimeOffset.UtcNow);
                position.SetCurrentValue(currentValue);

                if (isNewPosition == false)
                {
                    this.PositionChanged(position);
                }

                int loadedPositionsCount = this.loadedPositionsCount[account];
                if (loadedPositionsCount != positionsCount)
                {
                    return;
                }

                if (this.isNewPortfolioInitialized.ContainsKey(account) && this.isNewPortfolioInitialized[account] == true)
                {
                    this.loadedPositionsCount[account] = 0;
                    this.PortfolioChanged(this.portfolios[account]);
                }
                else
                {
                    this.loadedPositionsCount[account] = 0;
                    this.isNewPortfolioInitialized[account] = true;
                    this.NewPortfolio(this.portfolios[account]);

                    // После загрузки портфеля запустим процесс получения заявок
                    if (this.settings.IsTradeStream() == true)
                    {
                        this.ordersUpdateRequest();
                        this.securityEventsAllowed = true;
                    }
                }
            }
        }

        internal void SubscribePortfolioUpdates()
        {
            if (this.settings.IsTradeStream() == false)
            {
                return;
            }

            this.portfolioUpdateTimer = new Timer(
                new TimerCallback((state) => this.requestPortfolio()),
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
                new TimerCallback((state) => this.ordersUpdateRequest()),
                null,
                0,
                this.settings.GetOrdersUpdateInterval() * 1000
            );
        }
    }
}
