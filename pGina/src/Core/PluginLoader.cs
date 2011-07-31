using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

using log4net;

using pGina.Shared.Interfaces;

namespace pGina.Core
{    
    public class PluginLoader
    {
        private string[] m_pluginDirectories = null;
        private List<IPluginBase> m_plugins = new List<IPluginBase>();
        private ILog m_logger = LogManager.GetLogger("PluginLoader");

        public enum State
        {
            UIEnabled            = 1,
            AuthenticateEnabled  = 1 << 1,
            AuthorizeEnabled     = 1 << 2,
            GatewayEnabled       = 1 << 3,
            NotificationEnabled  = 1 << 4,
            UserSessionEnabled   = 1 << 5,
            SystemSessionEnabled = 1 << 6,
        }

        public string[] PluginDirectories
        {
            get { return m_pluginDirectories; }
            set { m_pluginDirectories = value; }
        }

        public PluginLoader(string[] dirs)
        {
            m_pluginDirectories = dirs;
        }

        public PluginLoader()
        {
        }

        public void Load()
        {
            m_plugins.Clear();
            
            foreach (string dir in m_pluginDirectories)
            {
                LoadPluginsFromDir(dir);
            }       
     
            // All plugins default to completely disabled
            foreach (IPluginBase plugin in m_plugins)
            {                
                Settings.Get.SetDefault(plugin.Uuid.ToString(), 0);
            }
        }

        private void LoadPluginsFromDir(string dir)
        {
            if (!Directory.Exists(dir))
            {
                m_logger.WarnFormat("Skipping invalid plugin directory: {0}", dir);
                return;
            }

            m_logger.DebugFormat("Loading plugins from {0}", dir);

            string[] files = Directory.GetFiles(dir, "*.dll");                        
            foreach (string file in files)
            {
                try
                {
                    // Load the assembly up
                    Assembly assembly = Assembly.LoadFile(file);
                    foreach (Type type in assembly.GetTypes())
                    {
                        // Make sure its a public class
                        if (!type.IsClass || type.IsNotPublic) 
                            continue;

                        Type[] interfaces = type.GetInterfaces();
                        if (interfaces.Contains(typeof(IPluginBase)))
                        {
                            // TBD: We could do inverted control here.. logger, settings, etc?
                            //  We could also consider loading plugins in their own app domain,
                            //  making them unloadable...
                            m_logger.DebugFormat("Created plugin object type: {0} from plugin: {1}", type.ToString(), file);
                            object pluginObject = Activator.CreateInstance(type);
                            m_plugins.Add(pluginObject as IPluginBase);
                        }                        
                    }
                }
                catch (Exception ex)
                {
                    m_logger.ErrorFormat("Error loading {0}: {1}", file, ex);
                }
            }
        }

        public List<IPluginBase> AllPlugins
        {
            get { return m_plugins; }
        }

        public List<T> GetPluginsOfType<T>(bool enabledOnly) where T : class, IPluginBase
        {
            List<T> pluginList = new List<T>();
            foreach (IPluginBase plugin in m_plugins)
            {
                if (plugin is T)
                {
                    int pluginMask = Settings.Get.GetSetting(plugin.Uuid.ToString());
                    if(enabledOnly && !IsEnabledFor<T>(pluginMask))
                        continue;

                    pluginList.Add(plugin as T);
                }
            }

            return pluginList;
        }

        private static bool TestMask(int mask, State state)
        {
            int stateMask = (int) state;
            int val = stateMask & mask;
            if (val != 0)
                return true;

            return false;
        }

        public static bool IsEnabledFor<T>(int mask) where T: IPluginBase
        {
            if (typeof(T) == typeof(IPluginAuthentication) && TestMask(mask, State.AuthenticateEnabled))
                return true;
            
            if (typeof(T) == typeof(IPluginAuthorization) && TestMask(mask, State.AuthorizeEnabled))
                return true;

            if (typeof(T) == typeof(IPluginAuthenticationGateway) && TestMask(mask, State.GatewayEnabled))
                return true;

            if (typeof(T) == typeof(IPluginEventNotifications) && TestMask(mask, State.NotificationEnabled))
                return true;
            
            if (typeof(T) == typeof(IPluginUserSessionHelper) && TestMask(mask, State.UserSessionEnabled))
                return true;
            
            if (typeof(T) == typeof(IPluginSystemSessionHelper) && TestMask(mask, State.SystemSessionEnabled))
                return true;
            
            return false;
        }

        public static bool IsEnabledFor<T>(IPluginBase plugin) where T : IPluginBase
        {
            int pluginMask = Settings.Get.GetSetting(plugin.Uuid.ToString());
            return IsEnabledFor<T>(pluginMask);
        }

        public List<IPluginConfiguration> GetConfigurablePlugins()
        {
            return GetPluginsOfType<IPluginConfiguration>(false);
        }

        public List<T> GetOrderedPluginsOfType<T>() where T : class, IPluginBase
        {
            List<T> loadedPlugins = GetPluginsOfType<T>(true);

            // Now sort by order in settings
            string setting = string.Format("{0}_Order", typeof(T).ToString().Replace("pGina.Shared.Interfaces.", ""));
            string[] order = Settings.Get.GetSetting(setting, new string[] { });

            List<T> orderedPlugins = new List<T>();
            foreach (string uuid in order)
            {
                foreach (T p in loadedPlugins.Where(plugin => plugin.Uuid.ToString() == uuid))
                {
                    orderedPlugins.Add(p);
                }
            }

            // We now have all plugins listed from our order list, what about any remaining? lets pair down 
            // and look.
            foreach (T p in orderedPlugins)
            {
                loadedPlugins.Remove(p);
            }

            // Any remaining plugins were loaded and enabled, so they go to the back of our list
            //  and then we update or list
            if (loadedPlugins.Count > 0)
            {
                foreach (T p in loadedPlugins)
                {
                    orderedPlugins.Add(p);
                }

                List<string> newOrder = new List<string>();
                foreach (var p in orderedPlugins)
                    newOrder.Add(p.Uuid.ToString());
                Settings.Get.SetSetting(setting, newOrder.ToArray());
            }

            return orderedPlugins;
        }          
    }
}
