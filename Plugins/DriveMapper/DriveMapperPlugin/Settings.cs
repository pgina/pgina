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
        public static List<DriveEntry> Load()
        {
            Dictionary<string, dynamic> driveSettings = pGinaDynamicSettings.GetSubSettings(DriveMapperPlugin.DriveMapperPluginUuid);
            List<DriveEntry> driveList = new List<DriveEntry>();

            foreach (KeyValuePair<string, dynamic> kv in driveSettings)
            {
                driveList.Add(new DriveEntry
                {
                    Drive = kv.Key,
                    UncPath = kv.Value.UNC,
                    UseAltCreds = kv.Value.UseAltCreds,
                    UserName = kv.Value.Username,
                    Password = kv.Value.GetEncryptedSetting("Password", null)
                });
            }

            return driveList;
        }

        public static void Save( List<DriveEntry> driveList )
        {
            List<string> driveLetterList = new List<string>();
            foreach (DriveEntry d in driveList)
            {
                driveLetterList.Add(d.Drive);
            }

            pGinaDynamicSettings.CleanSubSettings(DriveMapperPlugin.DriveMapperPluginUuid, driveLetterList);
            Dictionary<string, dynamic> driveSettings = pGinaDynamicSettings.GetSubSettings(DriveMapperPlugin.DriveMapperPluginUuid);

            foreach (DriveEntry d in driveList)
            {
                dynamic settings = null;
                try
                {
                    settings = driveSettings[d.Drive];
                }
                catch (KeyNotFoundException)
                {
                    settings = new pGinaDynamicSettings(DriveMapperPlugin.DriveMapperPluginUuid, d.Drive);
                }
                settings.UNC = d.UncPath;
                settings.UseAltCreds = d.UseAltCreds;
                settings.Username = d.UserName;
                settings.SetEncryptedSetting("Password", d.Password, null);
            }
        }
    }
}
