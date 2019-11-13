using Intelligences.FixProtocol;
using Intelligences.FixProtocol.Enum;
using Intelligences.FixProtocol.Filter;
using Intelligences.FixProtocol.Model;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace FixProtocol.Sample.SimpleWpfSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        private FIXConnection connection;

        public MainWindow()
        {
            InitializeComponent();

            //Settings settings = new Settings("EXANTE_TRADE_UAT",
            //     "FTD8044_TRADE_UAT",
            //     "fixuat2.exante.eu",
            //     8101,
            //     Dialect.Exante);

            //settings.SetProperty("Password", "_OvW*MPY#l1M");
            //settings.SetProperty("DataDictionary", "Dictionaries/FIX44.xml");
            //settings.SetProperty("UseDataDictionary", "Y");

            Settings settings = new Settings(
                "OEC_TEST",
                 "SRashin8927",
                 "api.gainfutures.com",
                 9300,
                 Dialect.GainCapital
            );

            settings.SetProperty("Password", "API29932");
            settings.SetProperty("FastPort", 9301);
            settings.SetProperty("UUID", "9e61a8bc-0a31-4542-ad85-33ebab0e4e86");
            settings.SetProperty("DataDictionary", "Dictionaries/FIX44.xml");
            settings.SetProperty("UseDataDictionary", "Y");

            this.connection = new FIXConnection(settings);

            Debug.WriteLine("Connection...");

            this.connection.Connected += () =>
            {
                Debug.WriteLine("FIX Connected !!!");

                //this.connection.FindSecurities(new SecurityFilter()
                //{
                //    Symbol = "ES",
                //    //Type = SecurityType.Future,
                //});

                //this.connection.FindSecurities(new SecurityFilter()
                //{
                //    //Symbol = "esz9",
                //    //Symbol = "TEST-RANDOM",
                //    //Type = SecurityType.Future,
                //});
            };

            this.connection.NewSecurity += (Security security) =>
            {
                //if (security.GetId() == "ES.CME.U2019")
                //{
                //    Debug.WriteLine("Подписываемся на инструмент: " + security.GetId());
                //    this.connection.SubscribeMarketDepth(security);
                //}

                if (security.GetId() == "TEST-RANDOM.TEST")
                {
                    Debug.WriteLine("Подписываемся на инструмент: " + security.GetId());
                    this.connection.SubscribeMarketDepth(security);
                }
            };

            this.connection.Disconnected += () =>
            {
                Debug.WriteLine("FIX disconnected");
            };


            Closing += this.onWindowClosing;
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
                if (this.connection != null)
                {
                    this.connection.Disconnect();
                    this.connection = null;

                    GC.Collect();
                }
            }
        }

        private void onWindowClosing(object sender, CancelEventArgs e)
        {
            this.connection.Disconnect();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.connection.Connect();

            //try
            //{
            //    this.tradeConnection.Connect();
            //    this.marketDataConnection.Connect();
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e.ToString());
            //}
        }

        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            this.connection.FindSecurities(new SecurityFilter()
            {
                Symbol = FindSecuritiesBtn.Text,
                //Type = SecurityType.Future,
            });
        }
    }
}
