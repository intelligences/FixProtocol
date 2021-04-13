using Intelligences.FixProtocol.Client;
using Intelligences.FixProtocol.Enum;
using Intelligences.FixProtocol.Factory;
using Intelligences.FixProtocol.Filter;
using Intelligences.FixProtocol.Model;
using QuickFix;
using QuickFix.Transport;
using System;

namespace Intelligences.FixProtocol
{
    /// <summary>
    /// FIX Connection
    /// </summary>
    public class FIXConnection : IConnectionProtocol, IDisposable
    {
        /// <summary>
        /// Сonnection success event
        /// </summary>
        public event Action Connected;

        /// <summary>
        /// Event about success disconnection
        /// </summary>
        public event Action Disconnected;

        /// <summary>
        /// Connection success reconnected
        /// </summary>
        public event Action ReСonnected;

        /// <summary>
        /// Connection error event
        /// </summary>
        public event Action<Exception> ConnectionError;

        /// <summary>
        /// Error event
        /// </summary>
        public event Action<Exception> Error;

        /// <summary>
        /// New security event
        /// </summary>
        public event Action<FixSecurity> NewSecurity;

        /// <summary>
        /// On market depth changed event
        /// </summary>
        public event Action<FixMarketDepth> MarketDepthChanged;

        /// <summary>
        /// Market depth unsubscribed event
        /// </summary>
        public event Action<string> MarketDepthUnsubscribed;
        
        /// <summary>
        /// Событие получения новой сделки
        /// </summary>
        public event Action<FixTrade> NewTrade;

        /// <summary>
        /// New account of balance
        /// </summary>
        public event Action<FixAccount> NewAccount;

        /// <summary>
        /// Account of balance changed
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
        /// Событие ошибок
        /// </summary>
        public event Action<FixOrderFail> OrderPlaceFailed;

        /// <summary>
        /// Событие ошибок
        /// </summary>
        public event Action<FixOrderFail> OrderCancelFailed;

        /// <summary>
        /// Событие ошибок
        /// </summary>
        public event Action<FixOrderFail> OrderModifyFailed;

        /// <summary>
        /// New my trade event
        /// </summary>
        public event Action<FixMyTrade> NewMyTrade;
        
        /// <summary>
        /// Trades unsubscribed event
        /// </summary>
        public event Action<string> TradesUnSubscribed;

        /// <summary>
        /// FIX client
        /// </summary>
        private FIXClient fixClient;

        /// <summary>
        /// Сокет инициатор
        /// </summary>
        private SocketInitiator socketInitiator;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="settings"></param>
        public FIXConnection(FixSettings settings)
        {
            SessionSettings sessionSettings = SessionSettingsFactory.Create(settings);

            this.fixClient = new FIXClient(settings);

            if (settings.IsLoggingEnabled())
            {
                this.socketInitiator = new SocketInitiator(
                    this.fixClient,
                    new FileStoreFactory(sessionSettings),
                    sessionSettings,
                    new FileLogFactory(sessionSettings),
                    new DefaultMessageFactory()
                );
            }
            else
            {
                this.socketInitiator = new SocketInitiator(
                    this.fixClient,
                    new FileStoreFactory(sessionSettings),
                    sessionSettings
                );
            }

            this.fixClient.Connected += this.connected;
            this.fixClient.Disconnected += this.disconnected;
            this.fixClient.MarketDepthChanged += this.marketDepthChanged;
            this.fixClient.MarketDepthUnsubscribed += this.marketDepthUnsubscribed;
            this.fixClient.NewSecurity += this.newSecurity;
            this.fixClient.NewAccount += this.newAccount;
            this.fixClient.AccountChanged += this.accountChanged;
            this.fixClient.NewPosition += this.newPosition;
            this.fixClient.PositionChanged += this.positionChanged;
            this.fixClient.NewTrade += this.newTrade;
            this.fixClient.NewOrder += this.newOrder;
            this.fixClient.OrderChanged += this.orderChanged;
            this.fixClient.NewMyTrade += this.newMyTrade;
            this.fixClient.TradesUnSubscribed += this.tradesUnSubscribed;
            this.fixClient.OrderPlaceFailed += this.orderPlaceFailed;
            this.fixClient.OrderCancelFailed += this.orderCancelFailed; 
            this.fixClient.OrderModifyFailed += this.orderModifyFailed;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (socketInitiator != null)
                {
                    this.socketInitiator.Stop();
                    this.fixClient.Dispose();

                    this.fixClient = null;
                    this.socketInitiator = null;

                    GC.Collect();
                }
            }
        }

        /// <summary>
        /// Connect to FIX
        /// </summary>
        public void Connect()
        {
            try
            {
                this.socketInitiator.Start();
            }
            catch (Exception e)
            {
                this.ConnectionError(e);
            }
        }

        /// <summary>
        /// Disconnect from FIX
        /// </summary>
        public void Disconnect()
        {
            this.socketInitiator.Stop();
        }

        public bool IsConnected()
        {
            return this.fixClient.IsConnected();
        }

        public void RequestSecurities()
        {
            if (!this.IsConnected())
            {
                throw new InvalidOperationException("Connection not established");
            }

            this.fixClient.RequestSecurities();
        }

        public void AccountSummaryRequest()
        {
            if (!this.IsConnected())
            {
                throw new InvalidOperationException("Connection not established");
            }

            this.fixClient.AccountSummaryRequest();
        }

        public void OrderMassStatusRequest()
        {
            if (!this.IsConnected())
            {
                throw new InvalidOperationException("Connection not established");
            }

            this.fixClient.OrderMassStatusRequest();
        }

        /// <summary>
        /// Поиск инструментов
        /// </summary>
        /// <param name="securityFilter">Фильтр инструментов <see cref="FixSecurityFilter"/></param>
        public void FindSecurities(FixSecurityFilter securityFilter)
        {
            if (!this.IsConnected())
            {
                throw new InvalidOperationException("Connection not established");
            }

            if (securityFilter != null)
            {
                this.fixClient.FindSecurities(securityFilter);
            }
        }

        /// <summary>
        /// Subscribe to market depth
        /// </summary>
        /// <param name="securityId">Security identifier</param>
        public void SubscribeMarketDepth(string securityId)
        {
            if (!this.IsConnected())
            {
                throw new InvalidOperationException("Connection not established");
            }

            if (String.IsNullOrEmpty(securityId))
            {
                return;
            }

            this.fixClient.SubscribeMarketDepth(securityId);
        }

        public void UnsubscribeMarketDepth(string securityId)
        {
            if (!this.IsConnected())
            {
                throw new InvalidOperationException("Connection not established");
            }

            if (String.IsNullOrEmpty(securityId))
            {
                return;
            }

            this.fixClient.UnsubscribeMarketDepth(securityId);

        }

        public void PlaceOrder(FixOrder order)
        {
            if (order is null)
            {
                throw new ArgumentNullException("Order can't be null");
            }

            order.Id = order.Id ?? Guid.NewGuid().ToString();
            order.ClientOrderId = order.ClientOrderId ?? Guid.NewGuid().ToString();
            order.State = FixOrderState.PendingRegistration;

            this.fixClient.PlaceOrder(order);
        }

        public void CancelOrder(FixOrder order)
        {
            if (order is null)
            {
                throw new ArgumentNullException("Order can't be null");
            }

            this.fixClient.CancelOrder(order);
        }

        public void ModifyOrder(FixOrder order)
        {
            if (order is null)
            {
                throw new ArgumentNullException("Order can't be null");
            }

            this.fixClient.ModifyOrder(order);
        }

        public void SubscribePortfolioUpdates()
        {
            this.fixClient.SubscribePortfolioUpdates();
        }

        public void SubscribeOrdersUpdates()
        {
            this.fixClient.SubscribeOrdersUpdates();
        }

        public void SubscribeTrades(string securityId)
        {
            this.fixClient.SubscribeTrades(securityId);
        }

        public void UnSubscribeTrades(string securityId)
        {
            this.fixClient.UnSubscribeTrades(securityId);
        }

        private void newSecurity(FixSecurity security)
        {
            try
            {
                this.NewSecurity?.Invoke(security);
            }
            catch (Exception e)
            {
                this.Error?.Invoke(e);
            }
        }

        private void marketDepthUnsubscribed(string securityId)
        {
            try
            {
                this.MarketDepthUnsubscribed?.Invoke(securityId);
            }
            catch (Exception e)
            {
                this.Error?.Invoke(e);
            }
        }

        private void marketDepthChanged(FixMarketDepth marketDepth)
        {
            try
            {
                this.MarketDepthChanged?.Invoke(marketDepth);
            }
            catch (Exception e)
            {
                this.Error?.Invoke(e);
            }
        }

        private void connected()
        {
            try
            {
                this.Connected?.Invoke();
            }
            catch (Exception e)
            {
                this.ConnectionError?.Invoke(e);
            }
        }

        private void disconnected()
        {
            try
            {
                this.Disconnected?.Invoke();
            }
            catch (Exception e)
            {
                this.ConnectionError?.Invoke(e);
            }
        }

        private void newAccount(FixAccount portfolio)
        {
            this.NewAccount?.Invoke(portfolio);
        }

        private void accountChanged(FixAccount portfolio)
        {
            this.AccountChanged?.Invoke(portfolio);
        }

        private void newPosition(FixPosition position)
        {
            this.NewPosition?.Invoke(position);
        }

        private void positionChanged(FixPosition position)
        {
            this.PositionChanged?.Invoke(position);
        }

        private void newTrade(FixTrade trade)
        {
            this.NewTrade?.Invoke(trade);
        }

        private void tradesUnSubscribed(string securityId)
        {
            this.TradesUnSubscribed?.Invoke(securityId);
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

        private void orderPlaceFailed(FixOrderFail orderFail)
        {
            this.OrderPlaceFailed?.Invoke(orderFail);
        }

        private void orderCancelFailed(FixOrderFail orderFail)
        {
            this.OrderCancelFailed?.Invoke(orderFail);
        }

        private void orderModifyFailed(FixOrderFail orderFail)
        {
            this.OrderModifyFailed?.Invoke(orderFail);
        }
    }
}
