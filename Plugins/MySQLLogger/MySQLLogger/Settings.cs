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

namespace pGina.Plugin.MySqlLogger
{
    

    class Settings
    {
        private static dynamic m_settings = new pGinaDynamicSettings(PluginImpl.PluginUuid);
        public static dynamic Store
        {
            get { return m_settings; }
        }

        static Settings()
        {
            // Set defaults
            //m_settings.SetDefault("LoggerMode", LoggerMode.EVENT);
            m_settings.SetDefault("EventMode", true);
            m_settings.SetDefault("SessionMode", false);
            m_settings.SetDefault("Host", "localhost");
            m_settings.SetDefault("Port", 3306);
            m_settings.SetDefault("User", "pGina");
            m_settings.SetDefaultEncryptedSetting("Password", "secret", null);
            m_settings.SetDefault("Database", "pGinaDB");
            m_settings.SetDefault("SessionTable", "");
            m_settings.SetDefault("EventTable", "pGinaLog");

            m_settings.SetDefault("EvtLogon", true);
            m_settings.SetDefault("EvtLogoff", true);
            m_settings.SetDefault("EvtLock", false);
            m_settings.SetDefault("EvtUnlock", false);
            m_settings.SetDefault("EvtConsoleConnect",false);
            m_settings.SetDefault("EvtConsoleDisconnect", false);
            m_settings.SetDefault("EvtRemoteControl", false);
            m_settings.SetDefault("EvtRemoteConnect", false);
            m_settings.SetDefault("EvtRemoteDisconnect", false);

            m_settings.SetDefault("UseModifiedName", false);
        }

    }
}
