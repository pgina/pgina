using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using log4net;

using pGina.Interfaces;

namespace pGina.Plugin.Sample
{
    public class SimplePlugin : IPluginAuthenticationUI, IPluginAuthentication
    {
        private ILog m_logger = LogManager.GetLogger("SimplePlugin");
        private Guid m_descriptionGuid = new Guid("{22B06063-DEF1-4CE2-96DC-C6FDB409FEFD}");        

        public SimplePlugin()
        {
            using(Process me = Process.GetCurrentProcess())
            {
                m_logger.DebugFormat("Plugin initialized on {0} in PID: {1} Session: {2}", Environment.MachineName, me.Id, me.SessionId);
            }
        }

        public void SetupUI(List<Interfaces.AuthenticationUI.Element> elements)
        {
            try
            {
                // Must have the username|password element            
                if (elements.Where(element => element.UUid == Interfaces.AuthenticationUI.EditTextElement.UsernameElement.UUid).Count() == 0)
                {
                    m_logger.DebugFormat("SetupUI: Adding username element");
                    elements.Add(Interfaces.AuthenticationUI.EditTextElement.UsernameElement);
                }

                if (elements.Where(element => element.UUid == Interfaces.AuthenticationUI.PasswordTextElement.PasswordElement.UUid).Count() == 0)
                {
                    m_logger.DebugFormat("SetupUI: Adding password element");
                    elements.Add(Interfaces.AuthenticationUI.PasswordTextElement.PasswordElement);
                }

                elements.Add(
                    new Interfaces.AuthenticationUI.SmallTextElement("Description", "A simple demonstration, enter a username that starts with 'p' to login") 
                    { 
                        UUid = m_descriptionGuid 
                    }
                );
            }
            catch (Exception e)
            {
                m_logger.ErrorFormat("SetupUI exception: {0}", e);
            }
        }

        public string Name
        {
            get { return "Simple Demonstration Plugin"; }
        }

        public string Description
        {
            get { return "A demonstration plugin that allows all usernames that begin with the letter \"p\" to succeed."; }
        }

        public Guid Uuid
        {
            get { return new Guid("{16FC47C0-F17B-4D99-A820-EDBF0B0C764A}"); }
        }

        public bool AuthenticateUser(Interfaces.AuthenticationUI.Element[] values, Guid trackingToken)
        {
            try
            {
                m_logger.DebugFormat("AuthenticateUser(..., {0})", trackingToken.ToString());

                // Dump UI elements, cuz we can
                foreach (Interfaces.AuthenticationUI.Element e in values)
                {
                    m_logger.DebugFormat("Element[{0}]: {1} => {2}", e.UUid, e.Type, e.Name);
                }

                Interfaces.AuthenticationUI.EditTextElement usernameField = (Interfaces.AuthenticationUI.EditTextElement)
                    values.First(v => v.UUid == Interfaces.AuthenticationUI.Constants.UsernameElementUuid);

                m_logger.DebugFormat("Found username: {0}", usernameField.Text);
                if (usernameField.Text.StartsWith("p"))
                {
                    m_logger.InfoFormat("Authenticated user: {0}", usernameField.Text);
                    return true;
                }

                m_logger.ErrorFormat("Failed to authenticate user: {0}", usernameField.Text);
                return false;
            }
            catch (Exception e)
            {
                m_logger.ErrorFormat("AuthenticateUser exception: {0}", e);
                return false;
            }
        }
    }
}
