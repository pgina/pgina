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

using log4net;
using pGina.Shared.Interfaces;

namespace pGina.Plugin.ScriptRunner
{
    public class PluginImpl : IPluginUserSessionHelper, IPluginSystemSessionHelper, IPluginConfiguration
    {
        public static readonly Guid ScriptRunnerPluginUuid = new Guid("1632D7AB-858F-4D88-9603-AADD4DEEA847");
        private ILog m_logger = LogManager.GetLogger("ScriptRunnerPlugin");

        void IPluginSystemSessionHelper.SessionEnding(Shared.Types.SessionProperties properties)
        {
            m_logger.DebugFormat("IPluginSystemSessionHelper.SessionEnding");
        }

        void IPluginSystemSessionHelper.SessionStarted(Shared.Types.SessionProperties properties)
        {
            m_logger.DebugFormat("IPluginSystemSessionHelper.SessionStarted");

            List<Script> scriptList = Settings.Load();
            foreach (Script scr in scriptList)
            {
                if (scr.SystemSession)
                    scr.Run();
            }
        }

        void IPluginUserSessionHelper.SessionEnding(Shared.Types.SessionProperties properties)
        {
            m_logger.DebugFormat("IPluginUserSessionHelper.SessionEnding");
        }

        void IPluginUserSessionHelper.SessionStarted(Shared.Types.SessionProperties properties)
        {
            m_logger.DebugFormat("IPluginUserSessionHelper.SessionStarted");

            List<Script> scriptList = Settings.Load();
            foreach (Script scr in scriptList)
            {
                if (scr.UserSession)
                    scr.Run();
            }
        }

        public string Description
        {
            get { return "Plugin for running scripts"; }
        }

        public string Name
        {
            get { return "Script Runner"; }
        }

        public Guid Uuid
        {
            get { return ScriptRunnerPluginUuid; }
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
            Configuration dlg = new Configuration();
            dlg.ShowDialog();
        }

        public void Starting() { }
        public void Stopping() { }
    }
}
