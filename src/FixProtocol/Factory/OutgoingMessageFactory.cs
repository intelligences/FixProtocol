using OpenFAST;
using OpenFAST.Template;
using System;

namespace Intelligences.FixProtocol.Factory
{
    internal class OutgoingMessageFactory
    {
        /// <summary>
        /// Registry for available messages
        /// </summary>
        private ITemplateRegistry templateRegistry;

        public OutgoingMessageFactory(ITemplateRegistry templateRegistry)
        {
            this.templateRegistry = templateRegistry;
        }

        public Message LogonMessage(string username, string password)
        {
            MessageTemplate template;
            bool result = this.templateRegistry.TryGetTemplate("Logon", out template);

            Message message = new Message(template);

            message.SetLong("SendingTime", DateTime.UtcNow.Ticks);
            message.SetInteger("HeartbeatInt", 30);
            message.SetString("Username", username);
            message.SetString("Password", password);

            return message;
        }

        public Message Hearbeat()
        {
            MessageTemplate template;
            bool result = this.templateRegistry.TryGetTemplate("Heartbeat", out template);


            var message = new Message(template);
            message.SetLong("SendingTime", DateTime.UtcNow.Ticks);

            return message;
        }
    }
}
