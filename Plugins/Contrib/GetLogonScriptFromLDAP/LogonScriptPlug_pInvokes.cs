/*
    Copyright (c) 2011, pGina Team
    All rights reserved.
    Adapted to the LogonScript plugin by Florian Rohmer (2013).

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
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Reflection;
using System.Net;
using System.Net.Security;
using System.Security.Principal;
using System.ComponentModel;

namespace pGina.Plugin.LogonScriptFromLDAP
{
    public class pInvokes
    {
        internal class SafeNativeMethods
        {
            #region Structs/Enums

            [StructLayout(LayoutKind.Sequential)]
            public struct SECURITY_ATTRIBUTES
            {
                public int nLength;
                public IntPtr lpSecurityDescriptor;
                public int bInheritHandle;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct STARTUPINFO
            {
                public Int32 cb;
                public String lpReserved;
                public String lpDesktop;
                public String lpTitle;
                public UInt32 dwX;
                public UInt32 dwY;
                public UInt32 dwXSize;
                public UInt32 dwYSize;
                public UInt32 dwXCountChars;
                public UInt32 dwYCountChars;
                public UInt32 dwFillAttribute;
                public UInt32 dwFlags;
                public short wShowWindow;
                public short cbReserved2;
                public IntPtr lpReserved2;
                public IntPtr hStdInput;
                public IntPtr hStdOutput;
                public IntPtr hStdError;
            };

            [StructLayout(LayoutKind.Sequential)]
            public struct PROCESS_INFORMATION
            {
                public IntPtr hProcess;
                public IntPtr hThread;
                public UInt32 dwProcessId;
                public UInt32 dwThreadId;
            };

            public const int CREATE_UNICODE_ENVIRONMENT = 0x00000400;
            #endregion

            #region wtsapi32.dll
            [DllImport("wtsapi32.dll", SetLastError = true)]
            public static extern bool WTSQueryUserToken(int sessionId, out IntPtr Token);
            #endregion

            #region kernel32.dll
            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool CloseHandle(IntPtr hObject);
            #endregion

            [DllImport("userenv.dll", SetLastError = true)]
            public static extern Boolean CreateEnvironmentBlock(ref IntPtr lpEnvironment, IntPtr hToken, Boolean bInherit);

            [DllImport("advapi32.dll", SetLastError = true)]
            public static extern bool CreateProcessAsUser(IntPtr hToken, string lpApplicationName, string lpCommandLine, ref SECURITY_ATTRIBUTES lpProcessAttributes,
                                                           ref SECURITY_ATTRIBUTES lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment,
                                                           string lpCurrentDirectory, ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

        }

        public static IntPtr WTSQueryUserToken(int sessionId)
        {
            IntPtr result = IntPtr.Zero;
            if (SafeNativeMethods.WTSQueryUserToken(sessionId, out result))
                return result;
            return IntPtr.Zero;
        }

        public static bool CloseHandle(IntPtr handle)
        {
            return SafeNativeMethods.CloseHandle(handle);
        }

        public static System.Diagnostics.Process StartUserProcessInSession(int sessionId, string appName, string cmdLine)
        {
            IntPtr processToken = IntPtr.Zero;

            try
            {
                // Get user's token from session id, WTSQueryUserToken already returns a primary token for us
                // so all we then have to do is use it to start the process.
                if (!SafeNativeMethods.WTSQueryUserToken(sessionId, out processToken))
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "WTSQueryUserToken");

                return StartProcessWithToken(processToken, appName, cmdLine);
            }
            finally
            {
                SafeNativeMethods.CloseHandle(processToken);
            }
        }

        public static System.Diagnostics.Process StartProcessWithToken(IntPtr token, string appName, string cmdLine)
        {
            IntPtr environmentBlock = IntPtr.Zero;

            try
            {
                // Default nil security attribute
                SafeNativeMethods.SECURITY_ATTRIBUTES defSec = new SafeNativeMethods.SECURITY_ATTRIBUTES();
                defSec.nLength = Marshal.SizeOf(defSec);
                defSec.lpSecurityDescriptor = IntPtr.Zero;
                defSec.bInheritHandle = 0;

                // Create an environment block
                if (!SafeNativeMethods.CreateEnvironmentBlock(ref environmentBlock, token, false))
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "CreateEnvironmentBlock");

                // Now we can finally get into the business at hand and setup our process info
                SafeNativeMethods.STARTUPINFO startInfo = new SafeNativeMethods.STARTUPINFO();
                startInfo.cb = Marshal.SizeOf(startInfo);
                startInfo.wShowWindow = 0;
                startInfo.lpDesktop = "Winsta0\\Default"; // TBD: Support other desktops?

                SafeNativeMethods.PROCESS_INFORMATION procInfo = new SafeNativeMethods.PROCESS_INFORMATION();

                if (!SafeNativeMethods.CreateProcessAsUser(token, appName, cmdLine,
                                    ref defSec, ref defSec, false, SafeNativeMethods.CREATE_UNICODE_ENVIRONMENT,
                                    environmentBlock, null, ref startInfo, out procInfo))
                {
                    int lastError = Marshal.GetLastWin32Error();
                    throw new Win32Exception(lastError, "CreateProcessAsUser");
                }

                // We made it, process is running! Closing our handles to it ensures it doesn't orphan,
                // then we just use its pid to return a process object
                SafeNativeMethods.CloseHandle(procInfo.hProcess);
                SafeNativeMethods.CloseHandle(procInfo.hThread);

                return System.Diagnostics.Process.GetProcessById((int)procInfo.dwProcessId);
            }
            finally
            {
                SafeNativeMethods.CloseHandle(environmentBlock);
            }
        }
    }
}