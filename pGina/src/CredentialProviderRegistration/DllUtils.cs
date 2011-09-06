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
            // Check path directory
            string fullPath = String.Format(@"{0}\{1}.dll", path, baseName);
            if (File.Exists(fullPath))
            {
                if (DllUtils.Is64BitDll(fullPath))
                    return new FileInfo(fullPath);
            }

            // Check x64 subdirectory
            fullPath = String.Format(@"{0}\x64\{1}.dll", path, baseName);
            if (File.Exists(fullPath))
            {
                if (DllUtils.Is64BitDll(fullPath))
                    return new FileInfo(fullPath);
            }

            return null;
        }

        public static FileInfo Find32BitDll(string path, string baseName)
        {
            // Check path directory
            string fullPath = String.Format(@"{0}\{1}.dll",path, baseName);
            if (File.Exists(fullPath))
            {
                if (!DllUtils.Is64BitDll(fullPath))
                    return new FileInfo(fullPath);
            }

            // Check Win32 subdirectory
            fullPath = String.Format(@"{0}\Win32\{1}.dll", path, baseName);
            if (File.Exists(fullPath))
            {
                if (!DllUtils.Is64BitDll(fullPath))
                    return new FileInfo(fullPath);
            }

            return null;
        }
    }
}
