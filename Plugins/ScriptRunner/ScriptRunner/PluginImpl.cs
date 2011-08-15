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

        void IPluginSystemSessionHelper.SessionEnding()
        {
            throw new NotImplementedException();
        }

        void IPluginSystemSessionHelper.SessionStarted(Shared.Types.UserInformation userInformation)
        {
            throw new NotImplementedException();
        }

        void IPluginUserSessionHelper.SessionEnding()
        {
            m_logger.DebugFormat("IPluginUserSessionHelper.SessionEnding");
        }

        void IPluginUserSessionHelper.SessionStarted(Shared.Types.UserInformation userInformation)
        {
            m_logger.DebugFormat("IPluginUserSessionHelper.SessionStarted");
            Script s = new Script();
            s.File = @"C:\test.bat";
            s.Run();
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
            throw new NotImplementedException();
        }
    }
}
