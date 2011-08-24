using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using pGina.Shared.Interfaces;
using Abstractions;
using log4net;
using System.Text.RegularExpressions;
using Abstractions.WindowsApi;

namespace pGina.Plugin.DriveMapper
{
    public class DriveMapperPlugin : IPluginUserSessionHelper, IPluginConfiguration
    {
        public static readonly Guid DriveMapperPluginUuid = new Guid("989C0EC5-B025-431B-8966-E276864A97A9");
        private ILog m_logger = LogManager.GetLogger("DriveMapperPlugin");

        public void SessionEnding(Shared.Types.SessionProperties properties)
        {
            m_logger.Debug("SessionEnding( ... )");
        }

        public void SessionStarted(Shared.Types.SessionProperties properties)
        {
            m_logger.Debug("SessionStarted( ... )");
            MapDrives(Settings.Load(), properties.GetTrackedSingle<Shared.Types.UserInformation>());
        }

        private void MapDrives(List<DriveEntry> drives, Shared.Types.UserInformation userInfo)
        {
            foreach (DriveEntry drive in drives)
            {
                string unc = Regex.Replace(drive.UncPath, @"\%u", userInfo.Username);
                m_logger.DebugFormat("Attempting to map {0} to {1}", unc, drive.Drive);
                string user = null;
                string password = null;
                if (drive.UseAltCreds)
                {
                    user = drive.UserName;
                    password = drive.Password;
                }

                int result = pInvokes.MapNetworkDrive(unc, drive.Drive, user, password);
                if (0 == result)
                {
                    m_logger.InfoFormat("Drive {0} mapped successfully to drive letter {1}", unc, drive.Drive);
                }
                else
                {
                    m_logger.ErrorFormat("Drive mapping failed for {0} to letter {1}.  Message: {2}",
                        unc, drive.Drive, GetMappingErrorMessage(result));
                }
            }
        }

        private string GetMappingErrorMessage(int result)
        {
            switch (result)
            {
                case 5:
                    return "Access denied.";
                case 66:
                    return "The network resource type is not correct.";
                case 67:
                    return "The network name cannot be found.";
                case 85:
                    return "Drive letter is already assigned";
                case 86:
                    return "The password is incorrect.";
                default:
                    return "Unkown (" + result + ")";
            }
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

        public void Configure()
        {
            Configuration conf = new Configuration();
            conf.ShowDialog();
        }
    }
}
