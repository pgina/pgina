﻿/*
	Copyright (c) 2012, pGina Team
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

namespace pGina.Plugin.pgSMB
{
    class Settings
    {
        private static dynamic m_settings = new pGinaDynamicSettings(PluginImpl.PluginUuid);

        static Settings()
        {
            m_settings.SetDefault("SMBshare", @"\\server.my.domain.com\%u");
            m_settings.SetDefault("RoamingSource", @"\\server.my.domain.com\%u\profile" );
            m_settings.SetDefault("Filename", @"%u.wim" );
            m_settings.SetDefault("RoamingDest", @"%PUBLIC%\%u" );
            m_settings.SetDefault("ConnectRetry", 3);
            m_settings.SetDefault("Compressor", @"imagex.exe" );
            m_settings.SetDefault("UncompressCLI", @"/APPLY ""\\server.my.domain.com\%u\profile\%u.wim"" 1 ""%PUBLIC%\%u""");
            m_settings.SetDefault("CompressCLI", @"/CAPTURE ""%PUBLIC%\%u"" ""%PUBLIC%\%u\%u.wim"" 1");

            m_settings.SetDefault("HomeDir", @"\\server.my.domain.com\%u" );
            m_settings.SetDefault("HomeDirDrive", @"O:" );
            m_settings.SetDefault("ScriptPath", @"\\server.my.domain.com\%u\scripts" );
            m_settings.SetDefault("MaxStore", 0);

            m_settings.SetDefault("ntp", @"ts1.my.domain.com ts2.my.domain.com");
            m_settings.SetDefault("email", @"mymail@my.domain.com anothermail@my.domain.com");
            m_settings.SetDefault("smtp", @"smtp.my.domain.com smtp2.my.domain.com");
        }

        public static dynamic Store
        {
            get { return m_settings; }
        }
    }
}
