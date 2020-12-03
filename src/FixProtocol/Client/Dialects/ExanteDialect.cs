using AllocationInstruction = QuickFix.FIX44.AllocationInstruction;
using ExecutionReport = QuickFix.FIX44.ExecutionReport;
using Intelligences.FixProtocol.Model;
using MarketDepth = Intelligences.FixProtocol.Model.MarketDepth;
using MarketDataSnapshotFullRefresh = QuickFix.FIX44.MarketDataSnapshotFullRefresh;
using MarketDataRequestReject = QuickFix.FIX44.MarketDataRequestReject;
using QuickFix.Fields;
using QuickFix;
using Settings = Intelligences.FixProtocol.Model.Settings;
using Security = Intelligences.FixProtocol.Model.Security;
using SecurityList = QuickFix.FIX44.SecurityList;
using System;
using System.Collections.Generic;
using Intelligences.FixProtocol.Enum;
using TimeInForce = QuickFix.Fields.TimeInForce;
using System.Globalization;
using Intelligences.FixProtocol.Filter;
using SecurityType = Intelligences.FixProtocol.Enum.SecurityType;
using Intelligences.FixProtocol.Fields;
using Tags = QuickFix.Fields.Tags;
using Intelligences.FixProtocol.DTO;
using Intelligences.FixProtocol.Model.Conditions;
using Intelligences.FixProtocol.Exceptions;

namespace Intelligences.FixProtocol.Client.Dialects
{
    internal class ExanteDialect : IDialectClient
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
        /// Order cancel failed event
        /// </summary>
        internal event Action<OrderFail> OrderCancelFailed;

        /// <summary>
        /// Order modify failed event
        /// </summary>
        internal event Action<OrderFail> OrderModifyFailed;

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

        /// <summary>
        /// List of portfolios
        /// </summary>
        private readonly Dictionary<string, Portfolio> portfolios = new Dictionary<string, Portfolio>();

        /// <summary>
        /// List of securities
        /// </summary>
        private readonly Dictionary<string, Security> securities = new Dictionary<string, Security>();

        /// <summary>
        /// List of registered orders
        /// </summary>
        private readonly Dictionary<string, Order> orders = new Dictionary<string, Order>();

        /// <summary>
        /// List of market depths
        /// </summary>
        private readonly Dictionary<string, MarketDepth> marketDepths = new Dictionary<string, MarketDepth>();

        /// <summary>
        /// List of subscriptions on new trades
        /// </summary>
        private readonly List<string> tradeSubscriptions = new List<string>();

        /// <summary>
        /// Number of loaded positions
        /// </summary>
        private readonly Dictionary<string, int> loadedPositionsCount = new Dictionary<string, int>();

        /// <summary>
        /// List with statuses, is the item currently loaded
        /// </summary>
        private readonly Dictionary<string, bool> isNewPortfolioInitialized = new Dictionary<string, bool>();

        /// <summary>
        /// Pending orders
        /// </summary>
        private readonly Dictionary<string, Order> pendingOrders = new Dictionary<string, Order>();

        /// <summary>
        /// FIX session
        /// </summary>
        private Session session;

        /// <summary>
        /// Application settings
        /// </summary>
        private readonly Settings settings;

        /// <summary>
        /// Account session ID
        /// </summary>
        private readonly string accountRequestId;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="settings"></param>
        public ExanteDialect(Settings settings)
        {
            this.settings = settings;
            this.accountRequestId = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Set session
        /// </summary>
        public void SetSession(Session session)
        {
            this.session = session;
        }

        /// <summary>
        /// Subscribe on market depth
        /// </summary>
        /// <param name="security">Security</param>
        public void SubscribeMarketDepth(Security security)
        {
            string securityId = security.GetId();

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

            #region temporarilyDeleted
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
            #endregion temporarilyDeleted

            QuickFix.FIX44.MarketDataRequest.NoRelatedSymGroup symGroup = new QuickFix.FIX44.MarketDataRequest.NoRelatedSymGroup();
            symGroup.Set(new SecurityIDSource("111"));
            symGroup.Set(new SecurityID(securityId));
            symGroup.Set(new Symbol(securityId));

            marketDataRequest.AddGroup(symGroup);

            if (!this.marketDepths.ContainsKey(securityId))
            {
                this.marketDepths.Add(securityId, new Model.MarketDepth(security, DateTimeOffset.UtcNow));
            }

            Session.SendToTarget(marketDataRequest, this.session.SessionID);
        }

        /// <summary>
        /// Unsubscribe from Market Depth
        /// </summary>
        /// <param name="security"></param>
        public void UnsubscribeMarketDepth(Security security)
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

            Session.SendToTarget(marketDataRequest, this.session.SessionID);
        }

        /// <summary>
        /// Place new order on the exchange
        /// </summary>
        /// <param name="order">New order</param>
        public void PlaceOrder(Order order)
        {
            if (order is null)
            {
                throw new ArgumentNullException("Order can't be null");
            }

            Security security = order.GetSecurity();
            string clientOrderId = order.GetClientOrderId();

            order.SetCreatedAt(DateTimeOffset.UtcNow);

            QuickFix.FIX44.NewOrderSingle newFixOrder = new QuickFix.FIX44.NewOrderSingle(
                new ClOrdID(clientOrderId),
                new Symbol(security.GetId()),
                new Side(order.GetDirection().ToFixOrderSide()),
                new TransactTime(order.GetCreatedAt().DateTime),
                new OrdType(order.GetFixOrderType())
            );

            this.fillOrderProperties(newFixOrder, order);

            this.pendingOrders.Add(clientOrderId, order);

            Session.SendToTarget(newFixOrder, this.session.SessionID);
        }

        /// <summary>
        /// Find securities on the exchange
        /// </summary>
        /// <param name="securityFilter">Security filter <see cref="SecurityFilter"/></param>
        public void FindSecurities(SecurityFilter securityFilter)
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

            string securityRequestId = Guid.NewGuid().ToString();

            QuickFix.FIX44.SecurityListRequest message = new QuickFix.FIX44.SecurityListRequest(
                new SecurityReqID(securityRequestId),
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

            Session.SendToTarget(message, this.session.SessionID);
        }

        public void CreateSecurity(SecurityData securityData)
        {
            if (securityData is null)
            {
                throw new ArgumentNullException("SecurityData can't be null");
            }

            string code = securityData.Code;
            string board = securityData.Board;
            SecurityType type = securityData.Type;
            string currency = securityData.Currency;
            decimal priceStep = securityData.PriceStep;
            decimal stepPrice = securityData.StepPrice;
            decimal volumeStep = securityData.VolumeStep;
            DateTimeOffset? expiryDate = securityData.ExpiryDate;
            int digits = securityData.Decimals;

            if (!this.securities.ContainsKey(code))
            {
                this.securities.Add(code, new Security(code));
            }

            Security security = this.securities[code];

            if (code != default)
            {
                security.SetCode(code);
            }

            if (board != default)
            {
                security.SetBoard(board);
            }

            if (type != default)
            {
                security.SetSecurityType(type);
            }

            if (currency != default)
            {
                security.SetCurrency(currency);
            }

            if (priceStep != default)
            {
                security.SetPriceStep(priceStep);
            }

            if (stepPrice != default)
            {
                security.SetStepPrice(stepPrice);
            }

            if (expiryDate != null)
            {
                security.SetExpiryDate((DateTimeOffset) expiryDate);
            }

            if (digits != default)
            {
                security.SetDigits(digits);
            }

            if (volumeStep != default)
            {
                security.SetVolumeStep(volumeStep);
            }

            this.NewSecurity(security);
        }

        /// <summary>
        /// Subscribe Trades
        /// </summary>
        /// <param name="security">Security <see cref="Security"/></param>
        public void SubscribeTrades(Security security)
        {
            if (security is null)
            {
                throw new ArgumentNullException("Security can't be null");
            }

            string securityId = security.GetId();

            if (!this.tradeSubscriptions.Contains(securityId))
            {
                this.tradeSubscriptions.Add(securityId);
            }
        }

        /// <summary>
        /// UnSubscribe Trades
        /// </summary>
        /// <param name="security">Security <see cref="Security"/></param>
        public void UnSubscribeTrades(Security security)
        {
            if (security is null)
            {
                throw new ArgumentNullException("Security can't be null");
            }

            string securityId = security.GetId();

            if (!this.tradeSubscriptions.Contains(securityId))
            {
                throw new InvalidOperationException("Subscription not exists");
            }

            this.tradeSubscriptions.Remove(securityId);

            this.TradesUnSubscribed(security);
        }

        /// <summary>
        /// Modify order
        /// </summary>
        /// <param name="order">Order <see cref="Order"/></param>
        public void ModifyOrder(Order order)
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
                new OrdType(order.GetFixOrderType())
            );

            this.fillOrderProperties(request, order);

            Session.SendToTarget(request, this.session.SessionID);
        }

        /// <summary>
        /// Cancel order
        /// </summary>
        /// <param name="order">Order <see cref="Order"/></param>
        public void CancelOrder(Order order)
        {

            string clientOrderId = order.GetClientOrderId();
            Security security = order.GetSecurity();
            string securityId = security.GetId();

            QuickFix.FIX44.OrderCancelRequest cancelOrder = new QuickFix.FIX44.OrderCancelRequest(
                new OrigClOrdID(clientOrderId),
                new ClOrdID(Guid.NewGuid().ToString()),
                new Symbol(securityId),
                new Side(order.GetDirection().ToFixOrderSide()),
                new TransactTime(order.GetCreatedAt().DateTime)
            );

            cancelOrder.SetField(new SecurityIDSource("111"));
            cancelOrder.SetField(new SecurityID(securityId));
            cancelOrder.SetField(new OrderQty(order.GetQuantity()));

            Session.SendToTarget(cancelOrder, this.session.SessionID);
        }

        /// <summary>
        /// Request to get all orders
        /// </summary>
        public void OrderMassStatusRequest()
        {
            if (this.session.IsLoggedOn == false)
            {
                return;
            }

            QuickFix.FIX44.OrderMassStatusRequest message = new QuickFix.FIX44.OrderMassStatusRequest(
                new MassStatusReqID(Guid.NewGuid().ToString()),
                new MassStatusReqType(MassStatusReqType.STATUS_FOR_ALL_ORDERS)
            );

            Session.SendToTarget(message, this.session.SessionID);
        }

        /// <summary>
        /// Account summary request
        /// </summary>
        public void AccountSummaryRequest()
        {
            if (this.session.IsLoggedOn == false)
            {
                return;
            }

            QuickFix.Message message = new QuickFix.Message();
            message.Header.SetField(new MsgType("UASQ"));

            message.SetField(new AccSumReqID(this.accountRequestId));

            Session.SendToTarget(message, this.session.SessionID);
        }

        /// <summary>
        /// Parse account info about portfolio and positions
        /// </summary>
        /// <param name="message">Fix message</param>
        public void ParseAccountSummaryResponse(Message message)
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

            if (message.IsSetField(securityExchangeField))
            {
                securityExchange = message.GetField(securityExchangeField).getValue();
            }

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

            Security security = this.getOrCreateSecurity(securityId);
            Position position = portfolio.GetPosition(security);

            bool isNewPosition = false;
            if (position == null)
            {
                isNewPosition = true;

                position = new Position(portfolio, security, currentValue, currency);

                portfolio.AddPosition(position);

                this.NewPosition(position);
            }

            position = portfolio.GetPosition(security);

            position.SetUpdatedAt(DateTimeOffset.UtcNow);
            position.SetCurrentValue(currentValue);
            position.SetProfitAndLoss(profitAndLoss);

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
                //if (this.settings.IsTradeStream() == true)
                //{
                //    this.ordersUpdateRequest();
                //    this.securityEventsAllowed = true;
                //}
            }
        }

        /// <summary>
        /// Parse orders execution info
        /// </summary>
        /// <param name="message">Fix message</param>
        public void ParseOrdersExecutionReport(ExecutionReport message)
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
                LastPx lastPx = new LastPx();
                LastQty lastQty = new LastQty();
                Text text = new Text();
                ExanteOrdRejReason exanteOrdRejReason = new ExanteOrdRejReason();

                message.GetField(clOrdID);
                message.GetField(orderQty);
                message.GetField(side);
                message.GetField(account);
                message.GetField(securityID);
                message.GetField(ordStatus);
                message.GetField(ordType);
              
                if (message.IsSetPrice())
                {
                    message.GetField(priceField);
                }

                if (message.IsSetLastPx())
                {
                    message.GetField(lastPx);
                }

                if (message.IsSetLastQty())
                {
                    message.GetField(lastQty);
                }

                string clientOrderid = clOrdID.getValue();
                string accountName = account.getValue();
                string securityId = securityID.getValue();
                OrderType orderType = ordType.ToOrderType();
                OrderState orderState = ordStatus.ToOrderState();
                decimal quantity = orderQty.getValue();
                decimal price = priceField.getValue();
                decimal lastPrice = lastPx.getValue();
                decimal lastQuantity = lastQty.getValue();

                if (!this.portfolios.TryGetValue(accountName, out Portfolio portfolio))
                {
                    return;
                }

                Security security = this.getOrCreateSecurity(securityId);

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

                if (orderState == OrderState.Failed)
                {
                    message.GetField(text);
                    message.GetField(exanteOrdRejReason);

                    this.OrderPlaceFailed(new OrderFail(order, new OrderException(text.ToString())));
                }

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
                    order.SetUpdatedAt(dateTime);

                    if (orderState == OrderState.PartialFilled || orderState == OrderState.Filled)
                    {
                        this.NewMyTrade(new MyTrade(new Trade(security, lastPrice, lastQuantity, dateTime), order));
                    }

                    if (orderState == OrderState.Filled || orderState == OrderState.Failed || orderState == OrderState.Canceled)
                    {
                        this.orders.Remove(orderId);
                    }

                    this.OrderChanged(order);
                }
            }
        }

        /// <summary>
        /// Parse securities list
        /// </summary>
        /// <param name="message">Fix message</param>
        public void ParseSecuritiesList(SecurityList message)
        {
            string status = message.GetField(560);

            if (status == "2")
            {
                //Debug.WriteLine("No instruments found that match selection criteria");
            }

            if (status != "0")
            {
                return;
            }

            NoRelatedSym numberOfSymbols = new NoRelatedSym();
            SecurityList.NoRelatedSymGroup securityListGroup = new SecurityList.NoRelatedSymGroup();

            message.GetField(numberOfSymbols);

            for (int i = 1; i <= numberOfSymbols.getValue(); i++)
            {
                message.GetGroup(i, securityListGroup);

                string securityId = securityListGroup.SecurityID.getValue(); // "ES.CME.H2020"

                if (!this.securities.ContainsKey(securityId))
                {
                    this.securities.Add(securityId, new Security(
                        securityId
                    ));
                }

                Security security = this.securities[securityId];

                if (this.securities.ContainsKey(securityId))
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

                    security.SetCode(symbol);
                    security.SetBoard(securityExchange);
                    security.SetSecurityType(cficode.ToSecurityType());
                    security.SetCurrency(currency);
                    security.SetPriceStep(priceStep);
                    security.SetStepPrice(stepPrice);
                    security.SetDigits(digits);
                    security.SetVolumeStep(lotSize);

                    if (expiryDate != null)
                    {
                        security.SetExpiryDate(expiryDate);
                    }
                }

                if (this.securities.TryGetValue(securityId, out Security output))
                {
                    this.NewSecurity(output);
                }
            }
        }

        /// <summary>
        /// Actions before sending message to exchange
        /// </summary>
        /// <param name="message">FIX message</param>
        public void PreOutputMessageAction(Message message)
        {
            if (message.Header.GetField(Tags.MsgType) == MsgType.LOGON)
            {
                string password = this.settings.GetProperty("Password");

                if (password != null)
                {
                    message.SetField(new Password(this.settings.GetProperty("Password")));
                }
            }
        }

        /// <summary>
        /// Parse market data snapshot
        /// </summary>
        /// <param name="message">FIX message</param>
        public void ParseMarketDataSnapshotFullRefresh(MarketDataSnapshotFullRefresh message)
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

            List<Model.Quote> bids = new List<Model.Quote>();
            List<Model.Quote> asks = new List<Model.Quote>();

            for (int i = 1; i <= noMDEntries; i++)
            {
                message.GetGroup(i, group);
                group.Get(mdEntryType);

                if (group.IsSetMDEntryPx())
                {
                    group.Get(mdEntryPx);
                }

                if (group.IsSetMDEntrySize())
                {
                    group.Get(mDEntrySize);
                }

                decimal price = mdEntryPx.getValue();
                decimal volume = mDEntrySize.getValue();
                Security security = marketDepth.GetSecurity();

                switch (mdEntryType.getValue())
                {
                    case MDEntryType.BID:
                        bids.Add(new Model.Quote(price, volume, Direction.Buy));
                        break;
                    case MDEntryType.OFFER:
                        asks.Add(new Model.Quote(price, volume, Direction.Sell));
                        break;
                    case MDEntryType.TRADE:
                        if (this.tradeSubscriptions.Contains(security.GetId()))
                        {
                            this.NewTrade(new Trade(security, price, volume, DateTimeOffset.UtcNow));
                        }
                        break;

                        #region TemporaryRemoved2
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
                        #endregion TemporaryRemoved2
                }
            }

            marketDepth.SetAsks(asks);
            marketDepth.SetBids(bids);
            marketDepth.SetUpdatedAt(DateTimeOffset.UtcNow);

            if (asks.Count > 0 && bids.Count > 0)
            {
                this.MarketDepthChanged(marketDepth);
            }
        }

        /// <summary>
        /// Parse market data request reject
        /// </summary>
        /// <param name="message">FIX message</param>
        public void ParseMarketDataRequestReject(MarketDataRequestReject message)
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

        public void OrderCancelReject(QuickFix.FIX44.OrderCancelReject message)
        {
            OrderID orderID = new OrderID();
            message.GetField(orderID);

            string orderId = orderID.getValue();

            if (orderId != "NONE")
            {
                ClOrdID clOrdID = new ClOrdID();

                message.GetField(clOrdID);

                string clientOrderid = clOrdID.getValue();

                Order order = this.findOrder(clientOrderid, orderId);

                if (order != null)
                {
                    Text text = new Text();
                    CxlRejReason cxlRejReason = new CxlRejReason();

                    message.GetField(text);
                    message.GetField(cxlRejReason);

                    this.OrderPlaceFailed(new OrderFail(order, new OrderException(text.ToString())));
                }
            }
        }

        public void OrderCancelReplaceRequest(QuickFix.FIX44.OrderCancelReplaceRequest message)
        {
            OrderID orderID = new OrderID();
            message.GetField(orderID);

            string orderId = orderID.getValue();

            if (orderId != "NONE")
            {
                ClOrdID clOrdID = new ClOrdID();

                message.GetField(clOrdID);

                string clientOrderid = clOrdID.getValue();

                Order order = this.findOrder(clientOrderid, orderId);

                if (order != null)
                {
                    Text text = new Text();

                    message.GetField(text);

                    this.OrderModifyFailed(new OrderFail(order, new OrderException(text.ToString())));
                }
            }

        }

        #region PrivateMethods
        /// <summary>
        /// Get or add security to dictionary
        /// </summary>
        /// <param name="securityId"></param>
        /// <returns></returns>
        private Security getOrCreateSecurity(string securityId)
        {
            if (!this.securities.ContainsKey(securityId))
            {
                this.securities.Add(securityId, new Security(securityId));
            }

            return this.securities[securityId];
        }

        /// <summary>
        /// Find order
        /// </summary>
        /// <param name="clientOrderId"></param>
        /// <param name="orderId"></param>
        /// <returns>Exists order, whitch the placed on exchange</returns>
        private Order findOrder(string clientOrderId, string orderId)
        {
            Order order = null;

            if (this.pendingOrders.TryGetValue(clientOrderId, out order))
            {
                this.pendingOrders.Remove(clientOrderId);

                return order;
            }

            if (this.orders.TryGetValue(orderId, out order))
            {
                return order;
            }

            return order;
        }

        /// <summary>
        /// Fill order fields
        /// </summary>
        /// <param name="newFixOrder"></param>
        /// <param name="order"></param>
        private void fillOrderProperties(QuickFix.FIX44.Message newFixOrder, Order order)
        {
            Security security = order.GetSecurity();
            string securityId = security.GetId();
            Portfolio portfolio = order.GetPortfolio();

            if (typeof(QuickFix.FIX44.NewOrderSingle) == newFixOrder.GetType())
            {
                newFixOrder.SetField(new Account(portfolio.GetName()));
                newFixOrder.SetField(new OrdType(order.GetFixOrderType()));
            }

            newFixOrder.SetField(new OrderQty(order.GetQuantity()));
            newFixOrder.SetField(new SecurityIDSource("111")); // TODO exante only
            newFixOrder.SetField(new SecurityID(securityId));

            if (order.GetOrderType() == OrderType.Limit)
            {
                newFixOrder.SetField(new Price(order.GetPrice()));
            }
            else if (order.GetOrderType() == OrderType.Conditional)
            {
                IOrderCondition condition = order.GetCondition();

                if (condition.GetType() == typeof(StopLimit))
                {
                    newFixOrder.SetField(new Price(order.GetPrice()));

                    StopLimit stopLimitCondition = (StopLimit)condition;
                    newFixOrder.SetField(new StopPx(stopLimitCondition.GetPrice()));
                }

                if (condition.GetType() == typeof(StopMarket))
                {
                    StopMarket stopLimitCondition = (StopMarket)condition;
                    newFixOrder.SetField(new StopPx(stopLimitCondition.GetPrice()));
                }
            }

            newFixOrder.SetField(new TimeInForce(order.GetTimeInForce().ToFixTimeInForce())); ;
        }
        #endregion PrivateMethods
    }
}
