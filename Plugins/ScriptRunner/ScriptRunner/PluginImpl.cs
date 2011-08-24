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
            get { return "1.0.0"; }
        }

        public void Configure()
        {
            Configuration dlg = new Configuration();
            dlg.ShowDialog();
        }
    }
}
