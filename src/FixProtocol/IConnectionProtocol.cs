using Intelligences.FixProtocol.Model;
using System;

namespace Intelligences.FixProtocol
{
    public interface IConnectionProtocol
    {
        /// <summary>
        /// Сonnection success event
        /// </summary>
        event Action Connected;

        /// <summary>
        /// Event about success disconnection
        /// </summary>
        event Action Disconnected;

        /// <summary>
        /// Connection success reconnected
        /// </summary>
        event Action ReСonnected;

        /// <summary>
        /// Connection error event
        /// </summary>
        event Action<Exception> ConnectionError;

        ///// <summary>
        ///// Error event
        ///// </summary>
        //event Action<Exception> Error;

        /// <summary>
        /// On market depth changed event
        /// </summary>
        event Action<MarketDepth> MarketDepthChanged;

        /// <summary>
        /// Connect to protocol
        /// </summary>
        void Connect();

        /// <summary>
        /// Disconnect from FIX
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Подписаться на изменения стакана
        /// </summary>
        /// <param name="security">Инструмент <see cref="Security"/></param>
        void SubscribeMarketDepth(Security security);

        /// <summary>
        /// Отписаться от изменений стакана
        /// </summary>
        /// <param name="security">Инструмент <see cref="Security"/></param>
        void UnsubscribeMarketDepth(Security security);
    }
}
