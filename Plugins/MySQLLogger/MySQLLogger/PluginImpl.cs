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
using pGina.Shared.Settings;
using log4net;

namespace pGina.Plugin.MySqlLogger
{
    public class PluginImpl : IPluginSystemSessionHelper, IPluginConfiguration
    {
        public static readonly Guid PluginUuid = new Guid("B68CF064-9299-4765-AC08-ACB49F93F892");
        private ILog m_logger = LogManager.GetLogger("MySqlLoggerPlugin");

        public static readonly string TABLE_NAME = "pGinaLog";

        private static dynamic m_settings = null;
        internal static dynamic Settings
        {
            get { return m_settings; }
        }

        static PluginImpl()
        {
            m_settings = new DynamicSettings(PluginUuid);
            // Set defaults
            m_settings.SetDefault("Host", "localhost");
            m_settings.SetDefault("Port", 3306);
            m_settings.SetDefault("User", "pGina");
            m_settings.SetDefault("Password", "secret");
            m_settings.SetDefault("Database", "pGinaDB");
        }

        public void SessionEnding(Shared.Types.SessionProperties properties)
        {
            m_logger.Debug("SessionEnding()");
        }

        public void SessionStarted(Shared.Types.SessionProperties properties)
        {
            m_logger.Debug("SessionStarted()");

            Shared.Types.UserInformation userInfo = properties.GetTrackedSingle<Shared.Types.UserInformation>();
            try
            {
                using (DbLogger db = DbLogger.Connect())
                {
                    db.Log(String.Format("Login: user={0}", userInfo.Username));
                }
            }
            catch (Exception e)
            {
                m_logger.ErrorFormat("{0}: {1}", e.GetType().ToString(), e.Message);
            }
        }

        public string Description
        {
            get { return "Logs various events to a MySQL database."; }
        }

        public string Name
        {
            get { return "MySQL Logger"; }
        }

        public Guid Uuid
        {
            get { return PluginUuid; }
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
    }
}
