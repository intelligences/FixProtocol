using AllocationInstruction = QuickFix.FIX44.AllocationInstruction;
using ExecutionReport = QuickFix.FIX44.ExecutionReport;
using Intelligences.FixProtocol.Model;
using MarketDepth = Intelligences.FixProtocol.Model.FixMarketDepth;
using MarketDataSnapshotFullRefresh = QuickFix.FIX44.MarketDataSnapshotFullRefresh;
using MarketDataRequestReject = QuickFix.FIX44.MarketDataRequestReject;
using QuickFix.Fields;
using QuickFix;
using FixSettings = Intelligences.FixProtocol.Model.FixSettings;
using FixSecurity = Intelligences.FixProtocol.Model.FixSecurity;
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
using Intelligences.FixProtocol.Exceptions;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Linq;
using Intelligences.FixProtocol.Service;

namespace Intelligences.FixProtocol.Client.Dialects
{
    internal class ExanteDialect : IDialectClient
    {
        /// <summary>
        /// New portfolio event
        /// </summary>
        public event Action<FixAccount> NewAccount;

        /// <summary>
        /// Portfolio changed event
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
        /// Order cancel failed event
        /// </summary>
        internal event Action<FixOrderFail> OrderCancelFailed;

        /// <summary>
        /// Order modify failed event
        /// </summary>
        internal event Action<FixOrderFail> OrderModifyFailed;

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

        internal event Action SecuritiesLoadingProcess;
        internal event Action SecuritiesLoaded;

        /// <summary>
        /// Trades unsubscribed event
        /// </summary>
        public event Action<string> TradesUnSubscribed;

        /// <summary>
        /// Market depth changed
        /// </summary>
        public event Action<MarketDepth> MarketDepthChanged;

        /// <summary>
        /// Market depth unsubscribed
        /// </summary>
        public event Action<string> MarketDepthUnsubscribed;

        /// <summary>
        /// List of portfolios
        /// </summary>
        private readonly Dictionary<string, FixAccount> accounts = new Dictionary<string, FixAccount>();

        /// <summary>
        /// List of portfolios
        /// </summary>
        private readonly Dictionary<string, FixPosition> positions = new Dictionary<string, FixPosition>();

        /// <summary>
        /// List of securities
        /// </summary>
        private readonly ConcurrentDictionary<string, FixSecurity> securities = new ConcurrentDictionary<string, FixSecurity>();

        /// <summary>
        /// List of registered orders
        /// </summary>
        private readonly ConcurrentDictionary<string, FixOrder> orders = new ConcurrentDictionary<string, FixOrder>();

        /// <summary>
        /// List of market depths
        /// </summary>
        private readonly Dictionary<string, MarketDepth> marketDepths = new Dictionary<string, MarketDepth>();

        /// <summary>
        /// List of subscriptions on new trades
        /// </summary>
        private readonly List<string> tradeSubscriptions = new List<string>();

        /// <summary>
        /// Pending orders
        /// </summary>
        private readonly Dictionary<string, FixOrder> pendingOrders = new Dictionary<string, FixOrder>();
        private readonly string requestSecuritiesId = "request-all-securities";

        /// <summary>
        /// FIX session
        /// </summary>
        private Session session;

        /// <summary>
        /// Application settings
        /// </summary>
        private readonly FixSettings settings;

        /// <summary>
        /// Account session ID
        /// </summary>
        private readonly string accountRequestId;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="settings"></param>
        public ExanteDialect(FixSettings settings)
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
        /// <param name="securityId">Security identifier</param>
        public void SubscribeMarketDepth(string securityId)
        {
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
                this.marketDepths.Add(securityId, new FixMarketDepth(securityId, DateTimeOffset.UtcNow));
            }

            Session.SendToTarget(marketDataRequest, this.session.SessionID);
        }

        /// <summary>
        /// Unsubscribe from Market Depth
        /// </summary>
        /// <param name="securityId">Sucurity identifier</param>
        public void UnsubscribeMarketDepth(string securityId)
        {
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
        public void PlaceOrder(FixOrder order)
        {
            if (order is null)
            {
                throw new ArgumentNullException("Order can't be null");
            }

            string security = order.SecurityId;
            string clientOrderId = order.ClientOrderId;

            order.CreatedAt = DateTimeOffset.UtcNow;

            QuickFix.FIX44.NewOrderSingle newFixOrder = new QuickFix.FIX44.NewOrderSingle(
                new ClOrdID(clientOrderId),
                new Symbol(security),
                new Side(order.Direction.ToFixOrderSide()),
                new TransactTime(order.CreatedAt.DateTime),
                new OrdType(order.ToFixOrderType())
            );

            this.fillOrderProperties(newFixOrder, order);

            this.pendingOrders.Add(clientOrderId, order);

            Session.SendToTarget(newFixOrder, this.session.SessionID);
        }

        /// <summary>
        /// Request list of all securities
        /// </summary>
        /// <remarks>If cache enabled, load from cache and async reload</remarks>
        public void RequestSecurities()
        {
            this.SecuritiesLoadingProcess?.Invoke();

            if (this.settings.IsSecuritiesCacheEnabled())
            {
                this.loadSecuritiesFromCache();

                Task.Run(() => {
                    this.requestAllSecurities("update");
                });
            }
            else
            {
                this.requestAllSecurities();
            }
        }

        private void loadSecuritiesFromCache()
        {
            List<FixSecurity> securitiesCache = BinaryStorage.ReadFromBinaryFile<List<FixSecurity>>(this.settings.GetSecuritiesCacheFileName());

            if (securitiesCache is null)
            {
                return;
            }

            if (securitiesCache.Count > 0)
            {
                foreach (var security in securitiesCache)
                {
                    string securityId = security.Id;
                    if (!this.securities.ContainsKey(securityId))
                    {
                        this.securities.TryAdd(securityId, security);
                    }
                    else
                    {
                        this.securities[securityId] = security;
                    }

                    this.NewSecurity(security);
                }

                this.SecuritiesLoaded?.Invoke();
            }
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

        private void requestAllSecurities(string requestId = null)
        {
            QuickFix.FIX44.SecurityListRequest message = new QuickFix.FIX44.SecurityListRequest(
                new SecurityReqID(requestId != null ? requestId : this.requestSecuritiesId),
                new SecurityListRequestType(SecurityListRequestType.ALL_SECURITIES)
            );

            Session.SendToTarget(message, this.session.SessionID);
        }

        /// <summary>
        /// Subscribe Trades
        /// </summary>
        /// <param name="securityId">Security identifier</param>
        public void SubscribeTrades(string securityId)
        {
            if (String.IsNullOrEmpty(securityId))
            {
                throw new ArgumentNullException("Security can't be null");
            }

            if (!this.tradeSubscriptions.Contains(securityId))
            {
                this.tradeSubscriptions.Add(securityId);
            }
        }

        /// <summary>
        /// UnSubscribe Trades
        /// </summary>
        /// <param name="securityId">Security identifier</param>
        public void UnSubscribeTrades(string securityId)
        {
            if (String.IsNullOrEmpty(securityId))
            {
                throw new ArgumentNullException("Security can't be null");
            }

            if (!this.tradeSubscriptions.Contains(securityId))
            {
                throw new InvalidOperationException("Subscription not exists");
            }

            this.tradeSubscriptions.Remove(securityId);

            this.TradesUnSubscribed(securityId);
        }

        /// <summary>
        /// Modify order
        /// </summary>
        /// <param name="order">Order <see cref="FixOrder"/></param>
        public void ModifyOrder(FixOrder order)
        {
            if (order is null)
            {
                throw new ArgumentNullException("Order can't be null");
            }

            string clientOrderId = order.ClientOrderId;
            string security = order.SecurityId;

            QuickFix.FIX44.OrderCancelReplaceRequest request = new QuickFix.FIX44.OrderCancelReplaceRequest(
                new OrigClOrdID(clientOrderId),
                new ClOrdID(Guid.NewGuid().ToString()),
                new Symbol(security),
                new Side(order.Direction.ToFixOrderSide()),
                new TransactTime(order.CreatedAt.DateTime),
                new OrdType(order.ToFixOrderType())
            );

            this.fillOrderProperties(request, order);

            Session.SendToTarget(request, this.session.SessionID);
        }

        /// <summary>
        /// Cancel order
        /// </summary>
        /// <param name="order">Order <see cref="FixOrder"/></param>
        public void CancelOrder(FixOrder order)
        {
            string clientOrderId = order.ClientOrderId;
            string security = order.SecurityId;

            QuickFix.FIX44.OrderCancelRequest cancelOrder = new QuickFix.FIX44.OrderCancelRequest(
                new OrigClOrdID(clientOrderId),
                new ClOrdID(Guid.NewGuid().ToString()),
                new Symbol(security),
                new Side(order.Direction.ToFixOrderSide()),
                new TransactTime(order.CreatedAt.DateTime)
            );

            cancelOrder.SetField(new SecurityIDSource("111"));
            cancelOrder.SetField(new SecurityID(security));
            cancelOrder.SetField(new OrderQty(order.Quantity));

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
            AvgPx avgPxField = new AvgPx();
            SecurityExchange securityExchangeField = new SecurityExchange();
            QuickFix.Fields.Account accountField = new QuickFix.Fields.Account();
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
            var secType = cfiCode.ToSecurityType();
            decimal averagePrice = 0;

            if (message.IsSetField(avgPxField))
            {
                averagePrice = message.GetField(avgPxField).getValue();
            }

            bool isNewAccount = false;
            if (!this.accounts.ContainsKey(account))
            {
                this.accounts.Add(account, new FixAccount(account, currency, totalNetValue));
                isNewAccount = true;
            }

            FixAccount fixAccount = this.accounts[account];

            fixAccount.TotalNewValue = totalNetValue;
            fixAccount.UsedMargin = usedMargin;

            if (isNewAccount) {
                this.NewAccount(fixAccount);
            } else {
                this.AccountChanged(fixAccount);
            }

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

            bool isNewPosition = false;
            if (!this.positions.ContainsKey(securityId))
            {
                this.positions.Add(securityId, new FixPosition(account, securityId, currency, currentValue));
                isNewPosition = true;
            }

            FixPosition position = this.positions[securityId];

            position.UpdatedAt = DateTimeOffset.UtcNow;
            position.CurrentValue = currentValue;
            position.ProfitAndLoss = profitAndLoss;
            position.AveragePrice = averagePrice;

            if (isNewPosition)
            {
                this.NewPosition(position);
            }

            if (isNewPosition)
            {
                this.PositionChanged(position);
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
                FixOrder order;

                ClOrdID clOrdID = new ClOrdID();
                OrderQty orderQty = new OrderQty();
                Side side = new Side();
                QuickFix.Fields.Account account = new QuickFix.Fields.Account();
                SecurityID securityID = new SecurityID();
                OrdStatus ordStatus = new OrdStatus();
                OrdType ordType = new OrdType();
                Price priceField = new Price();
                StopPx stopPxField = new StopPx();
                LastPx lastPx = new LastPx();
                LastQty lastQty = new LastQty();
                CumQty cumQty = new CumQty();
                AvgPx avgPxField = new AvgPx();
                Text text = new Text();
                TransactTime transactionTimeField = new TransactTime();
                ExanteOrdRejReason exanteOrdRejReason = new ExanteOrdRejReason();

                message.GetField(clOrdID);
                message.GetField(orderQty);
                message.GetField(side);
                message.GetField(account);
                message.GetField(securityID);
                message.GetField(ordStatus);
                message.GetField(ordType);
                message.GetField(cumQty);

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

                if (message.IsSetCumQty())
                {
                    message.GetField(cumQty);
                }

                if (message.IsSetStopPx())
                {
                    message.GetField(stopPxField);
                }

                if (message.IsSetTransactTime())
                {
                    message.GetField(transactionTimeField);
                }

                if (message.IsSetAvgPx())
                {
                    message.GetField(avgPxField);
                }

                string clientOrderid = clOrdID.getValue();
                string accountName = account.getValue();
                string securityId = securityID.getValue();
                FixOrderType orderType = ordType.ToOrderType();
                FixOrderState orderState = ordStatus.ToOrderState();
                decimal quantity = orderQty.getValue();
                decimal price = priceField.getValue();
                decimal stopPrice = stopPxField.getValue();
                decimal lastPrice = lastPx.getValue();
                decimal lastQuantity = lastQty.getValue();
                decimal filledQty = cumQty.getValue();
                decimal filledAvgPrice = avgPxField.getValue();
                DateTime transactTime = transactionTimeField.getValue();

                order = this.findOrder(clientOrderid, orderId);

                DateTime dateTime = message.Header.GetDateTime(52);

                if (order == null)
                {
                    if (orderType == FixOrderType.Market)
                    {
                        order = new FixMarketOrder(
                            quantity,
                            side.ToDirection(),
                            accountName,
                            securityId
                        );
                    }
                    else if (orderType == FixOrderType.Limit)
                    {
                        order = new FixLimitOrder(
                            price,
                            quantity,
                            side.ToDirection(),
                            accountName,
                            securityId
                        );
                    }
                    else if (orderType == FixOrderType.StopMarket)
                    {
                        order = new FixStopMarketOrder(
                            price,
                            stopPrice,
                            quantity,
                            side.ToDirection(),
                            accountName,
                            securityId
                        );
                    }
                    else if (orderType == FixOrderType.StopLimit)
                    {
                        order = new FixStopLimitOrder(
                            price,
                            stopPrice,
                            quantity,
                            side.ToDirection(),
                            accountName,
                            securityId
                        );
                    }

                    order.ClientOrderId = clientOrderid;
                    order.State = FixOrderState.PendingRegistration;
                }

                order.ClientOrderId = clientOrderid;
                order.OrderId = orderId;
                order.Quantity = quantity;
                order.FilledQty = filledQty;
                order.FilledAveragePrice = filledAvgPrice;
                order.Price = price;
                order.StopPrice = stopPrice;

                if (orderState == FixOrderState.Rejected)
                {
                    message.GetField(text);
                    message.GetField(exanteOrdRejReason);

                    this.OrderPlaceFailed(new FixOrderFail(order, new OrderException(text.ToString())));
                }

                order.State = orderState;
                order.CreatedAt = dateTime;

                if (transactTime != default)
                {
                    order.UpdatedAt = transactTime;
                }

                if (!this.orders.ContainsKey(orderId))
                {
                    this.orders.TryAdd(orderId, order);

                    this.NewOrder(order);
                }
                else
                {
                    if (orderState == FixOrderState.PartialFilled || orderState == FixOrderState.Filled)
                    {
                        this.NewMyTrade(new FixMyTrade(new FixTrade(securityId, lastPrice, lastQuantity, dateTime), order));
                    }

                    if (orderState == FixOrderState.Filled || orderState == FixOrderState.Rejected || orderState == FixOrderState.Canceled)
                    {
                        this.orders.TryRemove(orderId, out _);
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

            SecurityReqID requId = new SecurityReqID();
            SecurityResponseID responseID = new SecurityResponseID();
            TotNoRelatedSym totalNumberOfSymbols = new TotNoRelatedSym();
            NoRelatedSym numberOfSymbols = new NoRelatedSym();
            SecurityList.NoRelatedSymGroup securityListGroup = new SecurityList.NoRelatedSymGroup();

            message.GetField(requId);
            message.GetField(numberOfSymbols);
            message.GetField(totalNumberOfSymbols);
            message.GetField(responseID);

            for (int i = 1; i <= numberOfSymbols.getValue(); i++)
            {
                message.GetGroup(i, securityListGroup);

                string securityId = securityListGroup.SecurityID.getValue(); // "ES.CME.H2020"

                if (!this.securities.ContainsKey(securityId))
                {
                    this.securities.TryAdd(securityId, new FixSecurity(securityId));
                }

                FixSecurity security = this.securities[securityId];

                if (this.securities.ContainsKey(securityId))
                {
                    string symbol = securityListGroup.Symbol.getValue(); // "ES"
                    string securityExchange = securityListGroup.SecurityExchange.getValue(); // "CME"
                    string currency = securityListGroup.Currency.getValue(); // "USD"
                    string cficode = securityListGroup.CFICode.getValue(); // "FXXXXX"
                    decimal contractSize = securityListGroup.ContractMultiplier.getValue(); // 50M
                    decimal? strikePrice = null; // 50M
                    decimal priceStep = 0; // 0.25M
                    decimal minOrderPrice = 0; // 0.25M
                    decimal lotSize = 0; // 1.0M
                    DateTime? expiryDate = null; // {20.03.2020 0:00:00}
                    decimal initialMargin = 0; // 6300.0M
                    decimal maintenanceMargin = 0; // 6300.0M

                    int attributesCount = securityListGroup.NoInstrAttrib.getValue();
                    AllocationInstruction.NoInstrAttribGroup attributesGroup = new AllocationInstruction.NoInstrAttribGroup();

                    if (securityListGroup.IsSetStrikePrice())
                    {
                        strikePrice = securityListGroup.StrikePrice.getValue();
                    }

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

                    security.Code = symbol;
                    security.Board = securityExchange;
                    security.Type = cficode.ToSecurityType();
                    security.Currency = currency;
                    security.PriceStep = priceStep;
                    security.StepPrice = stepPrice;
                    security.VolumeStep = lotSize;
                    security.StrikePrice = strikePrice;

                    if (expiryDate != null)
                    {
                        security.ExpiryDate = expiryDate;
                    }
                }

                if (this.securities.TryGetValue(securityId, out FixSecurity output))
                {
                    this.NewSecurity(output);
                }
            }


            int totalRows = totalNumberOfSymbols.getValue();
            int rowsOnResponse = numberOfSymbols.getValue();
            int currentResponseId = int.Parse(responseID.getValue());

            int totalResponses = (int)Math.Ceiling((decimal)(totalRows / rowsOnResponse));

            if (currentResponseId == totalResponses)
            {
                if (this.settings.IsSecuritiesCacheEnabled())
                {
                    BinaryStorage.WriteToBinaryFile<List<FixSecurity>>(this.settings.GetSecuritiesCacheFileName(), this.securities.Values.ToList());

                    if (requId.getValue() == this.requestSecuritiesId)
                    {
                        this.SecuritiesLoaded?.Invoke();
                    }
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

                    string seqFlag = this.settings.GetProperty("ResetSeqNumFlag");

                    if (String.IsNullOrEmpty(seqFlag) || seqFlag.ToUpper() == "Y" || seqFlag == "1")
                    {
                        message.SetField(new ResetSeqNumFlag(ResetSeqNumFlag.YES));
                    }
                    else
                    {
                        message.SetField(new ResetSeqNumFlag(ResetSeqNumFlag.NO));
                    }
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

            Model.FixMarketDepth marketDepth = this.marketDepths[mdKey];

            int noMDEntries = message.NoMDEntries.getValue();
            QuickFix.FIX44.MarketDataSnapshotFullRefresh.NoMDEntriesGroup group = new QuickFix.FIX44.MarketDataSnapshotFullRefresh.NoMDEntriesGroup();
            MDEntryType mdEntryType = new MDEntryType();
            MDEntryPx mdEntryPx = new MDEntryPx();
            MDEntrySize mDEntrySize = new MDEntrySize();

            List<Model.FixQuote> bids = new List<Model.FixQuote>();
            List<Model.FixQuote> asks = new List<Model.FixQuote>();

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
                string securityId = marketDepth.SecurityId;

                switch (mdEntryType.getValue())
                {
                    case MDEntryType.BID:
                        bids.Add(new Model.FixQuote(price, volume, Direction.Buy));
                        break;
                    case MDEntryType.OFFER:
                        asks.Add(new Model.FixQuote(price, volume, Direction.Sell));
                        break;
                    case MDEntryType.TRADE:
                        if (this.tradeSubscriptions.Contains(securityId))
                        {
                            this.NewTrade(new FixTrade(securityId, price, volume, DateTimeOffset.UtcNow));
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

            marketDepth.Asks = asks;
            marketDepth.Bids = bids;
            marketDepth.UpdatedAt = DateTimeOffset.UtcNow;

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

                if (this.securities.TryGetValue(securityId, out _))
                {
                    this.MarketDepthUnsubscribed(securityId);
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

                FixOrder order = this.findOrder(clientOrderid, orderId);

                if (order != null)
                {
                    Text text = new Text();
                    CxlRejReason cxlRejReason = new CxlRejReason();

                    message.GetField(text);
                    message.GetField(cxlRejReason);

                    this.OrderPlaceFailed(new FixOrderFail(order, new OrderException(text.ToString())));
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

                FixOrder order = this.findOrder(clientOrderid, orderId);

                if (order != null)
                {
                    Text text = new Text();

                    message.GetField(text);

                    this.OrderModifyFailed(new FixOrderFail(order, new OrderException(text.ToString())));
                }
            }

        }

        #region PrivateMethods
        /// <summary>
        /// Get or add security to dictionary
        /// </summary>
        /// <param name="securityId"></param>
        /// <returns></returns>
        private FixSecurity getOrCreateSecurity(string securityId)
        {
            if (!this.securities.ContainsKey(securityId))
            {
                this.securities.TryAdd(securityId, new FixSecurity(securityId));
            }

            return this.securities[securityId];
        }

        /// <summary>
        /// Find order
        /// </summary>
        /// <param name="clientOrderId"></param>
        /// <param name="orderId"></param>
        /// <returns>Exists order, whitch the placed on exchange</returns>
        private FixOrder findOrder(string clientOrderId, string orderId)
        {
            FixOrder order = null;

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
        private void fillOrderProperties(QuickFix.FIX44.Message newFixOrder, FixOrder order)
        {
            string securityId = order.SecurityId;
            string accountId = order.AccountId;

            if (typeof(QuickFix.FIX44.NewOrderSingle) == newFixOrder.GetType())
            {
                newFixOrder.SetField(new QuickFix.Fields.Account(accountId));
                newFixOrder.SetField(new OrdType(order.ToFixOrderType()));
            }

            newFixOrder.SetField(new OrderQty(order.Quantity));
            newFixOrder.SetField(new SecurityIDSource("111")); // TODO exante only
            newFixOrder.SetField(new SecurityID(securityId));

            if (order.Type == FixOrderType.Limit)
            {
                newFixOrder.SetField(new Price(order.Price));
            }
            else if (order.Type == FixOrderType.StopLimit)
            {
                newFixOrder.SetField(new Price(order.Price));
                newFixOrder.SetField(new StopPx(order.StopPrice));
            }
            else if (order.Type == FixOrderType.StopMarket)
            {
                newFixOrder.SetField(new StopPx(order.StopPrice));
            }

            newFixOrder.SetField(new TimeInForce(order.TimeInForce.ToFixTimeInForce()));
        }
        #endregion PrivateMethods
    }
}
