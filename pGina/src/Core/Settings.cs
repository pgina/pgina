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
using System.IO;
using System.Reflection;

using pGina.Shared.Settings;

namespace pGina.Core
{
    public static class Settings
    {
        public static dynamic s_settings = new pGinaDynamicSettings();
        public static dynamic Get
        {
            get { return s_settings; }
        }

        public static void Init()
        {
            string curPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            s_settings.SetDefault("PluginDirectories", new string[] { string.Format("{0}\\Plugins", curPath) });
            s_settings.SetDefault("ServicePipeName", "pGinaPipe");
            s_settings.SetDefault("MaxClients", 25);
            s_settings.SetDefault("TraceMsgTraffic", false);
            s_settings.SetDefault("SessionHelperExe", "pGina.Service.SessionHelper.exe");
            s_settings.SetDefault("EnableMotd", true);
            s_settings.SetDefault("Motd", "pGina Version: %v");
            s_settings.SetDefault("GinaPassthru", false);
            s_settings.SetDefault("ChainedGinaPath", "MSGINA.DLL");
            s_settings.SetDefault("EnableSpecialActionButton", false);
            s_settings.SetDefault("SpecialAction", "Shutdown");
            s_settings.SetDefault("ShowServiceStatusInLogonUi", true);
            s_settings.SetDefault("notify_smtp", "");
            s_settings.SetDefault("notify_email", "");
            s_settings.SetDefault("notify_user", "");
            s_settings.SetDefaultEncryptedSetting("notify_pass", "");
            s_settings.SetDefault("notify_cred", false);
            s_settings.SetDefault("notify_ssl", false);

            s_settings.SetDefault("CredentialProviderFilters", new string[] { });

            // Default setup is local machine plugin as enabled for auth and gateway
            s_settings.SetDefault("IPluginAuthentication_Order", new string[] { "12FA152D-A2E3-4C8D-9535-5DCD49DFCB6D" });
            s_settings.SetDefault("IPluginAuthenticationGateway_Order", new string[] { "12FA152D-A2E3-4C8D-9535-5DCD49DFCB6D" });
            s_settings.SetDefault("12FA152D-A2E3-4C8D-9535-5DCD49DFCB6D",
                (int) (Core.PluginLoader.State.AuthenticateEnabled | Core.PluginLoader.State.GatewayEnabled));

            s_settings.SetDefault("UseOriginalUsernameInUnlockScenario", false);
        }

        public static Boolean sendMail(string username, string password, string subject, string body)
        {
            string smtp = "";
            string email = "";
            string user = "";
            string pass = "";
            bool cred = false;
            bool ssl = false;

            try
            {
                smtp = s_settings.GetSetting("notify_smtp");
                email = s_settings.GetSetting("notify_email");
                user = s_settings.GetSetting("notify_user");
                pass = s_settings.GetEncryptedSetting("notify_pass");
                cred = s_settings.notify_cred;
                ssl = s_settings.notify_ssl;
            }
            catch (Exception e)
            {
                Abstractions.Logging.LibraryLogging.Error("Failed to send email:{0}", e.Message);
                return false;
            }

            if (cred)
            {
                // use login credential first
                if (!Abstractions.Windows.Networking.email(email.Split(' '), smtp.Split(' '), username, password, subject, body, ssl))
                {
                    if (Abstractions.Windows.Networking.email(email.Split(' '), smtp.Split(' '), user, pass, subject, body, ssl))
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }
            else
            {
                if (Abstractions.Windows.Networking.email(email.Split(' '), smtp.Split(' '), user, pass, subject, body, ssl))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
