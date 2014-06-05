/*
	Copyright (c) 2014, pGina Team
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
using System.Threading;
using System.Threading.Tasks;

using pGina.Shared.Interfaces;
using Abstractions.WindowsApi;
using System.ComponentModel;
using log4net;
using pGina.Shared.Types;

namespace pGina.Plugin.DriveMapper
{
    public class PluginMain : IPluginEventNotifications, IPluginConfiguration
    {
        private ILog m_logger = LogManager.GetLogger("DriveMapper");      
        public static readonly Guid PluginUuid = new Guid("{534EAABF-AB1F-4B0E-9B28-DC77F3494A78}");

        public void SessionChange(System.ServiceProcess.SessionChangeDescription changeDescription, SessionProperties properties)
        {
            // Check that we're logging on, and that we're configured to do anything
            if (properties != null && changeDescription.Reason == System.ServiceProcess.SessionChangeReason.SessionLogon)
            {
                m_logger.DebugFormat("Attempting to map drive(s) in session: id={0}", changeDescription.SessionId);
                List<DriveMap> maps = Settings.GetMaps();
                foreach( DriveMap map in maps )
                {
                    MapDrive(changeDescription.SessionId, map, properties);
                }
            }
        }

        private void MapDrive(int sessId, DriveMap map, SessionProperties sessProperties)
        {
            // Get user information
            UserInformation userInfo = sessProperties.GetTrackedSingle<UserInformation>();

            // Update the UNC path if there are any place-holders such as (%u or %o)
            map.UncPath = map.UncPath.Replace("%u", userInfo.Username);
            map.UncPath = map.UncPath.Replace("%o", userInfo.OriginalUsername);

            // Check that we're in the right group for this share
            if( ! string.IsNullOrEmpty(map.Group) )
            {
                bool ok = false;
                foreach(GroupInformation gi in userInfo.Groups ) {
                    if( gi.Name.Equals(map.Group, StringComparison.CurrentCultureIgnoreCase) )
                    {
                        ok = true;
                        break;
                    }
                }
                if (!ok)
                {
                    m_logger.DebugFormat("User is not in group {0}, skipping drive map: {1}", map.Group, map.UncPath);
                    return;
                }
            }

            // Set the credentials if necessary
            if (map.Credentials == DriveMap.CredentialOption.Final)
            {
                map.Username = userInfo.Username;
                map.Password = userInfo.Password;
            } 
            else if( map.Credentials == DriveMap.CredentialOption.Original )
            {
                map.Username = userInfo.OriginalUsername;
                map.Password = userInfo.OriginalPassword;
            }

            m_logger.InfoFormat("Mapping '{0}' with username '{1}' to drive '{2}'", 
                map.UncPath, map.Username, map.Drive);

            // Map the drive in another thread, because the thread will do user impersonation
            // and we'd rather not do that in the service's main thread.
            Thread mapThread = new Thread(delegate()
            {
                try
                {                    
                    pInvokes.MapDriveInSession(sessId, map.UncPath, map.Drive, map.Username, map.Password);
                }
                catch (Win32Exception ex)
                {
                    m_logger.ErrorFormat("Failed to map drive {0}: {1}", map.UncPath, ex.Message);
                    m_logger.ErrorFormat("  Win32 error code: {0}", ((Win32Exception)ex).NativeErrorCode);
                }
                catch(Exception ex)
                {
                    m_logger.ErrorFormat("Failed to map drive, unexpected exception: {0}", ex);
                }
            } );

            mapThread.Start();
            mapThread.Join();
        }

        public string Description
        {
            get { return "Map drive(s) upon logon."; }
        }

        public string Name
        {
            get { return "Drive Mapper"; }
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
            get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(); }
        }

        public void Configure()
        {
            ConfigurationUi ui = new ConfigurationUi();
            ui.ShowDialog();
        }
    }
}
