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
using log4net;

namespace pGina.Plugin.ScriptRunner
{
    class Settings
    {
        private static ILog m_logger = LogManager.GetLogger("ScriptRunner Settings");

        enum ScriptType
        {
            BATCH = 0, POWERSHELL
        }

        enum Session
        {
            USER = 0x01, SYSTEM = 0x02
        }

        static dynamic m_settings = new pGinaDynamicSettings( PluginImpl.ScriptRunnerPluginUuid );

        public static List<Script> Load()
        {
            // Set defaults
            m_settings.SetDefault("Files", new string[] { });
            m_settings.SetDefault("Types", new string[] { });
            m_settings.SetDefault("Sessions", new string[] { });
            m_settings.SetDefault("Timeout", 30000);

            string[] fileNames = m_settings.Files;
            string[] types = m_settings.Types;
            string[] sessionData = m_settings.Sessions;
            Script.Timeout = m_settings.Timeout;

            List<Script> list = new List<Script>();

            for (int i = 0; i < fileNames.Count(); i++)
            {
                int type = Convert.ToInt32(types[i]);
                Script scr = null;
                if (type == (int)ScriptType.BATCH)
                {
                    scr = new BatchScript(fileNames[i]);
                }
                else if (type == (int)ScriptType.POWERSHELL)
                {
                    scr = new PowerShellScript(fileNames[i]);
                }
                if (scr != null)
                {
                    int sess = Convert.ToInt32(sessionData[i]);
                    scr.UserSession = (sess & (int)Session.USER) != 0;
                    scr.SystemSession = (sess & (int)Session.SYSTEM) != 0;
                    list.Add(scr);
                }
                else
                {
                    m_logger.ErrorFormat("Unrecognized script type {0}", type);
                }
            }

            return list;
        }

        public static void Save( List<Script> scriptList )
        {
            string[] fileNames = new string[scriptList.Count];
            string[] types = new string[scriptList.Count];
            string[] sessionData = new string[scriptList.Count];

            for (int i = 0; i < scriptList.Count; i++ )
            {
                Script scr = scriptList[i];
                fileNames[i] = scr.File;
                if (scr is BatchScript)
                {
                    types[i] = Convert.ToString((int)ScriptType.BATCH);
                }
                else if (scr is PowerShellScript)
                {
                    types[i] = Convert.ToString((int)ScriptType.POWERSHELL);
                }
                int sess = 0;
                if (scr.UserSession)
                    sess |= (int)Session.USER;
                if (scr.SystemSession)
                    sess |= (int)Session.SYSTEM;

                sessionData[i] = Convert.ToString(sess);
            }

            m_settings.Files = fileNames;
            m_settings.Types = types;
            m_settings.Sessions = sessionData;
            m_settings.Timeout = Script.Timeout;
        }
    }
}
