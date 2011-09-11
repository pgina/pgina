/*
	Copyright (c) 2011, pGina Team
	All rights reserved.

	Redistribution and use in source and binary forms, with or without
	modification, are permitted provided that the following conditions are met:
		* Redistributions of source code must retain the above copyright
		  notice, this list of conditions and the following disclaimer.
		* Redistributions in binary form must reproduce the above copyright
		  notice, this list of conditions and the following disclaimer in the
		  documentation and/or other materials provided with the distribution.
		* Neither the name of the pGina Team nor the names of its contributors 
		  may be used to endorse or promote products derived from this software without 
		  specific prior written permission.

	THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
	ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
	WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
	DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY
	DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
	(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
	LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
	ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
	(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
	SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Threading;
using System.ServiceProcess;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Net;

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
            m_logger.InfoFormat("Starting service");
            HookUpAbstractionsLibraryLogging();            
            m_server.Start();
            PluginDriver.Starting();
        }

        public void Stop()
        {
            m_logger.InfoFormat("Stopping service");
            PluginDriver.Stopping();
            DetachAbstractionsLibraryLogging();            
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

            m_logger.DebugFormat("Change: {0} Session: {1}", changeDescription.Reason, changeDescription.SessionId); 

            if (changeDescription.Reason == SessionChangeReason.SessionLogon)
            {
                // We only start session helpers in sessions we know have recently (within our timed cache
                // timeframe that is) authenticated with our provider.  We could run them for everyone...
                if (m_sessionInfoCache.Exists(changeDescription.SessionId))
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(KickoffSessionHelperThread), changeDescription.SessionId);
                }
            }
        }

        private const int ERROR_INVALID_PARAMETER = 87;
        private const int ERROR_PIPE_NOT_CONNECTED = 233;

        private void KickoffSessionHelperThread(object id)
        {
            KickoffSessionHelpers((int)id);
        }

        private void KickoffSessionHelpers(int sessionId)
        {
            string helperApp = pGina.Core.Settings.Get.GetSetting("SessionHelperExe", "pGina.Service.SessionHelper.exe");            
            int numHelperStartRetries = pGina.Core.Settings.Get.GetSetting("HelperStartRetryCount", 60);
            int helperStartRetryDelay = pGina.Core.Settings.Get.GetSetting("HelperStartRetryDelay", 1000);
            string systemApp = string.Format("{0} --serviceMode", helperApp);

            Process systemHelper = null;
            Process userHelper = null;

            try
            {
                // We know from experience that we are racing to start our helper as the session is 
                // being setup, and when we win that race, we get invalid param or epipe errors.  So 
                // we backoff for a few seconds then try again, up to 60 times, waiting 1 second each time.
                m_logger.DebugFormat("Starting session helper app in system context: {0} in session {1}", systemApp, sessionId);
                for (int x = 0; x < numHelperStartRetries && systemHelper == null; ++x)
                {
                    try
                    {
                        systemHelper = Abstractions.WindowsApi.pInvokes.StartProcessInSession(sessionId, systemApp);
                        m_logger.DebugFormat("System helper app started as process id: {0} ({1} retries necessary)", systemHelper.Id, x);
                    }
                    catch (Win32Exception exception)
                    {
                        if (exception.NativeErrorCode != ERROR_INVALID_PARAMETER &&
                            exception.NativeErrorCode != ERROR_PIPE_NOT_CONNECTED)
                            break;
                    }

                    Thread.Sleep(helperStartRetryDelay);
                }

                // Rinse repeat, this time for the user helper.  Note that ideally retries aren't necessary here,
                //  as we would have eventually started the system helper previously... but we'll do it anyway, 
                //  for completeness sake.
                m_logger.DebugFormat("Starting session helper app in user context: {0} in session {1}", helperApp, sessionId);
                for (int x = 0; x < numHelperStartRetries && userHelper == null; ++x)
                {
                    try
                    {
                        userHelper = Abstractions.WindowsApi.pInvokes.StartUserProcessInSession(sessionId, helperApp);
                        m_logger.DebugFormat("User helper app started as process id: {0} ({1} retries necessary)", userHelper.Id, x);
                    }
                    catch (Win32Exception exception)
                    {
                        if (exception.NativeErrorCode != ERROR_INVALID_PARAMETER &&
                            exception.NativeErrorCode != ERROR_PIPE_NOT_CONNECTED)
                            break;
                    }

                    Thread.Sleep(helperStartRetryDelay);
                }                
            }
            finally
            {
                if (systemHelper != null) systemHelper.Dispose();
                if (userHelper != null) userHelper.Dispose();
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
                case MessageType.DynLabelRequest:
                    return HandleDynamicLabelRequest(new DynamicLabelRequestMessage(msg)).ToExpando();
                case MessageType.LoginInfoChange:
                    return HandleLoginInfoChange(new LoginInfoChangeMessage(msg)).ToExpando();
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
            m_logger.DebugFormat("HelperInfo requested for session: {0}", msg.Key);
            HelperInfoResponseMessage response = new HelperInfoResponseMessage()
            {
                Success = m_sessionInfoCache.Exists(msg.Key)
            };
            
            if(response.Success)
            {
                m_logger.DebugFormat("Info found, returning user data");
                UserInformation info = m_sessionInfoCache.Get(msg.Key);
                response.Username = info.Username;
                // We always send an empty string for password, see GH issue #30
                // response.Password = info.Password;
                response.Password = "";
                response.Domain = info.Domain;                
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
                
                if(msg.Reason == LoginRequestMessage.LoginReason.Login)
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

        private DynamicLabelResponseMessage HandleDynamicLabelRequest(DynamicLabelRequestMessage msg)
        {
            switch (msg.Name)
            {
                case "MOTD":
                    string text = Settings.Get.Motd;
                    string motd = FormatMotd(text);
                    return new DynamicLabelResponseMessage() { Name = msg.Name, Text = motd };
                // Others can be added here.
            }
            return null;
        }

        private EmptyMessage HandleLoginInfoChange(LoginInfoChangeMessage msg)
        {
            switch (msg.Change)
            {
                case LoginInfoChangeMessage.ChangeType.Add:
                    {
                        UserInformation userInfo = new UserInformation() { Username = msg.Username, Domain = msg.Domain, Password = msg.Password };
                        m_logger.DebugFormat("Adding userinfo for {0} to session {1}", msg.Username, msg.Session);
                        m_sessionInfoCache.Add(msg.Session, userInfo);
                    }
                    break;
                case LoginInfoChangeMessage.ChangeType.Remove:
                    {
                        m_logger.DebugFormat("Removing userinfo for session {0}", msg.Session);
                        m_sessionInfoCache.Remove(msg.Session);
                    }
                    break;
            }
            return new EmptyMessage(MessageType.Ack);
        }

        private string FormatMotd(string text)
        {
            string motd = text;

            // Version
            string pattern = @"\%v";
            if (Regex.IsMatch(motd, pattern))
            {
                string vers = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                motd = Regex.Replace(motd, pattern, vers);
            }

            // IP Address
            pattern = @"\%i";
            if (Regex.IsMatch(motd, pattern))
            {
                // Get IP address of this computer
                IPAddress[] ipList = Dns.GetHostAddresses("");
                string ip = "";
                // Grab the first IPv4 address in the list
                foreach (IPAddress addr in ipList)
                {
                    if (addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        ip = addr.ToString();
                        break;
                    }
                }
                motd = Regex.Replace(motd, pattern, ip);
            }

            // machine name
            pattern = @"\%m";
            if (Regex.IsMatch(motd, pattern))
            {
                motd = Regex.Replace(motd, pattern, Environment.MachineName);
            }

            // Date
            pattern = @"\%d";
            if (Regex.IsMatch(motd, pattern))
            {
                string today = DateTime.Today.ToString("MMMM dd, yyyy");
                motd = Regex.Replace(motd, pattern, today);
            }

            // DNS name
            pattern = @"\%n";
            if (Regex.IsMatch(motd, pattern))
            {
                string dns = Dns.GetHostName();
                motd = Regex.Replace(motd, pattern, dns);
            }

            return motd;
        }
    }
}
