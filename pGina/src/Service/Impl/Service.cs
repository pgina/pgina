using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Threading;
using System.ServiceProcess;

using log4net;

using pGina.Shared.Settings;
using pGina.Shared.Logging;
using pGina.Shared.Interfaces;

using pGina.Core;
using pGina.Core.Messages;
using pGina.Shared.Types;

using Abstractions.Pipes;
using Abstractions.Logging;
using Abstractions.Helpers;

namespace pGina.Service.Impl
{
    public class Service
    {
        private ILog m_logger = LogManager.GetLogger("pGina.Service.Impl");
        private ILog m_abstractLogger = LogManager.GetLogger("Abstractions");
        private PipeServer m_server = null;
        private TimedCache<int, UserInformation> m_sessionInfoCache = null;

        static Service()
        {
            Framework.Init();            
        }

        private void HookUpAbstractionsLibraryLogging()
        {
            LibraryLogging.AddListener(LibraryLogging.Level.Debug, m_abstractLogger.DebugFormat);
            LibraryLogging.AddListener(LibraryLogging.Level.Error, m_abstractLogger.ErrorFormat);
            LibraryLogging.AddListener(LibraryLogging.Level.Info, m_abstractLogger.InfoFormat);
            LibraryLogging.AddListener(LibraryLogging.Level.Warn, m_abstractLogger.WarnFormat);
        }

        private void DetachAbstractionsLibraryLogging()
        {
            LibraryLogging.RemoveListener(LibraryLogging.Level.Debug, m_abstractLogger.DebugFormat);
            LibraryLogging.RemoveListener(LibraryLogging.Level.Error, m_abstractLogger.ErrorFormat);
            LibraryLogging.RemoveListener(LibraryLogging.Level.Info, m_abstractLogger.InfoFormat);
            LibraryLogging.RemoveListener(LibraryLogging.Level.Warn, m_abstractLogger.WarnFormat);
        }

        public string[] PluginDirectories
        {
            get { return Core.Settings.Get.PluginDirectories; }
        }

        public Service()
        {
            string pipeName = Core.Settings.Get.ServicePipeName;
            int maxClients = Core.Settings.Get.MaxClients;
            int cacheTimeoutInSeconds = Core.Settings.Get.SessionInfoCacheTimeout;
            
            m_logger.DebugFormat("Service created - PipeName: {0} MaxClients: {1} Cache Timeout (secs): {2}", pipeName, maxClients, cacheTimeoutInSeconds);                
            m_sessionInfoCache = new TimedCache<int,UserInformation>(TimeSpan.FromSeconds(cacheTimeoutInSeconds));            
            m_server = new PipeServer(pipeName, maxClients, (Func<dynamic, dynamic>) HandleMessage);                
        }

        public void Start()
        {
            HookUpAbstractionsLibraryLogging();
            m_logger.InfoFormat("Starting service");
            m_server.Start();
        }

        public void Stop()
        {
            DetachAbstractionsLibraryLogging();
            m_logger.InfoFormat("Stopping service");
            m_server.Stop();
        }

        public void SessionChange(SessionChangeDescription changeDescription)
        {
            m_logger.InfoFormat("SessionChange: {0} -> {1}", changeDescription.SessionId, changeDescription.Reason);

            foreach (IPluginEventNotifications plugin in PluginLoader.GetOrderedPluginsOfType<IPluginEventNotifications>())
            {
                try
                {
                    plugin.SessionChange(changeDescription);
                }
                catch (Exception e)
                {
                    m_logger.ErrorFormat("Ignoring unhandled exception from {0}: {1}", plugin.Uuid, e);
                }
            }
            
            if (changeDescription.Reason == SessionChangeReason.SessionLogon)
            {
                // We start our helpers in all sessions on login.  They will then phone home
                //  and ask for user information keyed on session id.  When our CredProv 
                //  authenticates, it indicates what it's session is.  We stash the authenticated
                //  user info in our timed cache keyed on that session id.  Sessions we did 
                //  not authenticate, or those that are re-connecting, wont exist or will timeout
                //  eventually.
                
                // TBD:
                // string process = pGina.Core.Settings.Get.GetSetting("SessionHelperExe", "pGina.Service.SessionHelper.exe");
                // Abstractions.Process.ExecuteInSession(sessionId, process, true);
                // Abstractions.Process.ExecuteInSession(sessionId, process, false);
            }
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

            MessageType type = (MessageType)msg.MessageType;

            // Very noisy, not usually worth having on, configurable via "TraceMsgTraffic" boolean
            bool traceMsgTraffic = pGina.Core.Settings.Get.GetSetting("TraceMsgTraffic", false);
            if (traceMsgTraffic)
            {
                logger.DebugFormat("{0} message received", type);
            }
            
            switch (type)
            {
                case MessageType.Disconnect:
                    // We ack, and mark this as LastMessage, which tells the pipe framework
                    //  not to expect further messages
                    dynamic disconnectAck = new EmptyMessage(MessageType.Ack).ToExpando();  // Ack
                    disconnectAck.LastMessage = true;
                    return disconnectAck;
                case MessageType.Hello:
                    return new EmptyMessage(MessageType.Hello).ToExpando();  // Ack with our own hello
                case MessageType.Log:
                    HandleLogMessage(new LogMessage(msg));
                    return new EmptyMessage(MessageType.Ack).ToExpando();  // Ack
                case MessageType.LoginRequest:
                    return HandleLoginRequest(new LoginRequestMessage(msg)).ToExpando();
                case MessageType.InfoRequest:
                    return HandleInfoRequest(new HelperInfoRequestMessage(msg)).ToExpando();
                default:
                    return null;                // Unknowns get disconnected
            }
        }

        private void HandleLogMessage(LogMessage msg)
        {
            ILog logger = LogManager.GetLogger(string.Format("RemoteLog[{0}]", msg.LoggerName));

            switch (msg.Level.ToLower())
            {
                case "info":
                    logger.InfoFormat("{0}", msg.LoggedMessage);
                    break;
                case "debug":
                    logger.DebugFormat("{0}", msg.LoggedMessage);
                    break;
                case "error":
                    logger.ErrorFormat("{0}", msg.LoggedMessage);
                    break;
                case "warn":
                    logger.WarnFormat("{0}", msg.LoggedMessage);
                    break;
                default:
                    logger.DebugFormat("{0}", msg.LoggedMessage);
                    break;
            }
        }
                
        private HelperInfoResponseMessage HandleInfoRequest(HelperInfoRequestMessage msg)
        {
            HelperInfoResponseMessage response = new HelperInfoResponseMessage()
            {
                Success = m_sessionInfoCache.Exists(msg.Key)
            };
            
            if(response.Success)
            {
                UserInformation info = m_sessionInfoCache.Get(msg.Key);
                response.Username = info.Username;
                response.Password = info.Password;
                response.Domain = info.Domain;

                // We're done with this, lets avoid letting folks spoof and just remove this info now
                m_sessionInfoCache.Remove(msg.Key);
            }

            return response;
        }

        private LoginResponseMessage HandleLoginRequest(LoginRequestMessage msg)
        {
            try
            {
                PluginDriver sessionDriver = new PluginDriver();
                sessionDriver.UserInformation.Username = msg.Username;
                sessionDriver.UserInformation.Password = msg.Password;

                m_logger.DebugFormat("Processing LoginRequest for {0} in session {1}", msg.Username, msg.Session);
                BooleanResult result = sessionDriver.PerformLoginProcess();
                
                m_sessionInfoCache.Add(msg.Session, sessionDriver.UserInformation);

                return new LoginResponseMessage()
                {
                    Result = result.Success,
                    Message = result.Message,
                    Username = sessionDriver.UserInformation.Username,
                    Domain = sessionDriver.UserInformation.Domain,
                    Password = sessionDriver.UserInformation.Password
                };                
            }
            catch (Exception e)
            {
                m_logger.ErrorFormat("Internal error, unexpected exception while handling login request: {0}", e);
                return new LoginResponseMessage() { Result = false, Message = "Internal error" };
            }
        }
    }
}
