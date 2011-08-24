using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;

using pGina.Shared;
using pGina.Core;
using pGina.Core.Messages;
using Abstractions;
using Abstractions.Logging;
using Abstractions.Pipes;

using log4net;

namespace SessionHelper
{
    class Program
    {
        private static ILog m_logger = LogManager.GetLogger("pGina.Service.SessionHelper");
        private static ILog m_abstractLogger = LogManager.GetLogger("Abstractions");
        private static bool m_serviceMode = false;        
        private static PluginDriver m_driver = new PluginDriver();

        static void Main(string[] args)
        {
            Framework.Init();            

            LibraryLogging.AddListener(LibraryLogging.Level.Debug, m_abstractLogger.DebugFormat);
            LibraryLogging.AddListener(LibraryLogging.Level.Error, m_abstractLogger.ErrorFormat);
            LibraryLogging.AddListener(LibraryLogging.Level.Info, m_abstractLogger.InfoFormat);
            LibraryLogging.AddListener(LibraryLogging.Level.Warn, m_abstractLogger.WarnFormat);

            using (Process me = Process.GetCurrentProcess())
            {
                m_logger.DebugFormat("Session helper {0} started in session {1}", me.Id, me.SessionId);
            }

            foreach (string arg in args)
            {
                switch(arg)
                {
                    case "--serviceMode":
                        m_serviceMode = true;
                        break;                             
                }                
            }

            // Use named pipe to get username, domain and password
            // TBD: serialize and pass the entire UserInformation object?
            GetDetailsFromService();
            
            Application.ApplicationExit += new EventHandler(OnApplicationExit);

            if (m_serviceMode)            
                m_driver.InvokeSystemSessionHelpers();
            else
                m_driver.InvokeUserSessionHelpers();

            // Infinite message loop looking for close/exit from logoff
            Application.Run();
        }

        static void OnApplicationExit(object sender, EventArgs e)
        {
            if (m_serviceMode)
                m_driver.EndSystemSessionHelpers();
            else
                m_driver.EndUserSessionHelpers();
        }

        static void GetDetailsFromService()
        {
            try
            {
                string pipeName = pGina.Core.Settings.Get.ServicePipeName;
                PipeClient client = new PipeClient(pipeName);
                client.Start(
                    (Func<dynamic, dynamic>)
                    ((m) =>
                    {
                        MessageType type = (MessageType)m.MessageType;

                        // Acceptable server responses are Hello, and InfoResponse
                        switch (type)
                        {
                            case MessageType.Hello:
                                // Send our info request
                                using (Process me = Process.GetCurrentProcess())
                                {
                                    HelperInfoRequestMessage requestMsg = new HelperInfoRequestMessage() { Key = me.SessionId };
                                    return requestMsg.ToExpando();
                                }                                
                            case MessageType.InfoResponse:
                                HelperInfoResponseMessage responseMsg = new HelperInfoResponseMessage(m);
                                if (responseMsg.Success)
                                {
                                    m_driver.UserInformation.Username = responseMsg.Username;
                                    m_driver.UserInformation.Domain = responseMsg.Domain;
                                    m_driver.UserInformation.Password = responseMsg.Password;
                                    m_logger.DebugFormat("Server responded with user '{0}' info", m_driver.UserInformation.Username);
                                }                                
                                // Respond with a disconnect, we're done
                                return (new EmptyMessage(MessageType.Disconnect).ToExpando());
                            case MessageType.Ack:   // Ack to our disconnect
                                return null;
                            default:
                                m_logger.ErrorFormat("Server responded with invalid message type: {0}", type);
                                return null;
                        }
                    }),
                (new EmptyMessage(MessageType.Hello)).ToExpando(), 1000);
            }
            catch (Exception e)
            {
                m_logger.ErrorFormat("Error while communicating with service: {0}", e);
            }
        }
    }
}
