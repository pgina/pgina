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
            if (String.IsNullOrEmpty(File))
            {
                m_logger.DebugFormat("No script file specified.");
                return;
            }

            if (! System.IO.File.Exists(this.File))
            {
                m_logger.ErrorFormat("Script {0} not found, skipping.", this.File);
                return;
            }

            m_logger.InfoFormat("Executing script: {0}", File);
            System.Diagnostics.ProcessStartInfo startInfo = 
                new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = File,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
            using (System.Diagnostics.Process p = new System.Diagnostics.Process())
            {
                p.StartInfo = startInfo;
                p.EnableRaisingEvents = true;
                p.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(p_OutputDataReceived);
                p.ErrorDataReceived += new System.Diagnostics.DataReceivedEventHandler(p_ErrorDataReceived);
                p.Exited += new EventHandler(p_Exited);
                
                p.Start();
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
                if (p.WaitForExit(Timeout))
                {
                    // Do this again to make sure that all output has been flushed.
                    p.WaitForExit();
                    m_logger.DebugFormat("Script finished execution.  Exit code: {0}", p.ExitCode);
                }
                else
                {
                    m_logger.ErrorFormat("Timeout reached before script finished executing.");
                }
            }
        }

        void p_Exited(object sender, EventArgs e)
        {
            m_logger.Debug("Process finished.");
        }

        void p_ErrorDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            if (!String.IsNullOrEmpty(e.Data))
            {
                m_logger.Error(e.Data);
            }
        }

        void p_OutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            if (!String.IsNullOrEmpty(e.Data))
            {
                m_logger.Info(e.Data);
            }
        }
    }
}
