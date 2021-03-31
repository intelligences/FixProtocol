using Intelligences.FixProtocol.Model;

namespace Intelligences.FixProtocol.Client
{
    internal interface IMarketDataClient
    {
        void SubscribeMarketDepth(FixSecurity security);
        void UnSubscribeMarketDepth(FixSecurity security);

        //void SubscribeTrades(Security security);
        //void UnSubscribeTrades(Security security);

        //void SubscribeCandles(Security security);
        //void UnSubscribeCandles(Security security);
    }
}
