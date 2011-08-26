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
using pGina.Shared.Settings;

namespace pGina.Plugin.DriveMapper
{
    class DriveEntry
    {
        public string Drive { get; set; }
        public string UncPath { get; set; }
        public bool UseAltCreds { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    class Settings
    {
        private static dynamic m_settings = new DynamicSettings(DriveMapperPlugin.DriveMapperPluginUuid);

        public static List<DriveEntry> Load()
        {
            List<DriveEntry> driveList = new List<DriveEntry>();

            // Set defaults in case the data is not there yet.
            m_settings.SetDefault("Drives", new string[] { });
            m_settings.SetDefault("UncPaths", new string[] { });
            m_settings.SetDefault("UseAlternateCreds", new string[] { });
            m_settings.SetDefault("Usernames", new string[] { });
            m_settings.SetDefault("Passwords", new string[] { });

            // Load the settings
            string[] drives = m_settings.Drives;
            string[] paths = m_settings.UncPaths;
            string[] useCreds = m_settings.UseAlternateCreds;
            string[] usernameList = m_settings.Usernames;
            string[] passList = m_settings.Passwords;

            for (int i = 0; i < drives.Count(); i++ )
            {
                driveList.Add(new DriveEntry
                {
                    Drive = drives[i],
                    UncPath = paths[i],
                    UseAltCreds = useCreds[i] != "0",
                    UserName = usernameList[i],
                    Password = passList[i]
                });
            }

            return driveList;
        }

        public static void Save( List<DriveEntry> driveList )
        {
            List<string> driveLetterList = new List<string>();
            List<string> uncPathList = new List<string>();
            List<string> useCredsList = new List<string>();
            List<string> usernameList = new List<string>();
            List<string> passList = new List<string>();
            foreach (DriveEntry d in driveList)
            {
                driveLetterList.Add(d.Drive);
                uncPathList.Add(d.UncPath);
                useCredsList.Add(d.UseAltCreds ? "1" : "0");
                usernameList.Add(d.UserName);
                passList.Add(d.Password);
            }
            m_settings.Drives = driveLetterList.ToArray();
            m_settings.UncPaths = uncPathList.ToArray();
            m_settings.UseAlternateCreds = useCredsList.ToArray();
            m_settings.Usernames = usernameList.ToArray();
            m_settings.Passwords = passList.ToArray();
        }
    }
}
