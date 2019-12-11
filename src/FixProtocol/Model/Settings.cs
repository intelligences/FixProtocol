using Intelligences.FixProtocol.Enum;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Intelligences.FixProtocol.Model
{
    /// <summary>
    /// FIX and FAST Settings model
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// Patch to fast template
        /// </summary>
        private string fastTemplatePatch;

        /// <summary>
        /// Dialect of FIX protocol
        /// </summary>
        private Dialect dialect;

        /// <summary>
        /// Version of FIX protocol
        /// </summary>
        private FixVersion fixVersion;

        /// <summary>
        /// List of properties
        /// </summary>
        private readonly Dictionary<string, dynamic> properties = new Dictionary<string, dynamic>();

        /// <summary>
        /// Interval to update positions info
        /// </summary>
        private int portfolioUpdateInterval;

        /// <summary>
        /// Interval to update positions info
        /// </summary>
        private int ordersUpdateInterval;

        /// <summary>
        /// If stream is stream for trades
        /// </summary>
        private bool isTradeStream;

        //private bool useCacheForSecuritiesList;

        //private TimeSpan cacheUpdateTime;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="sender"></param>
        /// <param name="socketHost"></param>
        /// <param name="socketPort"></param>
        /// <param name="dialect"></param>
        /// <param name="fixVersion"></param>
        public Settings(string target, string sender, string socketHost, int socketPort, Dialect dialect = Dialect.Default, FixVersion fixVersion = FixVersion.Fix44)
        {
            this.dialect = dialect;
            this.fixVersion = fixVersion;
            this.portfolioUpdateInterval = 60;
            this.ordersUpdateInterval = 60;
            this.isTradeStream = false;

            this.SetProperty("BeginString", "FIX4.4");

            this.SetProperty("TargetCompID", target);
            this.SetProperty("SenderCompID", sender);
            this.SetProperty("SocketConnectHost", socketHost);
            this.SetProperty("SocketConnectPort", socketPort);

            this.SetProperty("ReconnectInterval", 30);
            this.SetProperty("HeartBtInt", 30);
            this.SetProperty("StartDay", "SUN");
            this.SetProperty("EndDay", "SUN");
            this.SetProperty("StartTime", "22:00:00");
            this.SetProperty("EndTime", "22:00:00");
            this.SetProperty("ResetOnLogon", "Y");
            this.SetProperty("CheckLatency", "N");
            this.SetProperty("UseDataDictionary", "N");
            this.SetProperty("DataDictionary", "Dictionaries/FIX44.xml");
            this.SetProperty("FileStorePath", "FIX\\Files");
            this.SetProperty("FileLogPath", "FIX\\Log");
            this.SetProperty("ValidateFieldsOutOfOrder", "Y");

            if (this.dialect == Dialect.GainCapital)
            {
                List<string> requiredFields = new List<string>(new string[] { "UUID", "Login", "Password" });

                if (!String.IsNullOrEmpty(this.fastTemplatePatch))
                {
                    requiredFields.Add("FastPort");
                }

                if (
                    this.checkRequiredFields(requiredFields)
                ) {
                    throw new InvalidSettingsException(Resource.GainCapitalRequiredFieldsNotExists);
                }
            }
            else if (this.dialect == Dialect.Exante)
            {
                List<string> requiredFields = new List<string>(new string[] { "Password" });

                if (
                    this.checkRequiredFields(requiredFields)
                ) {
                    throw new InvalidSettingsException(Resource.GainCapitalRequiredFieldsNotExists);
                }
            }
        }

        public void TradeStream()
        {
            this.isTradeStream = true;
        }

        public string GetFastTemplatePatch()
        {
            return this.fastTemplatePatch;
        }

        public void SetFastTemplatePatch(string patch)
        {
            this.fastTemplatePatch = patch;
        }

        public Dialect GetDialect()
        {
            return this.dialect;
        }

        public int GetPortfolioUpdateInterval()
        {
            return this.portfolioUpdateInterval;
        }

        public void SetPortfolioUpdateInterval(int portfolioUpdateInterval)
        {
            this.portfolioUpdateInterval = portfolioUpdateInterval;
        }

        public int GetOrdersUpdateInterval()
        {
            return this.ordersUpdateInterval;
        }

        public void SetOrdersUpdateInterval(int ordersUpdateInterval)
        {
            this.ordersUpdateInterval = ordersUpdateInterval;
        }

        public void SetProperty(string key, dynamic value)
        {
            if (this.properties.ContainsKey(key))
            {
                this.properties[key] = value;
            }
            else
            {
                this.properties.Add(key, value);
            }
        }

        public dynamic GetProperty(string key)
        {
            if (!this.properties.ContainsKey(key))
            {
                return null;
            }

            return this.properties[key];
        }

        public Dictionary<string, dynamic> GetProperties()
        {
            return this.properties;
        }

        //public bool isUseCacheForSecuritiesList()
        //{
        //    return this.useCacheForSecuritiesList;
        //}

        //public void UseCacheForSecuritiesList()
        //{
        //    this.useCacheForSecuritiesList = true;
        //}

        //public TimeSpan GetCacheUpdateTime()
        //{
        //    return this.cacheUpdateTime;
        //}

        //public void SetCacheUpdateTime(TimeSpan timeSpan)
        //{
        //    this.cacheUpdateTime = timeSpan;
        //}

        private bool checkRequiredFields(List<string> keys)
        {
            bool containKeys = !keys.Except(this.properties.Keys).Any();
            bool dictionaryContainsValues = this.properties.Where(p => keys.All(r => r == p.Key && String.IsNullOrEmpty(p.Value))).Any();

            return containKeys && dictionaryContainsValues;
        }

        internal bool IsTradeStream()
        {
            return this.isTradeStream;
        }
    }
}
