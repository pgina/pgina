using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Abstractions.Windows
{
    public class OsInfo
    {
        public static bool IsVistaOrLater()
        {
            OperatingSystem sys = System.Environment.OSVersion;

            if (sys.Platform == PlatformID.Win32NT &&
                sys.Version.Major >= 6)
                return true;

            return false;
        }

        public static bool IsWindows()
        {
            OperatingSystem sys = System.Environment.OSVersion;

            if (sys.Platform == PlatformID.Win32NT ||
                sys.Platform == PlatformID.Win32S ||
                sys.Platform == PlatformID.Win32Windows ||
                sys.Platform == PlatformID.WinCE)
                return true;

            return false;
        }

        public static bool Is64Bit()
        {
            return IntPtr.Size == 8;
        }
    }
}
