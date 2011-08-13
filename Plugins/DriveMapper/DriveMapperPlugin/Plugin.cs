using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using pGina.Shared.Interfaces;
using Abstractions;
using log4net;

namespace pGina.Plugin.DriveMapper
{
    public class DriveMapperPlugin : IPluginUserSessionHelper
    {
        public static readonly Guid DriveMapperPluginUuid = new Guid("989C0EC5-B025-431B-8966-E276864A97A9");
        private ILog m_logger = LogManager.GetLogger("DriveMapperPlugin");

        public void SessionEnding()
        {
            m_logger.Debug("SessionEnding( ... )");
        }

        public void SessionStarted(Shared.Types.UserInformation userInformation)
        {
            m_logger.Debug("SessionStarted( ... )");

        }

        public string Description
        {
            get { return "Plugin for mapping drives after login."; }
        }

        public string Name
        {
            get { return "Drive Mapper"; }
        }

        public Guid Uuid
        {
            get { return DriveMapperPluginUuid; }
        }

        public string Version
        {
            get { return "1.0.0"; }
        }
    }
}
