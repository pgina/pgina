/*
	Copyright (c) 2012, pGina Team
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
        private ObjectCache<int, SessionProperties> m_sessionPropertyCache = new ObjectCache<int, SessionProperties>();

        static Service()
        {
            try
            {
                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
                Framework.Init();
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("pGina", ex.ToString(), EventLogEntryType.Error);
                throw;
            }
        }

        public static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            try
            {
                ILog logger = LogManager.GetLogger("pGina.Service.Exception");
                Exception e = args.ExceptionObject as Exception;
                logger.ErrorFormat("CurrentDomain_UnhandledException: {0}", e);
            }
            catch
            {
                // don't kill the existing exception stack with one of our own
            }
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
            try
            {
                string pipeName = Core.Settings.Get.ServicePipeName;
                int maxClients = Core.Settings.Get.MaxClients;
                m_logger.DebugFormat("Service created - PipeName: {0} MaxClients: {1}", pipeName, maxClients);
                m_logger.DebugFormat("System Info: {0}", Abstractions.Windows.OsInfo.OsDescription());
                m_server = new PipeServer(pipeName, maxClients, (Func<IDictionary<string, object>, IDictionary<string, object>>)HandleMessage);
                m_logger.DebugFormat("Using plugin directories: ");
                foreach (string dir in PluginDirectories)
                    m_logger.DebugFormat("  {0}", dir); 
            }
            catch (Exception e)
            {
                EventLog.WriteEntry("pGina", e.ToString(), EventLogEntryType.Error);
                m_logger.ErrorFormat("Service startup error: {0}", e.ToString());
                throw;
            }
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

            try
            {
                lock (m_sessionPropertyCache)
                {
                    foreach (IPluginEventNotifications plugin in PluginLoader.GetOrderedPluginsOfType<IPluginEventNotifications>())
                    {
                        try
                        {
                            if (m_sessionPropertyCache.Exists(changeDescription.SessionId))
                                plugin.SessionChange(changeDescription, m_sessionPropertyCache.Get(changeDescription.SessionId));
                            else
                                plugin.SessionChange(changeDescription, null);
                        }
                        catch (Exception e)
                        {
                            m_logger.ErrorFormat("Ignoring unhandled exception from {0}: {1}", plugin.Uuid, e);
                        }
                    }

                    // If this is a logout, remove from our map
                    if (changeDescription.Reason == SessionChangeReason.SessionLogoff && m_sessionPropertyCache.Exists(changeDescription.SessionId))
                        m_sessionPropertyCache.Remove(changeDescription.SessionId);
                }
            }
            catch (Exception e)
            {
                m_logger.ErrorFormat("Exception while handling SessionChange event: {0}", e);
            }
        }        

        // This will be called on seperate threads, 1 per client connection and
        //  represents a connected client - that is, until we return null,
        //  the connection remains open and operations on behalf of this client
        //  should occur in this thread etc.  The current managed thread id 
        //  can be used to differentiate between instances if scope requires.
        private IDictionary<string, object> HandleMessage(IDictionary<string, object> msg)
        {
            int instance = Thread.CurrentThread.ManagedThreadId;
            ILog logger = LogManager.GetLogger(string.Format("HandleMessage[{0}]", instance));
            
            MessageType type = (MessageType) Enum.ToObject(typeof (MessageType), msg["MessageType"]);            

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
                    IDictionary<string, object> disconnectAck = new EmptyMessage(MessageType.Ack).ToDict();  // Ack
                    disconnectAck["LastMessage"] = true;
                    return disconnectAck;
                case MessageType.Hello:
                    return new EmptyMessage(MessageType.Hello).ToDict();  // Ack with our own hello
                case MessageType.Log:
                    HandleLogMessage(new LogMessage(msg));
                    return new EmptyMessage(MessageType.Ack).ToDict();  // Ack
                case MessageType.LoginRequest:
                    return HandleLoginRequest(new LoginRequestMessage(msg)).ToDict();                
                case MessageType.DynLabelRequest:
                    return HandleDynamicLabelRequest(new DynamicLabelRequestMessage(msg)).ToDict();
                case MessageType.LoginInfoChange:
                    return HandleLoginInfoChange(new LoginInfoChangeMessage(msg)).ToDict();
                case MessageType.UserInfoRequest:
                    return HandleUserInfoRequest(new UserInformationRequestMessage(msg)).ToDict();
                case MessageType.ChangePasswordRequest:
                    return HandleChangePasswordRequest(new ChangePasswordRequestMessage(msg)).ToDict();
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
                
        private LoginResponseMessage HandleLoginRequest(LoginRequestMessage msg)
        {
            try
            {
                PluginDriver sessionDriver = new PluginDriver();
                sessionDriver.UserInformation.Username = msg.Username.Trim();
                sessionDriver.UserInformation.Password = msg.Password;

                m_logger.DebugFormat("Processing LoginRequest for: {0} in session: {1} reason: {2}", 
                    sessionDriver.UserInformation.Username, msg.Session, msg.Reason);
                BooleanResult result = sessionDriver.PerformLoginProcess();

                if (msg.Reason == LoginRequestMessage.LoginReason.Login)
                {
                    lock (m_sessionPropertyCache)
                    {
                        m_sessionPropertyCache.Add(msg.Session, sessionDriver.SessionProperties);
                    }
                }

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
            return new DynamicLabelResponseMessage();
        }

        private UserInformationResponseMessage HandleUserInfoRequest(UserInformationRequestMessage msg)
        {
            lock (m_sessionPropertyCache)
            {
                if (m_sessionPropertyCache.Exists(msg.SessionID))
                {
                    SessionProperties props = m_sessionPropertyCache.Get(msg.SessionID);
                    UserInformation userInfo = props.GetTrackedSingle<UserInformation>();

                    return new UserInformationResponseMessage
                    {
                        OriginalUsername = userInfo.OriginalUsername,
                        Username = userInfo.Username,
                        Domain = userInfo.Domain
                    };
                }
            }

            return new UserInformationResponseMessage();
        }

        private EmptyMessage HandleLoginInfoChange(LoginInfoChangeMessage msg)
        {
            m_logger.DebugFormat("Changing login info at request of client, User {0} moving from {1} to {2}", msg.Username, msg.FromSession, msg.ToSession);
            lock (m_sessionPropertyCache)
            {
                if (m_sessionPropertyCache.Exists(msg.FromSession))
                {
                    m_sessionPropertyCache.Add(msg.ToSession, m_sessionPropertyCache.Get(msg.FromSession));
                    m_sessionPropertyCache.Remove(msg.FromSession);
                }
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

        private ChangePasswordResponseMessage HandleChangePasswordRequest(ChangePasswordRequestMessage msg)
        {
            try
            {
                m_logger.DebugFormat("Processing ChangePasswordRequest for: {0} domain: {1}",
                    msg.Username, msg.Domain);

                ChangePasswordInfo cpInfo = new ChangePasswordInfo()
                {
                    Username = msg.Username,
                    Domain = msg.Domain,
                    OldPassword = msg.OldPassword,
                    NewPassword = msg.NewPassword
                };

                ChangePasswordPluginActivityInfo pluginInfo = new ChangePasswordPluginActivityInfo();
                pluginInfo.LoadedPlugins = PluginLoader.GetOrderedPluginsOfType<IPluginChangePassword>();
                BooleanResult finalResult = new BooleanResult { Success = false, Message = "" };

                // One success means the final result is a success, and we return the message from
                // the last success.  Otherwise, we return the message from the last failure.
                foreach ( IPluginChangePassword plug in PluginLoader.GetOrderedPluginsOfType<IPluginChangePassword>() ) 
                {
                    // Execute the plugin
                    m_logger.DebugFormat("ChangePassword: executing {0}", plug.Uuid);
                    BooleanResult pluginResult = plug.ChangePassword(cpInfo, pluginInfo);

                    // Add result to our list of plugin results
                    pluginInfo.AddResult(plug.Uuid, pluginResult);

                    m_logger.DebugFormat("ChangePassword: result from {0} is {1} message: {2}",
                        plug.Uuid, pluginResult.Success, pluginResult.Message);

                    if (pluginResult.Success)
                    {
                        finalResult.Success = true;
                        finalResult.Message = pluginResult.Message;
                    }
                    else
                    {
                        if (!finalResult.Success)
                        {
                            finalResult.Message = pluginResult.Message;
                        }
                    }
                }

                m_logger.DebugFormat("ChangePassword: returning final result {0}, message {1}",
                    finalResult.Success, finalResult.Message);

                return new ChangePasswordResponseMessage()
                {
                    Result = finalResult.Success,
                    Message = finalResult.Message,
                    Username = msg.Username,
                    Domain = msg.Domain
                };
            }
            catch (Exception e)
            {
                m_logger.ErrorFormat("Internal error, unexpected exception while handling change password request: {0}", e);
                return new ChangePasswordResponseMessage() { Result = false, Message = "Internal error" };
            }
        }
    }
}
