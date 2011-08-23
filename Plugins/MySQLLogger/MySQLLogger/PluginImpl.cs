using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using pGina.Shared.Interfaces;
using log4net;

namespace pGina.Plugin.MySQLLogger
{
    public class PluginImpl : IPluginSystemSessionHelper, IPluginConfiguration
    {
        public static readonly Guid PluginUuid = new Guid("B68CF064-9299-4765-AC08-ACB49F93F892");
        private ILog m_logger = LogManager.GetLogger("MySQLLoggerPlugin");

        public void SessionEnding()
        {
            m_logger.Debug("SessionEnding()");
        }

        public void SessionStarted(Shared.Types.UserInformation userInformation)
        {
            m_logger.Debug("SessionStarted()");
        }

        public string Description
        {
            get { return "Logs various events to a MySQL database."; }
        }

        public string Name
        {
            get { return "MySQL Logger"; }
        }

        public Guid Uuid
        {
            get { return PluginUuid; }
        }

        public string Version
        {
            get { return "1.0.0"; }
        }

        public void Configure()
        {
            throw new NotImplementedException();
        }
    }
}
