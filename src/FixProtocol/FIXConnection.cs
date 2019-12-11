using Intelligences.FixProtocol.Client;
using Intelligences.FixProtocol.DTO;
using Intelligences.FixProtocol.Enum;
using Intelligences.FixProtocol.Factory;
using Intelligences.FixProtocol.Filter;
using Intelligences.FixProtocol.Model;
using QuickFix;
using QuickFix.Transport;
using System;
using System.Diagnostics;

namespace Intelligences.FixProtocol
{
    /// <summary>
    /// 
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
        public event Action<Security> NewSecurity;

        /// <summary>
        /// On market depth changed event
        /// </summary>
        public event Action<MarketDepth> MarketDepthChanged;
        public event Action<Security> MarketDepthUnsubscribed;
        
        /// <summary>
        /// Событие получения новой сделки
        /// </summary>
        public event Action<Trade> NewTrade;

        /// <summary>
        /// New portfolio
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
        /// New my trade event
        /// </summary>
        public event Action<MyTrade> NewMyTrade;
        
        /// <summary>
        /// Trades unsubscribed event
        /// </summary>
        public event Action<Security> TradesUnSubscribed;

        /// <summary>
        /// FIX client
        /// </summary>
        private FIXClient fixClient;

        /// <summary>
        /// Сокет инициатор
        /// </summary>
        private SocketInitiator socketInitiator;

        public FIXConnection(Model.Settings settings)
        {
            SessionSettings sessionSettings = SessionSettingsFactory.Create(settings);

            this.fixClient = new FIXClient(settings);

            this.socketInitiator = new SocketInitiator(
                this.fixClient,
                new FileStoreFactory(sessionSettings),
                sessionSettings,
                new FileLogFactory(sessionSettings),
                new DefaultMessageFactory()
            );

            this.fixClient.Connected += this.connected;
            this.fixClient.Disconnected += this.disconnected;
            this.fixClient.MarketDepthChanged += this.marketDepthChanged;
            this.fixClient.MarketDepthUnsubscribed += this.marketDepthUnsubscribed;
            this.fixClient.NewSecurity += this.newSecurity;
            this.fixClient.NewPortfolio += this.newPortfolio;
            this.fixClient.PortfolioChanged += this.portfolioChanged;
            this.fixClient.NewPosition += this.newPosition;
            this.fixClient.PositionChanged += this.positionChanged;
            this.fixClient.NewTrade += this.newTrade;
            this.fixClient.NewOrder += this.newOrder;
            this.fixClient.OrderChanged += this.orderChanged;
            this.fixClient.NewMyTrade += this.newMyTrade;
            this.fixClient.TradesUnSubscribed += this.tradesUnSubscribed;
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
                Debug.WriteLine(e.Message);
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

        /// <summary>
        /// Поиск инструментов
        /// </summary>
        /// <param name="securityFilter">Фильтр инструментов <see cref="SecurityFilter"/></param>
        public void FindSecurities(SecurityFilter securityFilter)
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

        public void CreateSecurity(SecurityData securityData)
        {
            this.fixClient.CreateSecurity(securityData);
        }

        /// <summary>
        /// Load all securities
        /// </summary>
        internal void LoadAllSecurities()
        {
            if (!this.IsConnected())
            {
                throw new InvalidOperationException("Connection not established");
            }

            this.fixClient.LoadAllSecurities();
        }

        /// <summary>
        /// Подписаться на изменения стакана
        /// </summary>
        /// <param name="security">Инструмент <see cref="Security"/></param>
        public void SubscribeMarketDepth(Security security)
        {
            if (!this.IsConnected())
            {
                throw new InvalidOperationException("Connection not established");
            }

            if (security != null)
            {
                this.fixClient.SubscribeMarketDepth(security);
            }
        }

        public void UnsubscribeMarketDepth(Security security)
        {
            if (!this.IsConnected())
            {
                throw new InvalidOperationException("Connection not established");
            }

            if (security != null)
            {
                this.fixClient.UnsubscribeMarketDepth(security);
            }
        }

        public void PlaceOrder(Order order)
        {
            if (order is null)
            {
                throw new ArgumentNullException("Order can't be null");
            }

            order.SetTransactionId(Guid.NewGuid().ToString());
            order.SetClientOrderId(Guid.NewGuid().ToString());
            order.SetState(OrderState.Pending);

            this.fixClient.PlaceOrder(order);
        }

        public void CancelOrder(Order order)
        {
            if (order is null)
            {
                throw new ArgumentNullException("Order can't be null");
            }

            this.fixClient.CancelOrder(order);
        }

        public void ModifyOrder(Order order)
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

        public void SubscribeTrades(Security security)
        {
            this.fixClient.SubscribeTrades(security);
        }

        public void UnSubscribeTrades(Security security)
        {
            this.fixClient.UnSubscribeTrades(security);
        }

        private void newSecurity(Security security)
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

        private void marketDepthUnsubscribed(Security security)
        {
            try
            {
                this.MarketDepthUnsubscribed?.Invoke(security);
            }
            catch (Exception e)
            {
                this.Error?.Invoke(e);
            }
        }

        private void marketDepthChanged(MarketDepth marketDepth)
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

        private void newPortfolio(Portfolio portfolio)
        {
            this.NewPortfolio?.Invoke(portfolio);
        }

        private void portfolioChanged(Portfolio portfolio)
        {
            this.PortfolioChanged?.Invoke(portfolio);
        }

        private void newPosition(Position position)
        {
            this.NewPosition?.Invoke(position);
        }

        private void positionChanged(Position position)
        {
            this.PositionChanged?.Invoke(position);
        }

        private void newTrade(Trade trade)
        {
            this.NewTrade?.Invoke(trade);
        }

        private void tradesUnSubscribed(Security security)
        {
            this.TradesUnSubscribed?.Invoke(security);
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
            this.NewMyTrade?.Invoke(myTrade);
        }
    }
}
