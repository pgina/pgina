using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

using log4net;

using pGina.Shared.Settings;
using pGina.Shared.Logging;
using pGina.Shared.Interfaces;

using pGina.Core;

namespace pGina.Service.Impl
{
    public class Service
    {
        private ILog m_logger = LogManager.GetLogger("pGina.Service.Impl");
        private PluginLoader m_pluginLoader = new PluginLoader();

        static Service()
        {
            Framework.Init();
        }

        public string[] PluginDirectories
        {
            get { return Core.Settings.Get.PluginDirectories; }
        }

        public Service()
        {            
        }

        public void Start()
        {
            m_logger.DebugFormat("Loading plugins");
            m_pluginLoader.PluginDirectories = Core.Settings.Get.PluginDirectories;
            m_pluginLoader.Load();

            m_logger.DebugFormat("Plugins loaded, list follows: ");
            foreach (IPluginBase plugin in m_pluginLoader.AllPlugins)
            {
                m_logger.DebugFormat("  {0} -> {1}", plugin.Name, plugin.Uuid.ToString());
            }            
        }
    }
}
