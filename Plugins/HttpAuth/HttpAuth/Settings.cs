using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using pGina.Shared.Settings;
using log4net;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace pGina.Plugin.HttpAuth
{
    class Settings
    {
        private static dynamic m_settings = new pGinaDynamicSettings(PluginImpl.PluginUuid);
        private static ILog m_logger = LogManager.GetLogger("HttpAuthSettings");
        private static string DEFAULT_URL = "https://pginaloginserver/login";

        static Settings()
        {
            try
            {
                m_settings.SetDefault("Loginserver", @DEFAULT_URL);
            }
            catch (Exception)
            {
                // do nothing
            }    
        }

        public static dynamic Store
        {
            get { return m_settings; }
        }

        public static string resolveSettings()
        {
            string loginServer = _urlByEnvVar();
            if (loginServer == null)
            {
                // try to get URL from DNS
                try
                {
                    List<string> entries = _getTxtRecords("pginaloginserver");
                    if (entries.Count > 0)
                    {
                        loginServer = entries[0].ToString();    // gets the first item
                        m_logger.DebugFormat("Login server from DNS: {0}", loginServer);
                        _persist(loginServer);
                    }
                    else
                    {
                        loginServer = m_settings.Loginserver;
                        m_logger.DebugFormat("Login server from GinaSettings: {0}", loginServer);
                    }
                }
                catch (KeyNotFoundException)
                {
                    loginServer = DEFAULT_URL;
                    m_logger.DebugFormat("default Login server url: {0}", loginServer);
                }
                catch (Exception dnsex)
                {
                    m_logger.ErrorFormat("Response: {0}", dnsex.ToString());
                    loginServer = m_settings.Loginserver;
                    m_logger.DebugFormat("Login server from GinaSettings: {0}", loginServer);
                }
            }
            else
            {
                m_logger.DebugFormat("Login server from ENVVar: {0}", loginServer);
                _persist(loginServer);
            }
            return loginServer;
        }

        private static void _persist(string url) {
            try
            {
                m_settings.SetSetting("Loginserver", url);
            }
            catch (Exception e)
            {
                m_logger.ErrorFormat("Cannot save settings: {0}", e.ToString());
            }
        }

        /*
         * returns PGINALOGINSERVER environment variable content if set, otherwise null.
         * Setting by environment variable allows easy override of login endpoint address.
         */
        private static string _urlByEnvVar()
        {
            try
            {
                return Environment.GetEnvironmentVariable("PGINALOGINSERVER");
            }
            catch (Exception)
            {
                return null;
            }
        }

        /*
         * Uses http://www.robertsindall.co.uk/blog/getting-dns-txt-record-using-c-sharp/
         * because c# and whole M$osft is crap and has no tools to resolve TXT recs!!!
         */
        private static List<string> _getTxtRecords(string hostname)
        {
            List<string> txtRecords = new List<string>();
            string output;

            var startInfo = new ProcessStartInfo("nslookup");
            startInfo.Arguments = string.Format("-type=TXT {0}", hostname);
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;

            using (var cmd = Process.Start(startInfo))
            {
                output = cmd.StandardOutput.ReadToEnd();
            }

            MatchCollection matches = Regex.Matches(output, "\"([^\"]*)\"", RegexOptions.IgnoreCase);
            foreach (Match match in matches)
            {
                if (match.Success)
                    txtRecords.Add(match.Groups[1].Value);
            }

            return txtRecords;
        }
    }
}
