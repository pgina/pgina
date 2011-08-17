using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using log4net;

using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Collections.ObjectModel;

namespace pGina.Plugin.ScriptRunner
{
    class PowerShellScript
    {
        public string File { get; set; }
        private ILog m_logger = LogManager.GetLogger("PowerShellScript");

        public PowerShellScript()
        {
            this.File = null;
        }

        public void Run()
        {
            if (String.IsNullOrEmpty(File))
            {
                m_logger.DebugFormat("No script file specified.");
                return;
            }

            if (!System.IO.File.Exists(this.File))
            {
                m_logger.ErrorFormat("Script {0} not found, skipping.", this.File);
                return;
            }

            using (Runspace rSpace = RunspaceFactory.CreateRunspace())
            {
                try
                {
                    Collection<PSObject> results = null;
                    rSpace.Open();
                    using (Pipeline pipeline = rSpace.CreatePipeline())
                    {
                        m_logger.DebugFormat("Loading script {0}", this.File);
                        pipeline.Commands.AddScript(System.IO.File.ReadAllText(this.File));

                        m_logger.InfoFormat("Executing script {0}", this.File);
                        results = pipeline.Invoke();
                    }

                    if (results != null)
                    {
                        foreach (PSObject obj in results)
                        {
                            m_logger.InfoFormat(obj.ToString());
                        }
                    }
                    m_logger.InfoFormat("Script {0} finished", this.File);
                }
                catch (Exception e)
                {
                    m_logger.ErrorFormat("Exception {0} when executing script {1}", e.GetType().ToString(), this.File);
                    m_logger.ErrorFormat("   Message: {0}", e.Message);
                }
            }
        }
    }
}
