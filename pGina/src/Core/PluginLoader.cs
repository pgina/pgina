/*
	Copyright (c) 2017, pGina Team
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

using log4net;

using pGina.Shared.Interfaces;

namespace pGina.Core
{
    public static class PluginLoader
    {
        private static string[] m_pluginDirectories = null;
        private static List<IPluginBase> m_plugins = new List<IPluginBase>();
        private static ILog m_logger = LogManager.GetLogger("PluginLoader");

        public enum State
        {
            UIEnabled            = 1,
            AuthenticateEnabled  = 1 << 1,
            AuthorizeEnabled     = 1 << 2,
            GatewayEnabled       = 1 << 3,
            NotificationEnabled  = 1 << 4,
            ChangePasswordEnabled = 1 << 5,
        }

        public static string[] PluginDirectories
        {
            get { return m_pluginDirectories; }
            set { m_pluginDirectories = value; }
        }

        public static void Init()
        {
            m_logger.DebugFormat("Initializing");
            PluginDirectories = Core.Settings.Get.PluginDirectories;

            // Set up an event handler to load plugin dependencies
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += new ResolveEventHandler(ResolvePluginDependencies);

            // Load the plugins themselves
            PluginLoader.LoadPlugins();

            m_logger.DebugFormat("Plugins loaded, list follows: ");
            foreach (IPluginBase plugin in PluginLoader.AllPlugins)
            {
                m_logger.DebugFormat("  {0} -> {1}", plugin.Name, plugin.Uuid.ToString());
            }
        }

        /// <summary>
        /// This method is intended to help plugins load their dependencies.  It is an event handler
        /// that is triggered when an assembly is not found (resolved).  It tries to load the
        /// assembly from the same directory as the assembly that is requesting the dependency.
        /// </summary>
        /// <returns>The assembly or null if not found.</returns>
        public static Assembly ResolvePluginDependencies(Object sender, ResolveEventArgs args)
        {
            // Get the file name
            string[] fields = args.Name.Split(',');
            string name = fields[0].Trim();
            string culture = fields[2].Trim();

            // Ignore requests for resources.  These are "satellite assemblies" for
            // localization, and this loader will not load them.
            if (name.EndsWith(".resources") && !culture.EndsWith("neutral"))
                return null;

            string fileName = name + ".dll";
            m_logger.DebugFormat("Resolving dependency {0}", fileName);

            string requestorPath = null;
            if (args.RequestingAssembly == null)
            {
                requestorPath = Assembly.GetExecutingAssembly().Location;
            }
            else
            {
                requestorPath = args.RequestingAssembly.Location;
                if (string.IsNullOrEmpty(requestorPath))
                {
                    requestorPath = args.RequestingAssembly.CodeBase;

                    // In certain cases (using SOAP references) this may be a URI
                    if (Uri.IsWellFormedUriString(requestorPath, UriKind.Absolute))
                    {
                        Uri requestorUri = new Uri(requestorPath);
                        if (requestorUri.IsFile)
                        {
                            requestorPath = requestorUri.LocalPath;
                        }
                    }
                }
            }

            // If unable to find a good path to search
            if (string.IsNullOrEmpty(requestorPath))
            {
                m_logger.ErrorFormat("Unable to find a reasonable search path for {0}, giving up.", fileName);
                return null;
            }

            try
            {
                // Look for the assembly in the same directory as the plugin that is loading the assembly
                DirectoryInfo dir = Directory.GetParent(requestorPath);
                string path = Path.Combine(dir.FullName, fileName);

                m_logger.DebugFormat("Looking for: {0}", path);
                Assembly a = null;
                if (dir.Exists)
                {
                    if (File.Exists(path))
                        a = Assembly.LoadFile(path);
                    else
                        m_logger.DebugFormat("{0} not found", path);
                }

                if (a == null)
                    m_logger.ErrorFormat("Unable to resolve dependency: {0}", args.Name);
                else
                    m_logger.InfoFormat("Successfully loaded assembly: {0}", a.FullName);

                return a;
            }
            catch (Exception e)
            {
                m_logger.ErrorFormat("Error when loading dependency: {0}", e);
                return null;
            }
        }

        public static void LoadPlugins()
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

        private static void LoadPluginsFromDir(string dir)
        {
            if (!Directory.Exists(dir))
            {
                m_logger.WarnFormat("Skipping invalid plugin directory: {0}", dir);
                return;
            }

            m_logger.DebugFormat("Loading plugins from {0}", dir);

            string[] files = Directory.GetFiles(dir, "pGina.Plugin.*.dll");
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
                            object pluginObject = Activator.CreateInstance(type);
                            IPluginBase pluginBase = pluginObject as IPluginBase;
                            m_logger.DebugFormat("Created plugin object type: {0} from plugin: {1} uuid: {2}", type.ToString(), file, pluginBase.Uuid);
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

        public static List<IPluginBase> AllPlugins
        {
            get { return m_plugins; }
        }

        /// <summary>
        /// Returns stateful plugins that are enabled in at least one of the three
        /// stages in the login chain.
        /// </summary>
        /// <returns></returns>
        public static List<IStatefulPlugin> GetEnabledStatefulPlugins()
        {
            List<IStatefulPlugin> list = new List<IStatefulPlugin>();

            foreach (IPluginBase plugin in m_plugins)
            {
                int pluginMask = Settings.Get.GetSetting(plugin.Uuid.ToString());
                if ( plugin is IStatefulPlugin &&
                        (
                        IsEnabledFor<IPluginAuthentication>(pluginMask) ||
                        IsEnabledFor<IPluginAuthorization>(pluginMask) ||
                        IsEnabledFor<IPluginAuthenticationGateway>(pluginMask)
                        )
                   )
                {
                    list.Add(plugin as IStatefulPlugin);
                }
            }
            return list;
        }

        public static List<T> GetPluginsOfType<T>(bool enabledOnly) where T : class, IPluginBase
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

            if (typeof(T) == typeof(IPluginLogoffRequestAddTime) && TestMask(mask, State.NotificationEnabled))
                return true;

            if (typeof(T) == typeof(IPluginChangePassword) && TestMask(mask, State.ChangePasswordEnabled))
                return true;

            return false;
        }

        public static bool IsEnabledFor<T>(IPluginBase plugin) where T : IPluginBase
        {
            int pluginMask = Settings.Get.GetSetting(plugin.Uuid.ToString());
            return IsEnabledFor<T>(pluginMask);
        }

        public static List<IPluginConfiguration> GetConfigurablePlugins()
        {
            return GetPluginsOfType<IPluginConfiguration>(false);
        }

        public static List<T> GetOrderedPluginsOfType<T>() where T : class, IPluginBase
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
