/*
	Copyright (c) 2011, pGina Team
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

using log4net;

namespace pGina.Plugin.ScriptRunner
{
    class BatchScript : Script
    {
        private ILog m_logger = LogManager.GetLogger("BatchScript");

        public BatchScript(string fileName)
        {
            this.File = fileName;
        }

        public override void Run()
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
