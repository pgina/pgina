using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using log4net;

namespace pGina.Plugin.ScriptRunner
{
    class Script
    {
        public string File { get; set; }
        public int Timeout { get; set; }
        private ILog m_logger = LogManager.GetLogger("Script");

        public Script()
        {
            this.Timeout = 300000;
            this.File = null;
        }

        public void Run()
        {
            if (File == null) return;

            m_logger.InfoFormat("Executing script: {0}", File);
            System.Diagnostics.ProcessStartInfo startInfo = 
                new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = File,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo = startInfo;
            p.EnableRaisingEvents = true;
            p.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(p_OutputDataReceived);
            p.ErrorDataReceived += new System.Diagnostics.DataReceivedEventHandler(p_ErrorDataReceived);
            p.Exited += new EventHandler(p_Exited);

            p.Start();
            if (p.WaitForExit(Timeout))
            {
                m_logger.DebugFormat("Script finished execution successfully.");                
            }
            else
            {
                m_logger.ErrorFormat("Timeout reached before script finished executing.");
            }
            p.Close();
        }

        void p_Exited(object sender, EventArgs e)
        {
            m_logger.DebugFormat("p_Exited");
        }

        void p_ErrorDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            m_logger.ErrorFormat(e.Data);
        }

        void p_OutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            m_logger.InfoFormat(e.Data);
        }
    }
}
