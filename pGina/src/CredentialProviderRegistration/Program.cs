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
using System.IO;
using Microsoft.Win32;

using log4net;

namespace pGina.CredentialProvider.Registration
{
    class Program
    {
        static Program()
        {
            // Init logging
            pGina.Shared.Logging.Logging.Init();
        }

        static ILog m_logger = LogManager.GetLogger("Program");

        static int Main(string[] args)
        {
            // Default settings
            Settings settings = new Settings();
            try
            {
                // Parse command line arguments
                settings = Settings.ParseClArgs(args);
            }
            catch (Exception e)
            {
                m_logger.ErrorFormat("{0}" + Environment.NewLine + Environment.NewLine + "{1}",
                    e.Message, Settings.UsageText());
                return 1;
            }

            // Check path for sanity
            DirectoryInfo pathInfo = new DirectoryInfo(settings.Path);
            if (! pathInfo.Exists )
            {
                m_logger.ErrorFormat("Path {0} doesn't exist or is not a directory.", settings.Path);
                return 1;
            }
               
            // Do the work...
            try
            {
                CredProviderManager manager = CredProviderManager.GetManager();
                manager.CpInfo = settings;
                manager.ExecuteDefaultAction();
            }
            catch (Exception e)
            {
                m_logger.ErrorFormat("Error: {0}" + Environment.NewLine, e);
                return 1;
            }
            
            return 0;
        }
    }
}
