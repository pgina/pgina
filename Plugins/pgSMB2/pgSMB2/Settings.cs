/*
	Copyright (c) 2016, pGina Team
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

namespace pGina.Plugin.pgSMB2
{
    class Settings
    {
        private static dynamic m_settings = new pGinaDynamicSettings(PluginImpl.PluginUuid);
        private static dynamic m_settings_global = new pGinaDynamicSettings(PluginImpl.PluginUuid, "global");

        static Settings()
        {
            m_settings.SetDefault("SMBshare", @"\\server.my.domain.com\%u");
            m_settings.SetDefault("RoamingSource", @"%s\profile" );
            m_settings.SetDefault("Filename", @"%u.rar" );
            m_settings.SetDefault("TempComp", @"%PUBLIC%" );
            m_settings.SetDefault("ConnectRetry", 3);
            m_settings.SetDefault("Compressor", @"rar.exe" );
            m_settings.SetDefault("UncompressCLI", @"a -m5 -ma5 -md512m -mc -dh -ep1 -r -s -t -tl -ola -ts4 -y -w""%d"" -xntuser.dat.log* -xntuser.dat{* -xAppData\Local\ -xAppData\LocalLow\ -xextensions.log -xsessionstore-*.js -ms7z;rar;ace;cab;zip;bz2;wim;avi;mp4;mkv;ogm;qtm;ts;mpeg;ogg;ac3;ape;wmv;mp3;jpg;gif;png ""%d\%f"" ""%z\.""");
            m_settings.SetDefault("CompressCLI", @"x -y -ola -o+ -ts4 ""%r\%f"" ""%z""");

            m_settings.SetDefault("HomeDir", @"\\server.my.domain.com\%u" );
            m_settings.SetDefault("HomeDirDrive", @"O:" );
            m_settings.SetDefault("ScriptPath", @"\\server.my.domain.com\%u\script\login.cmd");
            m_settings.SetDefault("MaxStore", 0);
            m_settings_global.SetDefault("MaxStoreExclude", @"(?i).*(\\AppData\\)(Local|LocalLow)$");
            m_settings_global.SetDefault("MaxStoreText", new string[] {
                "MaxStoreUserprofile\tProfile storage space",
                "MaxStorefree\tfree",
                "MaxStoreExceeded\texceeded",
                "MaxStoreWarningTitle\tProfile storage space limit soon reached",
                "MaxStoreErrorTitle\tProfile storage space limit exceeded",
                "MaxStoreErrorText\tYou have exceeded your profile storage space.\nBefore you can log off, you need to move some items from your profile to network or local storage.",
                "MaxStoreCalculateText\tcalculate"
            });
            m_settings_global.SetDefault("ACE", "this has to be set by pgina to adapt the user access to this key");
        }

        public static dynamic Store
        {
            get { return m_settings; }
        }
        public static dynamic StoreGlobal
        {
            get { return m_settings_global; }
        }
    }
}
