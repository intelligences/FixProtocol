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
        event Action<FixMarketDepth> MarketDepthChanged;

        /// <summary>
        /// Connect to protocol
        /// </summary>
        void Connect();

        /// <summary>
        /// Disconnect from FIX
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Subscribe to market depth
        /// </summary>
        /// <param name="securityId">Security identifier</param>
        void SubscribeMarketDepth(string securityId);

        /// <summary>
        /// Unsubscribe from MD
        /// </summary>
        /// <param name="securityId">Security identifier</param>
        void UnsubscribeMarketDepth(string securityId);
    }
}
