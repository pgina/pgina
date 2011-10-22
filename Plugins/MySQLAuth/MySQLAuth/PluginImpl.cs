using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;

using pGina.Shared.Interfaces;

namespace pGina.Plugin.MySQLAuth
{
    public class PluginImpl : IPluginAuthentication, IPluginConfiguration
    {
        public static readonly Guid PluginUuid = new Guid("{A89DF410-53CA-4FE1-A6CA-4479B841CA19}");
        private ILog m_logger = LogManager.GetLogger("MySQLAuth");

        public Shared.Types.BooleanResult AuthenticateUser(Shared.Types.SessionProperties properties)
        {
            return new Shared.Types.BooleanResult() {Success = false, Message = "Not implemented."};
        }

        public string Description
        {
            get { return "Authenticates users using a MySQL server as the account database."; }
        }

        public string Name
        {
            get { return "MySQL Authentication"; }
        }

        public void Starting()
        {
            
        }

        public void Stopping()
        {
            
        }

        public Guid Uuid
        {
            get { return PluginUuid; }
        }

        public string Version
        {
            get 
            {
                return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public void Configure()
        {
            Configuration dialog = new Configuration();
            dialog.ShowDialog();
        }
    }
}
