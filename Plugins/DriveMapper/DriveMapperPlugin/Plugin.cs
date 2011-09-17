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
                bool groupCheckOk = true;
                foreach (string group in drive.Groups)
                {
                    if (!userInfo.Groups.Any( g => g.Name == group ) )
                    {
                        m_logger.DebugFormat("User is not a member of {0}", group);
                        groupCheckOk = false;
                        break;
                    }
                }

                if (groupCheckOk)
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
                else
                {
                    m_logger.DebugFormat("User not in all required groups, not mapping drive {0}", drive.Drive);
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
            get
            {
                return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public void Configure()
        {
            Configuration conf = new Configuration();
            conf.ShowDialog();
        }
    }
}
