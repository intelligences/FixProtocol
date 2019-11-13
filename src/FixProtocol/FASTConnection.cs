using Intelligences.FixProtocol.Client;
using Intelligences.FixProtocol.Model;
using System;

namespace Intelligences.FixProtocol
{
    /// <summary>
    /// 
    /// </summary>
    public class FASTConnection : IConnectionProtocol, IDisposable
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

        /// <summary>
        /// FIX connection service
        /// </summary>
        private readonly FASTClient fastService;

        public FASTConnection(Settings settings)
        {
            this.fastService = new FASTClient(settings);

            this.fastService.Connected += this.connected;
            this.fastService.Disconnected += this.disconnected;
            this.fastService.MarketDepthChanged += this.marketDepthChanged;
        }

        public void Dispose()
        {
            this.fastService.Dispose();
        }

        /// <summary>
        /// Connect to FAST
        /// </summary>
        public void Connect()
        {
            this.fastService.Connect();
        }

        /// <summary>
        /// Disconnect from FIX
        /// </summary>
        public void Disconnect()
        {
            this.fastService.Disconnect();
        }

        /// <summary>
        /// Подписаться на изменения стакана
        /// </summary>
        /// <param name="security">Инструмент <see cref="Security"/></param>
        public void SubscribeMarketDepth(Security security)
        {
            this.fastService.SubscribeMarketDepth(security);
        }

        public void UnsubscribeMarketDepth(Security security)
        {
            //this.fastService.UnsubscribeMarketDepth(security);
        }
        

        /// <summary>
        /// Connected to server event
        /// </summary>
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

        /// <summary>
        /// Disconnected from server event
        /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="marketDepth"></param>
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
    }
}
