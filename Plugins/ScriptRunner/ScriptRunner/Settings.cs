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

        public static List<Script> Load()
        {
            dynamic globalSettings = new pGinaDynamicSettings(PluginImpl.ScriptRunnerPluginUuid);
            globalSettings.SetDefault("Timeout", 30000);
            Script.Timeout = globalSettings.Timeout;

            Dictionary<string, dynamic> scripts = pGinaDynamicSettings.GetSubSettings(PluginImpl.ScriptRunnerPluginUuid);
            List<Script> result = new List<Script>();
            foreach (KeyValuePair<string, dynamic> kv in scripts)
            {
                int type = kv.Value.Type;

                string fileName = kv.Value.FileName;
                Script scr = null;
                if (type == (int)ScriptType.BATCH)
                {
                    scr = new BatchScript(fileName);
                }
                else if (type == (int)ScriptType.POWERSHELL)
                {
                    scr = new PowerShellScript(fileName);
                }
                if (scr != null)
                {
                    scr.Uuid = new Guid(kv.Key);
                    int sess = kv.Value.Session;
                    scr.UserSession = (sess & (int)Session.USER) != 0;
                    scr.SystemSession = (sess & (int)Session.SYSTEM) != 0;
                    result.Add(scr);
                }
                else
                {
                    m_logger.ErrorFormat("Unrecognized script type {0}", type);
                }
            }

            return result;
        }

        public static void Save( List<Script> scriptList )
        {
            List<string> guids = new List<string>();
            foreach (Script s in scriptList)
            {
                guids.Add(s.Uuid.ToString());
            }

            pGinaDynamicSettings.CleanSubSettings(PluginImpl.ScriptRunnerPluginUuid, guids);
            Dictionary<string, dynamic> scripts = pGinaDynamicSettings.GetSubSettings(PluginImpl.ScriptRunnerPluginUuid);

            foreach (Script s in scriptList)
            {
                dynamic settings = null;
                if (scripts.ContainsKey(s.Uuid.ToString()))
                {
                    settings = scripts[s.Uuid.ToString()];
                }
                else
                {
                    settings = new pGinaDynamicSettings(PluginImpl.ScriptRunnerPluginUuid, s.Uuid.ToString());
                }

                settings.FileName = s.File;
                if (s is BatchScript)
                {
                    settings.Type = (int)ScriptType.BATCH;
                }
                else if (s is PowerShellScript)
                {
                    settings.Type = (int)ScriptType.POWERSHELL;
                }
                int sess = 0;
                if (s.UserSession)
                    sess |= (int)Session.USER;
                if (s.SystemSession)
                    sess |= (int)Session.SYSTEM;

                settings.Session = sess;
            }
        }
    }
}
