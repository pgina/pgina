using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using log4net;

using pGina.Shared.AuthenticationUI;
using pGina.Shared.Interfaces;
using pGina.Shared.Types;
using pGina.Shared.Settings;

namespace pGina.Plugin.Sample
{
    public class SimplePlugin : IPluginConfiguration, IPluginAuthenticationUI, IPluginAuthentication
    {
        private ILog m_logger = LogManager.GetLogger("SimplePlugin");
        private Guid m_descriptionGuid = new Guid("{22B06063-DEF1-4CE2-96DC-C6FDB409FEFD}");
        public static Guid SimpleUuid = new Guid("{16FC47C0-F17B-4D99-A820-EDBF0B0C764A}");
        private string m_defaultDescription = "A demonstration plugin that allows all usernames that begin with the letter \"p\" to succeed.";
        private dynamic m_settings = null;

        public SimplePlugin()
        {
            using(Process me = Process.GetCurrentProcess())
            {
                m_settings = new DynamicSettings(SimpleUuid);
                m_settings.SetDefault("ShowDescription", true);
                m_settings.SetDefault("Description", m_defaultDescription);
                
                m_logger.DebugFormat("Plugin initialized on {0} in PID: {1} Session: {2}", Environment.MachineName, me.Id, me.SessionId);
            }
        }

        public void SetupUI(List<Element> elements)
        {
            try
            {
                // Must have the username|password element            
                if (elements.Where(element => element.UUid == EditTextElement.UsernameElement.UUid).Count() == 0)
                {
                    m_logger.DebugFormat("SetupUI: Adding username element");
                    elements.Add(EditTextElement.UsernameElement);
                }

                if (elements.Where(element => element.UUid == PasswordTextElement.PasswordElement.UUid).Count() == 0)
                {
                    m_logger.DebugFormat("SetupUI: Adding password element");
                    elements.Add(PasswordTextElement.PasswordElement);
                }

                if (m_settings.ShowDescription)
                {
                    elements.Add(
                        new SmallTextElement("Description", "A simple demonstration, enter a username that starts with 'p' to login")
                        {
                            UUid = m_descriptionGuid
                        }
                    );
                }
            }
            catch (Exception e)
            {
                m_logger.ErrorFormat("SetupUI exception: {0}", e);
            }
        }

        public string Name
        {
            get { return "Simple Demonstration"; }
        }

        public string Description
        {
            get { return m_settings.Description; }
        }

        public string Version
        {
            get { return "1.0.0"; }
        }

        public Guid Uuid
        {
            get { return SimpleUuid; }
        }
        
        BooleanResult IPluginAuthentication.AuthenticateUser(Element[] values, Guid trackingToken)
        {
            try
            {
                m_logger.DebugFormat("AuthenticateUser(..., {0})", trackingToken.ToString());

                // Dump UI elements, cuz we can
                foreach (Element e in values)
                {
                    m_logger.DebugFormat("Element[{0}]: {1} => {2}", e.UUid, e.Type, e.Name);
                }

                EditTextElement usernameField = (EditTextElement)
                    values.First(v => v.UUid == Constants.UsernameElementUuid);

                m_logger.DebugFormat("Found username: {0}", usernameField.Text);
                if (usernameField.Text.StartsWith("p"))
                {
                    m_logger.InfoFormat("Authenticated user: {0}", usernameField.Text);
                    return new BooleanResult() { Success = true };
                }

                m_logger.ErrorFormat("Failed to authenticate user: {0}", usernameField.Text);
                return new BooleanResult() { Success = false };
            }
            catch (Exception e)
            {
                m_logger.ErrorFormat("AuthenticateUser exception: {0}", e);
                throw;  // Allow pGina service to catch and handle exception
            }
        }

        public void Configure()
        {
            Configuration conf = new Configuration();
            conf.ShowDialog();
        }
    }
}
