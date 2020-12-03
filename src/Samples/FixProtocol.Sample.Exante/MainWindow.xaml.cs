using Intelligences.FixProtocol.Enum;
using Intelligences.FixProtocol.Filter;
using Intelligences.FixProtocol.Model;
using Intelligences.FixProtocol.Sample.Exante.View;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace Intelligences.FixProtocol.Sample.Exante
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private FIXConnection tradeStream;
        private FIXConnection marketDataStream;
        private Security security;
        private Portfolio portfolio;

        private readonly ObservableCollection<MarketDepthView> marketDepthList;
        private readonly ObservableCollection<OrderView> ordersList;

        public MainWindow()
        {
            InitializeComponent();

            Closing += this.onWindowClosing;

            this.marketDepthList = new ObservableCollection<MarketDepthView>();
            this.ordersList = new ObservableCollection<OrderView>();

            this.MarketDepthList.ItemsSource = this.marketDepthList;
            this.OrdersList.ItemsSource = this.ordersList;
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
                if (this.marketDataStream != null)
                {
                    this.marketDataStream.Disconnect();
                    this.marketDataStream = null;
                }

                if (this.tradeStream != null)
                {
                    this.tradeStream.Disconnect();
                    this.tradeStream = null;
                }

                GC.Collect();
            }
        }

        private void onWindowClosing(object sender, CancelEventArgs e)
        {
            this.Dispose(true);
        }

        private void ConnectButtonEvent(object sender, RoutedEventArgs e)
        {
            if (this.tradeStream != null && this.tradeStream.IsConnected())
            {
                this.tradeStream.Disconnect();
                this.marketDataStream.Disconnect();
                this.tradeStream.Dispose();
                this.marketDataStream.Dispose();
                this.tradeStream = null;
                this.marketDataStream = null;

                return;
            }

            Settings settings = new Settings(
               this.TradeStreamTarget.Text,
               this.TradeStreamSender.Text,
               "fixuat2.exante.eu",
               8101,
               Dialect.Exante
            );

            settings.SetProperty("Password", this.TradeStreamPassword.Text);
            settings.SetProperty("DataDictionary", "Dictionaries/FIX44Exante.xml");
            settings.SetProperty("UseDataDictionary", "Y");

            settings.SetPortfolioUpdateInterval(20);
            settings.SetOrdersUpdateInterval(20);
            settings.TradeStream();
            settings.SetProperty("ResetOnLogon", "N");

            this.tradeStream = new FIXConnection(settings);

            Settings settings2 = new Settings(
                this.MarketStreamTarget.Text,
                this.MarketStreamSender.Text,
                "fixuat2.exante.eu",
                8100,
                Dialect.Exante
            );

            settings2.SetProperty("Password", this.MarketStreamPassword.Text);
            settings2.SetProperty("DataDictionary", "Dictionaries/FIX44Exante.xml");
            settings2.SetProperty("UseDataDictionary", "Y");

            this.marketDataStream = new FIXConnection(settings2);

            /// Portfolios
            this.tradeStream.NewPortfolio += (Portfolio portfolio) =>
            {
                this.portfolio = portfolio;
                Debug.WriteLine("Portfolio: {0}, {1}", portfolio.GetName(), portfolio.GetCurrentValue());
            };

            this.tradeStream.PortfolioChanged += (Portfolio portfolio) =>
            {
               // Debug.WriteLine("Portfolio: {0}, {1}", portfolio.GetName(), portfolio.GetCurrentValue());
            };

            /// Positions
            this.tradeStream.NewPosition += (Position position) =>
            {
               // Debug.WriteLine("Position: {0}, {1}", position.GetSecurityCode(), position.GetCurrentValue());
            };

            this.tradeStream.PositionChanged += (Position position) =>
            {
                //Debug.WriteLine("Position: {0}, {1}", position.GetSecurityCode(), position.GetCurrentValue());
            };

            this.tradeStream.NewMyTrade += (MyTrade myTrade) =>
            {
                Debug.WriteLine("New my trade: {0}, {1}, {2}", myTrade.GetOrder().GetOrderId(), myTrade.GetTrade().GetPrice(), myTrade.GetTrade().GetVolume());
            };

            // connection to feed stream
            this.tradeStream.Connected += () =>
            {
                this.changeConnectBtnText("Disconnect");

                Debug.WriteLine("FIX Feed stream Connected !!!");
               // this.marketDataStream.Connect();
                this.tradeStream.SubscribePortfolioUpdates();
                this.tradeStream.SubscribeOrdersUpdates();
            };

            //// connection to market data stream
            this.marketDataStream.Connected += () =>
            {
                Debug.WriteLine("FIX Market data stream Connected !!!");

                this.tradeStream.FindSecurities(new SecurityFilter()
                {
                    Code = "TEST-RANDOM",
                });

                //this.tradeStream.FindSecurities(new SecurityFilter()
                //{
                //    Code = "ES",
                //    Type = SecurityType.Future,
                //});
            };

            this.tradeStream.Disconnected += () =>
            {
                this.changeConnectBtnText("Connect");

                Debug.WriteLine("FIX disconnected");
            };

            this.tradeStream.NewSecurity += (Security security) =>
            {
                if (security.GetId() == "TEST-RANDOM.TEST")
                {
                    Debug.WriteLine("Подписываемся на инструмент: " + security.GetId());
                    this.marketDataStream.SubscribeMarketDepth(security);

                    this.security = security;
                }
                //if (security.GetId() == "ES.CME.Z2019")
                //{
                //    Debug.WriteLine("Подписываемся на инструмент: " + security.GetId());
                //    this.marketDataStream.SubscribeMarketDepth(security);

                //    this.security = security;
                //}
            };

            this.marketDataStream.MarketDepthChanged += (MarketDepth marketDepth) =>
            {
                this.MarketDepthList.Dispatcher.Invoke(new Action(() =>
                {
                    this.marketDepthList.Clear();

                    foreach (var ask in marketDepth.GetAsks())
                    {
                        this.marketDepthList.Add(new MarketDepthView()
                        {
                            Price = ask.GetPrice(),
                            Ask = ask.GetVolume(),
                        });
                    }

                    foreach (var bid in marketDepth.GetBids())
                    {
                        this.marketDepthList.Add(new MarketDepthView() {
                            Price = bid.GetPrice(),
                            Bid = bid.GetVolume(),
                        });
                    }

                    this.MarketDepthList.Items.Refresh();
                }));
            };

            this.tradeStream.NewOrder += (Order order) =>
            {
                this.OrdersList.Dispatcher.Invoke(new Action(() =>
                {
                    this.ordersList.Add(new OrderView(order));
                }));
            };

            this.tradeStream.OrderChanged += (Order order) =>
            {
                this.OrdersList.Dispatcher.Invoke(new Action(() =>
                {
                    OrderView ord = this.ordersList.FirstOrDefault(x => x.TransactionId == order.GetTransactionId());

                    if (ord != null)
                    {
                        ord.UpdateView(order);

                        this.OrdersList.Items.Refresh();
                    }
                }));
            };

            this.changeConnectBtnText("Connection...");

            this.tradeStream.Connect();
        }

        private void changeConnectBtnText(string text)
        {
            this.ConnectButton.Dispatcher.Invoke(new Action(() =>
            {
                this.ConnectButton.Content = text;
            }));
        }

        private void MarketBuyBtn_Click(object sender, RoutedEventArgs e)
        {
            this.tradeStream.PlaceOrder(new MarketOrder(1, Direction.Buy, this.portfolio, this.security));
        }

        private void MarketSellBtn_Click(object sender, RoutedEventArgs e)
        {
            this.tradeStream.PlaceOrder(new MarketOrder(1, Direction.Sell, this.portfolio, this.security));
        }

        private void BuyLimitBtn_Click(object sender, RoutedEventArgs e)
        {
            this.tradeStream.PlaceOrder(new LimitOrder(this.buyLimitValue, 1, Direction.Buy, this.portfolio, this.security));
        }

        private void SellLimitBtn_Click(object sender, RoutedEventArgs e)
        {
            this.tradeStream.PlaceOrder(new LimitOrder(this.sellLimitValue, 1, Direction.Sell, this.portfolio, this.security));
        }

        private decimal buyLimitValue = 0;
        private decimal sellLimitValue = 0;
        private Order selectedOrder;

        private void BuyLimitValue_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            this.buyLimitValue = convStringToDecimal(this.BuyLimitValueElement.Text);

            if (this.buyLimitValue > 0)
            {
                this.BuyLimitBtn.IsEnabled = true;
            }
        }

        private void SellLimitValue_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            this.sellLimitValue = convStringToDecimal(this.SellLimitValueElement.Text);

            if (this.sellLimitValue > 0)
            {
                this.SellLimitBtn.IsEnabled = true;
            }
        }

        private decimal convStringToDecimal(string str)
        {
            if (String.IsNullOrEmpty(str))
            {
                return 0;
            }

            return Decimal.Parse(str, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
        }

        private void CancelOrderBtn_Click(object sender, RoutedEventArgs e)
        {
            OrderView orderView = this.OrdersList.SelectedItem as OrderView;

            if (orderView.GetOrder() != null)
            {
                this.tradeStream.CancelOrder(orderView.GetOrder());
            }
        }


        private void ChangeOrderPrice_Click(object sender, RoutedEventArgs e)
        {
            OrderView orderView = this.OrdersList.SelectedItem as OrderView;
            Order order = orderView.GetOrder();
            if (order != null)
            {
                order.SetPrice(order.GetPrice() - 0.1m);
                this.tradeStream.ModifyOrder(order);
            }
        }

        private void MarketDepthList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            MarketDepthView marketDepthRow = this.MarketDepthList.SelectedItem as MarketDepthView;

            if (marketDepthRow == null)
            {
                return;
            }

            this.BuyLimitValueElement.Text =  marketDepthRow.Price.ToString(CultureInfo.InvariantCulture);
            this.SellLimitValueElement.Text = marketDepthRow.Price.ToString(CultureInfo.InvariantCulture);
        }
    }
}