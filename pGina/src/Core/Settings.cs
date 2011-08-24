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
        public static dynamic s_settings = new DynamicSettings(DynamicSettings.PGINA_KEY);
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
            s_settings.SetDefault("SessionInfoCacheTimeout", 300);
            s_settings.SetDefault("SessionHelperExe", "pGina.Service.SessionHelper.exe");
        }
    }
}
