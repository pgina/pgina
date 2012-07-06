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

namespace pGina.CredentialProvider.Registration
{
    public class DllUtils
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

        public static bool Is64BitDll(string fullPath)
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
                if (br != null) br.Close();
                if (fs != null) fs.Close();
            }
        }

        public static FileInfo Find64BitDll(string path, string baseName)
        {
            if (! baseName.EndsWith(".dll", StringComparison.CurrentCultureIgnoreCase))
                baseName += ".dll";

            // Check path directory
            string fullPath = Path.Combine(path, baseName);
            if (File.Exists(fullPath))
            {
                if (DllUtils.Is64BitDll(fullPath))
                    return new FileInfo(fullPath);
            }

            // Check x64 subdirectory
            fullPath = Path.Combine(path, "x64", baseName);
            if (File.Exists(fullPath))
            {
                if (DllUtils.Is64BitDll(fullPath))
                    return new FileInfo(fullPath);
            }

            return null;
        }

        public static FileInfo Find32BitDll(string path, string baseName)
        {
            if (!baseName.EndsWith(".dll", StringComparison.CurrentCultureIgnoreCase))
                baseName += ".dll";

            // Check path directory
            string fullPath = Path.Combine(path, baseName);
            if (File.Exists(fullPath))
            {
                if (!DllUtils.Is64BitDll(fullPath))
                    return new FileInfo(fullPath);
            }

            // Check Win32 subdirectory
            fullPath = Path.Combine(path, "Win32", baseName);
            if (File.Exists(fullPath))
            {
                if (!DllUtils.Is64BitDll(fullPath))
                    return new FileInfo(fullPath);
            }

            return null;
        }
    }
}
