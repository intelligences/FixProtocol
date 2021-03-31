using Intelligences.FixProtocol.Factory;
using Intelligences.FixProtocol.Filter;
using Intelligences.FixProtocol.Listener;
using Intelligences.FixProtocol.Model;
using OpenFAST.Error;
using OpenFAST.Sessions;
using OpenFAST.Sessions.Tcp;
using OpenFAST.Template;
using OpenFAST.Template.Loader;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Intelligences.FixProtocol.Client
{
    internal class FASTClient : IDisposable
    {
        /// <summary>
        /// Сonnection success event
        /// </summary>
        internal event Action Connected;

        /// <summary>
        /// Event about success disconnection
        /// </summary>
        internal event Action Disconnected;

        /// <summary>
        /// Connection success reconnected
        /// </summary>
        internal event Action ReСonnected;

        /// <summary>
        /// Connection error event
        /// </summary>
        internal event Action<Exception> ConnectionError;

        /// <summary>
        /// Error event
        /// </summary>
        internal event Action<Exception> Error;

        /// <summary>
        /// Событие изменения стакана
        /// </summary>
        internal event Action<FixMarketDepth> MarketDepthChanged;

        /// <summary>
        /// FAST prorocol
        /// </summary>
        private readonly SessionControlProtocol11 protocol;

        /// <summary>
        /// 
        /// </summary>
        private readonly ITemplateRegistry templateRegistry;

        /// <summary>
        /// 
        /// </summary>
        private readonly OutgoingMessageFactory outgoingMessageFactory;

        /// <summary>
        /// 
        /// </summary>
        private readonly IMessageListener messageListener;

        /// <summary>
        /// Settings
        /// </summary>
        private readonly FixSettings settings;

        /// <summary>
        /// Fast session
        /// </summary>
        private Session session;

        /// <summary>
        /// 
        /// </summary>
        private FastClient fastClient;

        public FASTClient(FixSettings settings)
        {
            this.settings = settings;

            string file = settings.GetFastTemplatePatch();

            XmlMessageTemplateLoader templateLoader = new XmlMessageTemplateLoader
            {
                LoadTemplateIdFromAuxId = true,
            };

            if (!File.Exists(file))
            {
                throw new FileNotFoundException("The file by path: \"" + file + "\"not found");
            }

            templateLoader.Load(new FileStream(file, FileMode.Open, FileAccess.Read));

            this.templateRegistry = templateLoader.TemplateRegistry;
            this.outgoingMessageFactory = new OutgoingMessageFactory(this.templateRegistry);
            this.messageListener = new MessagesListener();

            this.protocol = new SessionControlProtocol11();
            this.protocol.RegisterSessionTemplates(this.templateRegistry);

            TcpEndpoint tcpEndpoint = new TcpEndpoint(this.settings.GetProperty("Host"), this.settings.GetProperty("Port"));

            this.fastClient = new FastClient("client", this.protocol, tcpEndpoint)
            {
                InboundTemplateRegistry = templateRegistry,
                OutboundTemplateRegistry = templateRegistry
            };
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Connect()
        {
            if (this.IsConnected())
            {
                throw new FastConnectionException("Already connected to the FAST server.");
            }

            this.session = this.fastClient.Connect();

            this.session.MessageOutputStream.TemplateRegistry.RegisterAll(this.templateRegistry);
            //this.session.ErrorHandler = new ClientErrorHandler();

            this.session.MessageHandler = this.messageListener;
            var message = this.outgoingMessageFactory.LogonMessage(this.settings.GetProperty("Login"), this.settings.GetProperty("Password"));

            this.session.MessageOutputStream.WriteMessage(message);

            Debug.WriteLine("<fast out>: {0}", message);
        }

        public void Disconnect()
        {
            if (!this.IsConnected())
            {
                throw new FastConnectionException("Already disconnected from FAST server");
            }

            this.session.MessageOutputStream.WriteMessage(protocol.CloseMessage);

            int step = 0;
            while (this.session.IsListening && step < 20)
            {
                Thread.Sleep(100);
                step++;
            }
            this.session.Close();
            //_clientMessageHandler.Clear();
        }

        /// <summary>
        /// Is connected
        /// </summary>
        /// <returns></returns>
        internal bool IsConnected()
        {
            return this.session != null && this.session.IsListening;
        }

        /// <summary>
        /// Search securiries by filter
        /// </summary>
        /// <param name="securityFilter">Security Filter <see cref="SecurityFilter"/></param>
        public void FindSecurities(SecurityFilter filter)
        {

        }

        /// <summary>
        /// Subscribe to market depth
        /// </summary>
        /// <param name="securityId">Security Identifier</param>
        public void SubscribeMarketDepth(string securityId)
        {

        }


        #region Nested type: ClientErrorHandler

        private class ClientErrorHandler : IErrorHandler
        {
            public void OnError(Exception exception, StaticError error, string format, params object[] args)
            {
                if (format != null)
                    Debug.WriteLine(format, args);
                else
                    Debug.WriteLine(error);
            }

            public void OnError(Exception exception, DynError error, string format, params object[] args)
            {
                if (format != null)
                    Debug.WriteLine(format, args);
                else
                    Debug.WriteLine(error);
            }

            public void OnError(Exception exception, RepError error, string format, params object[] args)
            {
                if (format != null)
                    Debug.WriteLine(format, args);
                else
                    Debug.WriteLine(error);
            }
        }

        #endregion
    }
}
