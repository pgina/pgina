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
using System.Text.RegularExpressions;
using System.IO;
using System.Reflection;

using Abstractions;

namespace pGina.CredentialProvider.Registration
{
    public enum OperationMode
    {
        INSTALL, UNINSTALL, DISABLE, ENABLE
    }

    public class Settings
    {
        // What to do when ExecuteDefaultAction is called.
        public OperationMode OpMode { get; set; }

        // The GUID of the CP (not needed for GINA)
        public Guid ProviderGuid { get; set; }

        // The short name of the DLL (not including the extension)
        public string ShortName { get; set; }

        // The path to the directory containing the DLL (or to a parent directory
        // if the DLL is contained in architecture specific subdirectories 'x64' and
        // 'Win32'.)
        public string Path { get; set; }

        public Settings()
        {
            // Defaults
            this.ProviderGuid = new Guid("{D0BEFEFB-3D2C-44DA-BBAD-3B2D04557246}");
            this.Path = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            this.ShortName = null;
            this.OpMode = OperationMode.INSTALL;
        }
     }
}
