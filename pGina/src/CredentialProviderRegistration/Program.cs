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
    public enum MachineType : ushort
    {
        IMAGE_FILE_MACHINE_UNKNOWN = 0x0,
        IMAGE_FILE_MACHINE_AM33 = 0x1d3,
        IMAGE_FILE_MACHINE_AMD64 = 0x8664,
        IMAGE_FILE_MACHINE_ARM = 0x1c0,
        IMAGE_FILE_MACHINE_EBC = 0xebc,
        IMAGE_FILE_MACHINE_I386 = 0x14c,
        IMAGE_FILE_MACHINE_IA64 = 0x200,
        IMAGE_FILE_MACHINE_M32R = 0x9041,
        IMAGE_FILE_MACHINE_MIPS16 = 0x266,
        IMAGE_FILE_MACHINE_MIPSFPU = 0x366,
        IMAGE_FILE_MACHINE_MIPSFPU16 = 0x466,
        IMAGE_FILE_MACHINE_POWERPC = 0x1f0,
        IMAGE_FILE_MACHINE_POWERPCFP = 0x1f1,
        IMAGE_FILE_MACHINE_R4000 = 0x166,
        IMAGE_FILE_MACHINE_SH3 = 0x1a2,
        IMAGE_FILE_MACHINE_SH3DSP = 0x1a3,
        IMAGE_FILE_MACHINE_SH4 = 0x1a6,
        IMAGE_FILE_MACHINE_SH5 = 0x1a8,
        IMAGE_FILE_MACHINE_THUMB = 0x1c2,
        IMAGE_FILE_MACHINE_WCEMIPSV2 = 0x169,
    }

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

        static Settings m_settings = null;
        static string m_providerKey = null;
        static string m_clsidRoot = null;
        static string m_clsidInProc = null;
        static string m_6432ProviderKey = null;

        static int Main(string[] args)
        {
            // Parse command line arguments
            try
            {
                m_settings = Settings.ParseClArgs(args);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("{0}" + Environment.NewLine + Environment.NewLine + "{1}",
                    e.Message, Settings.UsageText());
                return 1;
            }

            // Check path for sanity
            DirectoryInfo pathInfo = new DirectoryInfo(m_settings.Path);
            if (! pathInfo.Exists )
            {
                Console.Error.WriteLine("Path {0} doesn't exist or is not a directory.", m_settings.Path);
                return 1;
            }

            // The registry keys
            string guid = m_settings.ProviderGuid.ToString();
            m_providerKey = string.Format(
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Authentication\Credential Providers\{{{0}}}", 
                guid);
            m_clsidRoot = string.Format(@"CLSID\{{{0}}}", guid);
            m_clsidInProc = string.Format(@"CLSID\{{{0}}}\InprocServer32", guid);
            m_6432ProviderKey = string.Format(
                @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Authentication\Credential Providers\{{{0}}}", 
                guid);
               
            // Do the work...
            try
            {
                ExecuteAction();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("{0}" + Environment.NewLine, e.Message);
                return 1;
            }
            
            return 0;
        }

        private static void ExecuteAction()
        {
            switch (m_settings.OpMode)
            {
                case OperationMode.INSTALL:
                    InstallCP();
                    break;
                case OperationMode.UNINSTALL:
                    // TODO
                    break;
                case OperationMode.DISABLE:
                    // TODO
                    //  - Disable (set a DWORD "Disabled" = 1 in the CLSID key (ala: http://blogs.technet.com/b/ad/archive/2009/07/10/testing-a-credential-provider.aspx)
                    break;
                case OperationMode.ENABLE:
                    // TODO
                    break;
            }
        }

        private static void InstallCP()
        {
            // This will throw if unsuccessful
            CopyDlls();

            WriteRegistryInstall();
        }

        private static void CopyDlls()
        {
            // Copy the DLL
            if (IntPtr.Size == 8)  // Are we on a 64 bit OS?
            {
                FileInfo x64Dll = Find64BitDll();
                FileInfo x32Dll = Find32BitDll();
                string destination64 = String.Format(@"C:\Windows\System32\{0}.dll", m_settings.ShortName);
                string destination32 = String.Format(@"C:\Windows\Syswow64\{0}.dll", m_settings.ShortName);

                if (x64Dll == null && x32Dll == null)
                {
                    throw new Exception("No 64 or 32 bit DLL found in: " + m_settings.Path);
                }

                if (x64Dll != null)
                {
                    Console.WriteLine("Found 64 bit DLL: {0}", x64Dll.FullName);
                    Console.WriteLine("   copying to: {0}", destination64);
                    File.Copy(x64Dll.FullName, destination64, true);
                }

                if (x32Dll != null)
                {
                    Console.WriteLine("Found 32 bit DLL: {0}", x32Dll.FullName);
                    Console.WriteLine("   copying to: {0}", destination32);

                    File.Copy(x32Dll.FullName, destination32, true);
                    
                    // Write registry key for 32 bit DLL
                    SetHKLMValue(m_6432ProviderKey, "", m_settings.ShortName);
                }
            }
            else
            {
                FileInfo x32Dll = Find32BitDll();
                string destination = String.Format(@"C:\Windows\System32\{0}.dll", m_settings.ShortName);

                if (x32Dll != null)
                {
                    Console.WriteLine("Found 32 bit DLL: {0}", x32Dll.FullName);
                    Console.WriteLine("   copying to: {0}", destination);

                    File.Copy(x32Dll.FullName, destination, true);
                }
                else
                {
                    throw new Exception("No 32 bit DLL found in: " + m_settings.Path);
                }                
            }
        }

        private static void WriteRegistryInstall()
        {
            Console.WriteLine("Writing registry entries...");
            
            // Write registry values
            SetHKLMValue(m_providerKey, "", m_settings.ShortName);
            SetClsidValue(m_clsidRoot, "", m_settings.ShortName);
            SetClsidValue(m_clsidInProc, "", m_settings.ShortName);
            SetClsidValue(m_clsidInProc, "ThreadingModel", "Apartment");
        }

        private static FileInfo Find64BitDll()
        {
            // Check path directory
            string path = String.Format(@"{0}\{1}.dll", m_settings.Path, m_settings.ShortName);
            if (File.Exists(path))
            {
                if( Is64BitDll(path) )
                    return new FileInfo(path);
            }

            // Check x64 subdirectory
            path = String.Format(@"{0}\x64\{1}.dll", m_settings.Path, m_settings.ShortName);
            if (File.Exists(path))
            {
                if (Is64BitDll(path))
                    return new FileInfo(path);
            }

            return null;
        }

        private static FileInfo Find32BitDll()
        {
            // Check path directory
            string path = String.Format(@"{0}\{1}.dll", m_settings.Path, m_settings.ShortName);
            if (File.Exists(path))
            {
                if (! Is64BitDll(path))
                    return new FileInfo(path);
            }

            // Check x86 subdirectory
            path = String.Format(@"{0}\x86\{1}.dll", m_settings.Path, m_settings.ShortName);
            if (File.Exists(path))
            {
                if (! Is64BitDll(path))
                    return new FileInfo(path);
            }

            return null;
        }

        private static void SetHKLMValue(string subKey, string name, string value)
        {
            using (RegistryKey key = Registry.LocalMachine.CreateSubKey(subKey))
            {
                Console.WriteLine("Writing {0} => {1}", key.ToString(), value);
                key.SetValue(name, value);
            }
        }

        private static void SetClsidValue(string subKey, string name, string value)
        {
            using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(subKey))
            {
                Console.WriteLine("Writing {0} => {1}", key.ToString(), value);
                key.SetValue(name, value);
            }
        }

        private static bool Is64BitDll(string fullPath)
        {
            switch (GetDllMachineType(fullPath))
            {
                case MachineType.IMAGE_FILE_MACHINE_AMD64:
                case MachineType.IMAGE_FILE_MACHINE_IA64:
                    return true;
            }
            return false;
        }

        private static MachineType GetDllMachineType(string fullPath)
        {
            FileStream fs = null;
            BinaryReader br = null;
            try
            {
                fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
                br = new BinaryReader(fs);

                fs.Seek(0x3c, SeekOrigin.Begin);
                Int32 peOffset = br.ReadInt32();
                fs.Seek(peOffset, SeekOrigin.Begin);
                UInt32 peHead = br.ReadUInt32();
                if (peHead != 0x00004550) // "PE00" little-endian
                {
                    throw new Exception("Unable to find PE header in " + fullPath);
                }

                MachineType type = (MachineType)br.ReadUInt16();
                return type;
            }
            finally
            {
                if( br != null ) br.Close();
                if( fs != null ) fs.Close();
            }
        }
    }
}
