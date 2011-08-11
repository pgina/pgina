using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Threading;

using log4net;

using pGina.Shared.Settings;
using pGina.Shared.Logging;
using pGina.Shared.Interfaces;

using pGina.Core;
using pGina.Core.Messages;

using Abstractions.Pipes;

namespace pGina.Service.Impl
{
    public class Service
    {
        private ILog m_logger = LogManager.GetLogger("pGina.Service.Impl");
        private PipeServer m_server = null;

        static Service()
        {
            Framework.Init();
        }

        public string[] PluginDirectories
        {
            get { return Core.Settings.Get.PluginDirectories; }
        }

        public Service()
        {
            string pipeName = Core.Settings.Get.ServicePipeName;
            int maxClients = Core.Settings.Get.MaxClients;

            m_logger.DebugFormat("Service created - PipeName: {0} MaxClients: {1}", pipeName, maxClients);                
            m_server = new PipeServer(pipeName, maxClients, (Func<dynamic, dynamic>) HandleMessage);                
        }

        public void Start()
        {
            m_logger.InfoFormat("Starting service");
            m_server.Start();
        }

        public void Stop()
        {
            m_logger.InfoFormat("Stopping service");
            m_server.Stop();
        }

        // This will be called on seperate threads, 1 per client connection and
        //  represents a connected client - that is, until we return null,
        //  the connection remains open and operations on behalf of this client
        //  should occur in this thread etc.  The current managed thread id 
        //  can be used to differentiate between instances if scope requires.
        private dynamic HandleMessage(dynamic msg)
        {
            int instance = Thread.CurrentThread.ManagedThreadId;
            ILog logger = LogManager.GetLogger(string.Format("HandleMessage[{0}]", instance));

            logger.DebugFormat("{0} message received", msg.MessageType);

            MessageType type = msg.MessageType;
            switch (type)
            {
                case MessageType.Disconnect:
                    return null;                // Ack a disconnect by just closing
                case MessageType.Hello:
                    return new EmptyMessage(MessageType.Hello).ToExpando();  // Ack with our own hello
                case MessageType.Log:
                    HandleLogMessage(new LogMessage(msg));
                    return new EmptyMessage(MessageType.Ack).ToExpando();  // Ack with our own hello
                default:
                    return null;                // Unknowns get disconnected
            }
        }

        private void HandleLogMessage(LogMessage msg)
        {
            ILog logger = LogManager.GetLogger(msg.LoggerName);

            switch (msg.Level.ToLower())
            {
                case "info":
                    logger.InfoFormat("RemoteLog: {0}", msg.LoggedMessage);
                    break;
                case "debug":
                    logger.DebugFormat("RemoteLog: {0}", msg.LoggedMessage);
                    break;
                case "error":
                    logger.ErrorFormat("RemoteLog: {0}", msg.LoggedMessage);
                    break;
                case "warn":
                    logger.WarnFormat("RemoteLog: {0}", msg.LoggedMessage);
                    break;
                default:
                    logger.DebugFormat("RemoteLog: {0}", msg.LoggedMessage);
                    break;
            }
        }
    }
}
