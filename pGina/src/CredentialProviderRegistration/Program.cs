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

namespace pGina.CredentialProvider.Registration
{
    class Program
    {
        /*
         [HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Authentication\Credential Providers\{781A7B48-79A7-4fcf-92CC-A6977171F1A8}]
         @="pGinaCredentialProvider"

         [HKEY_CLASSES_ROOT\CLSID\{781A7B48-79A7-4fcf-92CC-A6977171F1A8}]
         @="pGinaCredentialProvider"

         [HKEY_CLASSES_ROOT\CLSID\{781A7B48-79A7-4fcf-92CC-A6977171F1A8}\InprocServer32]
         @="SampleCredUICredentialProvider.dll"
         "ThreadingModel"="Apartment"
        */

        static void Main(string[] args)
        {
            // Currently this only works on our provider, and only does registration, ideally
            //  we'd extend this to be more generic and take arguments for:
            //  - Disable (set a DWORD "Disabled" = 1 in the CLSID key (ala: http://blogs.technet.com/b/ad/archive/2009/07/10/testing-a-credential-provider.aspx)
            //  - Guid - so we can register/disable other provider guids
            //  - Path to dll - copy a different source provider dll
            
            Guid m_providerGuid = new Guid("{D0BEFEFB-3D2C-44DA-BBAD-3B2D04557246}");

            if (IntPtr.Size == 8)
            {
                string providerKey = string.Format(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Authentication\Credential Providers\{{{0}}}", m_providerGuid.ToString());
                string clsidRoot = string.Format(@"CLSID\{{{0}}}", m_providerGuid.ToString());
                string clsidInProc = string.Format(@"CLSID\{{{0}}}\InprocServer32", m_providerGuid.ToString());

                string name = new FileInfo("x64\\pGinaCredentialProvider.dll").FullName;
                string shortName = "pGinaCredentialProvider";                
                File.Copy(name, @"C:\Windows\System32\pGinaCredentialProvider.dll", true);

                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(providerKey))
                {
                    key.SetValue("", shortName);
                }

                using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(clsidRoot))
                {
                    key.SetValue("", shortName);
                }

                using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(clsidInProc))
                {
                    key.SetValue("", shortName);
                    key.SetValue("ThreadingModel", "Apartment");
                }

                if (File.Exists("x86\\pGinaCredentialProvider"))
                {
                    name = new FileInfo("Win32\\pGinaCredentialProvider.dll").FullName;
                    File.Copy(name, @"C:\Windows\Syswow64\pGinaCredentialProvider.dll", true);

                    providerKey = string.Format(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Authentication\Credential Providers\{{{0}}}", m_providerGuid.ToString());
                    using (RegistryKey key = Registry.LocalMachine.CreateSubKey(providerKey))
                    {
                        key.SetValue("", shortName);
                    }
                }
            }
            else
            {
                string providerKey = string.Format(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Authentication\Credential Providers\{{{0}}}", m_providerGuid.ToString());
                string clsidRoot = string.Format(@"CLSID\{{{0}}}", m_providerGuid.ToString());
                string clsidInProc = string.Format(@"CLSID\{{{0}}}\InprocServer32", m_providerGuid.ToString());

                string name = new FileInfo("Win32\\pGinaCredentialProvider.dll").FullName;
                string shortName = "pGinaCredentialProvider";
                File.Copy(name, @"C:\Windows\System32\pGinaCredentialProvider.dll", true);

                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(providerKey))
                {
                    key.SetValue("", shortName);
                }

                using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(clsidRoot))
                {
                    key.SetValue("", shortName);
                }

                using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(clsidInProc))
                {
                    key.SetValue("", shortName);
                    key.SetValue("ThreadingModel", "Apartment");
                }
            }
        }
    }
}
