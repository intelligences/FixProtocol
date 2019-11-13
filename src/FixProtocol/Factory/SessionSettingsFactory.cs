using QuickFix;
using System;
using System.Collections.Generic;

namespace Intelligences.FixProtocol.Factory
{
    public static class SessionSettingsFactory
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static SessionSettings Create(Model.Settings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException();
            }

            string sender = settings.GetProperty("SenderCompID");
            string target = settings.GetProperty("TargetCompID");

            //string target = settings.GetTarget();
            //string socketHost = settings.GetSocketHost();
            //int socketPort = settings.GetSocketPort();
            //string password = settings.GetPassword();

            SessionID sessionId = new SessionID("FIX.4.4", sender, target);
            Dictionary defaultConfig = new Dictionary();
            Dictionary sessionConfig = new Dictionary();

            defaultConfig.SetString(SessionSettings.CONNECTION_TYPE, "initiator");
            //defaultConfig.SetString(SessionSettings.FILE_STORE_PATH, "FIX\\Files");
            //defaultConfig.SetString(SessionSettings.FILE_LOG_PATH, "FIX\\Log");
            //defaultConfig.SetString(SessionSettings.START_DAY, "SUN");
            //defaultConfig.SetString(SessionSettings.END_DAY, "SUN");
            //defaultConfig.SetString(SessionSettings.START_TIME, "22:00:00");
            //defaultConfig.SetString(SessionSettings.END_TIME, "22:00:00");
            //defaultConfig.SetLong(SessionSettings.HEARTBTINT, 30);
            //defaultConfig.SetString(SessionSettings.RESET_ON_LOGON, "Y");
            //defaultConfig.SetString(SessionSettings.CHECK_LATENCY, "N");
            //defaultConfig.SetLong("ORDER_TIMEOUT", 30000);
            //defaultConfig.SetString(SessionSettings.USE_DATA_DICTIONARY, "N");
            //defaultConfig.SetString(SessionSettings.DATA_DICTIONARY, "FIX44.xml");

            //sessionConfig.SetString(SessionSettings.BEGINSTRING, "FIX.4.4");

            foreach(KeyValuePair<string, dynamic> item in settings.GetProperties())
            {
                if (item.Value is string)
                {
                    sessionConfig.SetString(item.Key, item.Value);
                }
                else if (item.Value is int)
                {
                    sessionConfig.SetLong(item.Key, item.Value);
                }
                else if (item.Value is double)
                {
                    sessionConfig.SetDouble(item.Key, item.Value);
                }
                else if (item.Value is bool)
                {
                    sessionConfig.SetBool(item.Key, item.Value);
                }
                else if (item.Value is DayOfWeek)
                {
                    sessionConfig.SetDay(item.Key, item.Value);
                }

            }

            SessionSettings sessionSettings = new SessionSettings();

            sessionSettings.Set(defaultConfig);
            sessionSettings.Set(sessionConfig);
            sessionSettings.Set(sessionId, defaultConfig);

            return sessionSettings;
        }
    }
}
