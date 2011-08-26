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

using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Collections.ObjectModel;

namespace pGina.Plugin.ScriptRunner
{
    class PowerShellScript : Script
    {
        private ILog m_logger = LogManager.GetLogger("PowerShellScript");

        public PowerShellScript()
        {
            this.File = null;
        }

        public PowerShellScript(string fileName)
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
                            m_logger.Info(obj.ToString());
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
