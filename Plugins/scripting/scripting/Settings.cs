using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using pGina.Shared.Settings;

namespace pGina.Plugin.scripting
{
    public class Settings
    {
        private static dynamic m_settings = new pGinaDynamicSettings(PluginImpl.PluginUuid);

        static Settings()
        {
            // Authentication step settings:
            m_settings.SetDefault("authe_sys", new string[] { @"True	cmd.exe /c echo %USERNAME% %u %p %i %Ae %Ao %Gw authe>>%SystemDrive%\sys.txt" });

            // Authorization step settings
            m_settings.SetDefault("autho_sys", new string[] { @"False	cmd.exe /c echo %USERNAME% %u %p %i %Ae %Ao %Gw autho>>%SystemDrive%\sys.txt"  });

            // Gateway step settings
            m_settings.SetDefault("gateway_sys", new string[] { @"True	cmd.exe /c echo %USERNAME% %u %p %i %Ae %Ao %Gw gateway>>%SystemDrive%\sys.txt"  });

            // Notification
            m_settings.SetDefault("notification_sys", new string[] { @"True	True	False	cmd.exe /c echo %USERNAME% %u %p %s %e %i %Ae %Ao %Gw logon %p >>%PUBLIC%\sys.txt", @"False	True	False	cmd.exe /c echo %USERNAME% %u %p %s %e %i %Ae %Ao %Gw logon>>%PUBLIC%\sys.txt", @"True	False	True	cmd.exe /c echo %USERNAME% %u %p %s %e %i %Ae %Ao %Gw logoff %p>>%PUBLIC%\sys.txt", @"False	False	True	cmd.exe /c echo %USERNAME% %u %p %s %e %i %Ae %Ao %Gw logoff>>%PUBLIC%\sys.txt" });
            m_settings.SetDefault("notification_usr", new string[] { @"True	False	True	cmd.exe /c echo %USERNAME% %u %p %s %e %i %Ae %Ao %Gw logoff %p >>%PUBLIC%\usr.txt", @"False	False	True	cmd.exe /c echo %USERNAME% %u %p %s %e %i %Ae %Ao %Gw logoff>>%PUBLIC%\usr.txt", @"True	True	False	cmd.exe /c echo %USERNAME% %u %p %s %e %i %Ae %Ao %Gw logon %p >>%PUBLIC%\usr.txt", @"False	True	False	cmd.exe /c echo %USERNAME% %u %p %s %e %i %Ae %Ao %Gw logon>>%PUBLIC%\usr.txt" });

            // change password
            m_settings.SetDefault("changepwd_sys", new string[] { });
            m_settings.SetDefault("changepwd_usr", new string[] { });
        }

        public static dynamic Store
        {
            get { return m_settings; }
        }
    }
}
