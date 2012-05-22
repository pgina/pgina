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
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Reflection;
using System.Net;
using System.Net.Security;
using System.Security.Principal;
using System.ComponentModel;

namespace Abstractions.WindowsApi
{
    public class pInvokes
    {
        internal class SafeNativeMethods
        {
            #region Structs/Enums
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            public struct CREDUI_INFO
            {
                public int cbSize;
                public IntPtr hwndParent;
                public string pszMessageText;
                public string pszCaptionText;
                public IntPtr hbmBanner;
            }

            public enum PromptForWindowsCredentialsFlags
            {
                /// <summary>
                /// The caller is requesting that the credential provider return the user name and password in plain text.
                /// This value cannot be combined with SECURE_PROMPT.
                /// </summary>
                CREDUIWIN_GENERIC = 0x1,
                /// <summary>
                /// The Save check box is displayed in the dialog box.
                /// </summary>
                CREDUIWIN_CHECKBOX = 0x2,
                /// <summary>
                /// Only credential providers that support the authentication package specified by the authPackage parameter should be enumerated.
                /// This value cannot be combined with CREDUIWIN_IN_CRED_ONLY.
                /// </summary>
                CREDUIWIN_AUTHPACKAGE_ONLY = 0x10,
                /// <summary>
                /// Only the credentials specified by the InAuthBuffer parameter for the authentication package specified by the authPackage parameter should be enumerated.
                /// If this flag is set, and the InAuthBuffer parameter is NULL, the function fails.
                /// This value cannot be combined with CREDUIWIN_AUTHPACKAGE_ONLY.
                /// </summary>
                CREDUIWIN_IN_CRED_ONLY = 0x20,
                /// <summary>
                /// Credential providers should enumerate only administrators. This value is intended for User Account Control (UAC) purposes only. We recommend that external callers not set this flag.
                /// </summary>
                CREDUIWIN_ENUMERATE_ADMINS = 0x100,
                /// <summary>
                /// Only the incoming credentials for the authentication package specified by the authPackage parameter should be enumerated.
                /// </summary>
                CREDUIWIN_ENUMERATE_CURRENT_USER = 0x200,
                /// <summary>
                /// The credential dialog box should be displayed on the secure desktop. This value cannot be combined with CREDUIWIN_GENERIC.
                /// Windows Vista: This value is not supported until Windows Vista with SP1.
                /// </summary>
                CREDUIWIN_SECURE_PROMPT = 0x1000,
                /// <summary>
                /// The credential provider should align the credential BLOB pointed to by the refOutAuthBuffer parameter to a 32-bit boundary, even if the provider is running on a 64-bit system.
                /// </summary>
                CREDUIWIN_PACK_32_WOW = 0x10000000,
            }
            
            public enum ResourceScope
            {
                RESOURCE_CONNECTED = 1,
                RESOURCE_GLOBALNET,
                RESOURCE_REMEMBERED,
                RESOURCE_RECENT,
                RESOURCE_CONTEXT
            }

            public enum ResourceType
            {
                RESOURCETYPE_ANY,
                RESOURCETYPE_DISK,
                RESOURCETYPE_PRINT,
                RESOURCETYPE_RESERVED
            }

            public enum ResourceUsage
            {
                RESOURCEUSAGE_CONNECTABLE = 0x00000001,
                RESOURCEUSAGE_CONTAINER = 0x00000002,
                RESOURCEUSAGE_NOLOCALDEVICE = 0x00000004,
                RESOURCEUSAGE_SIBLING = 0x00000008,
                RESOURCEUSAGE_ATTACHED = 0x00000010,
                RESOURCEUSAGE_ALL = (RESOURCEUSAGE_CONNECTABLE | RESOURCEUSAGE_CONTAINER | RESOURCEUSAGE_ATTACHED),
            }

            public enum ResourceDisplayType
            {
                RESOURCEDISPLAYTYPE_GENERIC,
                RESOURCEDISPLAYTYPE_DOMAIN,
                RESOURCEDISPLAYTYPE_SERVER,
                RESOURCEDISPLAYTYPE_SHARE,
                RESOURCEDISPLAYTYPE_FILE,
                RESOURCEDISPLAYTYPE_GROUP,
                RESOURCEDISPLAYTYPE_NETWORK,
                RESOURCEDISPLAYTYPE_ROOT,
                RESOURCEDISPLAYTYPE_SHAREADMIN,
                RESOURCEDISPLAYTYPE_DIRECTORY,
                RESOURCEDISPLAYTYPE_TREE,
                RESOURCEDISPLAYTYPE_NDSCONTAINER
            }

            [StructLayout(LayoutKind.Sequential)]
            internal class NETRESOURCE
            {
                public ResourceScope dwScope = 0; // Ignored by WNetAddConnection2
                public ResourceType dwType = ResourceType.RESOURCETYPE_DISK;
                public ResourceDisplayType dwDisplayType = 0; // Ignored by WNetAddConnection2
                public ResourceUsage dwUsage = 0;  // Ignored by WNetAddConnection2
                public string lpLocalName = null;
                public string lpRemoteName = null;
                public string lpComment = "";  // Ignored by WNetAddConnection2
                public string lpProvider = null;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct SECURITY_ATTRIBUTES
            {
                public int nLength;
                public IntPtr lpSecurityDescriptor;
                public int bInheritHandle;
            }

            public enum SECURITY_IMPERSONATION_LEVEL
            {
                SecurityAnonymous,
                SecurityIdentification,
                SecurityImpersonation,
                SecurityDelegation
            }

            [Flags]
            public enum ACCESS_MASK : uint
            {
                DELETE = 0x00010000,
                READ_CONTROL = 0x00020000,
                WRITE_DAC = 0x00040000,
                WRITE_OWNER = 0x00080000,
                SYNCHRONIZE = 0x00100000,
                STANDARD_RIGHTS_REQUIRED = 0x000f0000,
                STANDARD_RIGHTS_READ = 0x00020000,
                STANDARD_RIGHTS_WRITE = 0x00020000,
                STANDARD_RIGHTS_EXECUTE = 0x00020000,
                STANDARD_RIGHTS_ALL = 0x001f0000,
                SPECIFIC_RIGHTS_ALL = 0x0000ffff,
                ACCESS_SYSTEM_SECURITY = 0x01000000,
                MAXIMUM_ALLOWED = 0x02000000,
                GENERIC_READ = 0x80000000,
                GENERIC_WRITE = 0x40000000,
                GENERIC_EXECUTE = 0x20000000,
                GENERIC_ALL = 0x10000000,
                DESKTOP_READOBJECTS = 0x00000001,
                DESKTOP_CREATEWINDOW = 0x00000002,
                DESKTOP_CREATEMENU = 0x00000004,
                DESKTOP_HOOKCONTROL = 0x00000008,
                DESKTOP_JOURNALRECORD = 0x00000010,
                DESKTOP_JOURNALPLAYBACK = 0x00000020,
                DESKTOP_ENUMERATE = 0x00000040,
                DESKTOP_WRITEOBJECTS = 0x00000080,
                DESKTOP_SWITCHDESKTOP = 0x00000100,
                WINSTA_ENUMDESKTOPS = 0x00000001,
                WINSTA_READATTRIBUTES = 0x00000002,
                WINSTA_ACCESSCLIPBOARD = 0x00000004,
                WINSTA_CREATEDESKTOP = 0x00000008,
                WINSTA_WRITEATTRIBUTES = 0x00000010,
                WINSTA_ACCESSGLOBALATOMS = 0x00000020,
                WINSTA_EXITWINDOWS = 0x00000040,
                WINSTA_ENUMERATE = 0x00000100,
                WINSTA_READSCREEN = 0x00000200,
                WINSTA_ALL_ACCESS = 0x0000037f
            }

            public enum TOKEN_TYPE
            {
                TokenPrimary = 1,
                TokenImpersonation
            }

            public enum LogonFlags
            {
                LOGON_WITH_PROFILE = 1,
                LOGON_NETCREDENTIALS_ONLY = 2,
            }

            public enum TOKEN_INFORMATION_CLASS : int
            {
                TokenUser = 1,
                TokenGroups,
                TokenPrivileges,
                TokenOwner,
                TokenPrimaryGroup,
                TokenDefaultDacl,
                TokenSource,
                TokenType,
                TokenImpersonationLevel,
                TokenStatistics,
                TokenRestrictedSids,
                TokenSessionId,
                TokenGroupsAndPrivileges,
                TokenSessionReference,
                TokenSandBoxInert,
                TokenAuditPolicy,
                TokenOrigin,
                MaxTokenInfoClass
            };

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

            [StructLayout(LayoutKind.Sequential)]
            public struct WTS_SESSION_INFO
            {
                public Int32 SessionID;

                [MarshalAs(UnmanagedType.LPStr)]
                public String pWinStationName;

                public WTS_CONNECTSTATE_CLASS State;
            }

            public enum WTS_INFO_CLASS
            {
                WTSInitialProgram,
                WTSApplicationName,
                WTSWorkingDirectory,
                WTSOEMId,
                WTSSessionId,
                WTSUserName,
                WTSWinStationName,
                WTSDomainName,
                WTSConnectState,
                WTSClientBuildNumber,
                WTSClientName,
                WTSClientDirectory,
                WTSClientProductId,
                WTSClientHardwareId,
                WTSClientAddress,
                WTSClientDisplay,
                WTSClientProtocolType
            }
            public enum WTS_CONNECTSTATE_CLASS
            {
                WTSActive,
                WTSConnected,
                WTSConnectQuery,
                WTSShadow,
                WTSDisconnected,
                WTSIdle,
                WTSListen,
                WTSReset,
                WTSDown,
                WTSInit
            }

            public static IntPtr WTS_CURRENT_SERVER_HANDLE = IntPtr.Zero;

            /// <summary>
            /// Used with LogonUser
            /// </summary>
            public enum LogonType
            {
                /// <summary>
                /// This logon type is intended for users who will be interactively using the computer, such as a user being logged on  
                /// by a terminal server, remote shell, or similar process.
                /// This logon type has the additional expense of caching logon information for disconnected operations;
                /// therefore, it is inappropriate for some client/server applications,
                /// such as a mail server.
                /// </summary>
                LOGON32_LOGON_INTERACTIVE = 2,

                /// <summary>
                /// This logon type is intended for high performance servers to authenticate plaintext passwords.
                /// The LogonUser function does not cache credentials for this logon type.
                /// </summary>
                LOGON32_LOGON_NETWORK = 3,

                /// <summary>
                /// This logon type is intended for batch servers, where processes may be executing on behalf of a user without 
                /// their direct intervention. This type is also for higher performance servers that process many plaintext
                /// authentication attempts at a time, such as mail or Web servers. 
                /// The LogonUser function does not cache credentials for this logon type.
                /// </summary>
                LOGON32_LOGON_BATCH = 4,

                /// <summary>
                /// Indicates a service-type logon. The account provided must have the service privilege enabled. 
                /// </summary>
                LOGON32_LOGON_SERVICE = 5,

                /// <summary>
                /// This logon type is for GINA DLLs that log on users who will be interactively using the computer. 
                /// This logon type can generate a unique audit record that shows when the workstation was unlocked. 
                /// </summary>
                LOGON32_LOGON_UNLOCK = 7,

                /// <summary>
                /// This logon type preserves the name and password in the authentication package, which allows the server to make 
                /// connections to other network servers while impersonating the client. A server can accept plaintext credentials 
                /// from a client, call LogonUser, verify that the user can access the system across the network, and still 
                /// communicate with other servers.
                /// NOTE: Windows NT:  This value is not supported. 
                /// </summary>
                LOGON32_LOGON_NETWORK_CLEARTEXT = 8,

                /// <summary>
                /// This logon type allows the caller to clone its current token and specify new credentials for outbound connections.
                /// The new logon session has the same local identifier but uses different credentials for other network connections. 
                /// NOTE: This logon type is supported only by the LOGON32_PROVIDER_WINNT50 logon provider.
                /// NOTE: Windows NT:  This value is not supported. 
                /// </summary>
                LOGON32_LOGON_NEW_CREDENTIALS = 9,
            }

            /// <summary>
            /// Used with LogonUser
            /// </summary>
            public enum LogonProvider
            {
                /// <summary>
                /// Use the standard logon provider for the system. 
                /// The default security provider is negotiate, unless you pass NULL for the domain name and the user name 
                /// is not in UPN format. In this case, the default provider is NTLM. 
                /// NOTE: Windows 2000/NT:   The default security provider is NTLM.
                /// </summary>
                LOGON32_PROVIDER_DEFAULT = 0,
            }
            #endregion            

            #region credui.dll
            [DllImport("credui.dll", CharSet = CharSet.Auto)]
            public static extern int CredUIPromptForWindowsCredentials(ref CREDUI_INFO uiInfo, int authError, ref uint authPackage,
                                                                         IntPtr InAuthBuffer, uint InAuthBufferSize,
                                                                         out IntPtr refOutAuthBuffer, out uint refOutAuthBufferSize,
                                                                         ref bool fSave, PromptForWindowsCredentialsFlags flags);

            [DllImport("credui.dll", CharSet = CharSet.Auto)]
            public static extern bool CredUnPackAuthenticationBuffer(int dwFlags, IntPtr pAuthBuffer, uint cbAuthBuffer,                                                                       
                                                                       StringBuilder pszUserName, ref int pcchMaxUserName,                                                                       
                                                                       StringBuilder pszDomainName, ref int pcchMaxDomainname,                                                                       
                                                                       StringBuilder pszPassword, ref int pcchMaxPassword);                                                                       
            #endregion

            #region ole32.dll
            [DllImport("ole32.dll")]
            public static extern void CoTaskMemFree(IntPtr ptr);
            #endregion

            #region mpr.dll
            [DllImport("mpr.dll")]
            public static extern int WNetAddConnection2(NETRESOURCE netResource,
                            string password, string username, int flags);

            [DllImport("mpr.dll")]
            public static extern int WNetCancelConnection2(string name, int flags,
                            bool force);
            #endregion

            #region wtsapi32.dll
            [DllImport("wtsapi32.dll", SetLastError = true)]            
            public static extern bool WTSQueryUserToken(int sessionId, out IntPtr Token);

            [DllImport("wtsapi32.dll")]
            public static extern IntPtr WTSOpenServer([MarshalAs(UnmanagedType.LPStr)] String pServerName);

            [DllImport("wtsapi32.dll")]
            public static extern void WTSCloseServer(IntPtr hServer);

            [DllImport("wtsapi32.dll")]
            public static extern int WTSEnumerateSessions(IntPtr hServer,
                [MarshalAs(UnmanagedType.U4)] int Reserved,
                [MarshalAs(UnmanagedType.U4)] int Version,
                ref IntPtr ppSessionInfo,
                [MarshalAs(UnmanagedType.U4)] ref int pCount);

            [DllImport("wtsapi32.dll")]
            public static extern void WTSFreeMemory(IntPtr pMemory);

            [DllImport("Wtsapi32.dll")]
            public static extern bool WTSQuerySessionInformation(System.IntPtr hServer, int sessionId, WTS_INFO_CLASS wtsInfoClass, out System.IntPtr ppBuffer, out uint pBytesReturned);

            [DllImport("wtsapi32.dll")]
            public static extern bool WTSLogoffSession(IntPtr hServer, int sessionId, bool bWait);
            #endregion

            #region userenv.dll
            [DllImport("userenv.dll")]
            public static extern bool DeleteProfile(string sidString, string path, string machine);            

            #endregion

            #region kernel32.dll
            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool CloseHandle(IntPtr hObject);
            #endregion

            #region advapi32.dll
            [DllImport("advapi32.dll", SetLastError = true)]
            public extern static bool DuplicateTokenEx(IntPtr hExistingToken, uint dwDesiredAccess, IntPtr tokenAttr, 
                SECURITY_IMPERSONATION_LEVEL ImpersonationLevel, TOKEN_TYPE TokenType, out IntPtr phNewToken);

            [DllImport("advapi32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool OpenProcessToken(IntPtr ProcessHandle,
                                                        UInt32 DesiredAccess, out IntPtr TokenHandle);
            
            // TokenInformation is really an IntPtr, but we only ever call this with SessionId, so we ref the int directly
            [DllImport("advapi32.dll", SetLastError = true)]
            public static extern bool SetTokenInformation(IntPtr TokenHandle, TOKEN_INFORMATION_CLASS TokenInformationClass,
                                                           ref int TokenInformation, int TokenInformationLength);

            [DllImport("userenv.dll", SetLastError = true)]
            public static extern Boolean CreateEnvironmentBlock(ref IntPtr lpEnvironment, IntPtr hToken, Boolean bInherit);

            [DllImport("advapi32.dll", SetLastError = true)]
            public static extern bool CreateProcessAsUser(IntPtr hToken, string lpApplicationName, string lpCommandLine, ref SECURITY_ATTRIBUTES lpProcessAttributes,
                                                           ref SECURITY_ATTRIBUTES lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment,
                                                           string lpCurrentDirectory, ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

            [DllImport("advapi32.dll", SetLastError = true)]
            public static extern bool LogonUser(
                string lpszUsername, string domain, string password,
                int dwLogonType, int dwLogonProvider,
                out IntPtr phToken);
            #endregion
        }

        public static IntPtr WTSQueryUserToken(int sessionId)
        {
            IntPtr result = IntPtr.Zero;
            if (SafeNativeMethods.WTSQueryUserToken(sessionId, out result))
                return result;
            return IntPtr.Zero;        
        }

        public static bool DeleteProfile(SecurityIdentifier sid)
        {
            return SafeNativeMethods.DeleteProfile(sid.ToString(), null, null);
        }

        public static List<string> GetInteractiveUserList()
        {            
            List<string> result = new List<string>();                          

            IntPtr sessionInfoList = IntPtr.Zero;
            int sessionCount = 0;
            int retVal = SafeNativeMethods.WTSEnumerateSessions(SafeNativeMethods.WTS_CURRENT_SERVER_HANDLE, 0, 1, ref sessionInfoList, ref sessionCount);

            if(retVal != 0)
            {                
                int dataSize = Marshal.SizeOf(typeof(SafeNativeMethods.WTS_SESSION_INFO));
                int currentSession = (int) sessionInfoList;                

                for(int x = 0; x < sessionCount; x++)
                {
                    SafeNativeMethods.WTS_SESSION_INFO sessionInfo = 
                        (SafeNativeMethods.WTS_SESSION_INFO)Marshal.PtrToStructure((IntPtr)currentSession, typeof(SafeNativeMethods.WTS_SESSION_INFO));
                    currentSession += dataSize;

                    uint bytes = 0;
                    IntPtr userInfo = IntPtr.Zero;
                    IntPtr domainInfo = IntPtr.Zero;
                    bool sResult = SafeNativeMethods.WTSQuerySessionInformation(SafeNativeMethods.WTS_CURRENT_SERVER_HANDLE, sessionInfo.SessionID, SafeNativeMethods.WTS_INFO_CLASS.WTSUserName, out userInfo, out bytes);
                    if (!sResult)
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error(), "WTSQuerySessionInformation");
                    }
                    string user = Marshal.PtrToStringAnsi(userInfo);
                    SafeNativeMethods.WTSFreeMemory(userInfo);

                    sResult = SafeNativeMethods.WTSQuerySessionInformation(SafeNativeMethods.WTS_CURRENT_SERVER_HANDLE, sessionInfo.SessionID, SafeNativeMethods.WTS_INFO_CLASS.WTSDomainName, out domainInfo, out bytes);
                    if (!sResult)
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error(), "WTSQuerySessionInformation");
                    }

                    string domain = Marshal.PtrToStringAnsi(domainInfo);
                    SafeNativeMethods.WTSFreeMemory(domainInfo);
                    
                    if (!string.IsNullOrEmpty(domain))
                    {
                        result.Add(string.Format("{0}\\{1}", domain, user));
                    }
                    else if (!string.IsNullOrEmpty(user))
                    {
                        result.Add(user);
                    }
                }

                SafeNativeMethods.WTSFreeMemory(sessionInfoList);
            }

            return result;            
        }

        public static string GetUserName(int sessionId)
        {
            uint bytes = 0;
            IntPtr userInfo = IntPtr.Zero;
            bool result = SafeNativeMethods.WTSQuerySessionInformation(SafeNativeMethods.WTS_CURRENT_SERVER_HANDLE,
                sessionId, SafeNativeMethods.WTS_INFO_CLASS.WTSUserName, out userInfo, out bytes);

            if (!result)
                throw new Win32Exception(Marshal.GetLastWin32Error(), "WTSQuerySessionInformation");
            
            string userName = Marshal.PtrToStringAnsi(userInfo);
            SafeNativeMethods.WTSFreeMemory(userInfo);
            return userName;
        }

        public static bool CloseHandle(IntPtr handle)
        {
            return SafeNativeMethods.CloseHandle(handle);
        }

        public static System.Diagnostics.Process StartProcessInSession(int sessionId, string cmdLine)
        {           
            IntPtr processToken = IntPtr.Zero;
            IntPtr duplicateToken = IntPtr.Zero;            

            try
            {
                // Get our current process token
                using (System.Diagnostics.Process me = System.Diagnostics.Process.GetCurrentProcess())
                {
                    if (!SafeNativeMethods.OpenProcessToken(me.Handle, (uint)TokenAccessLevels.AllAccess, out processToken))
                        throw new Win32Exception(Marshal.GetLastWin32Error(), "Unable to open process token");
                }

                // Duplicate it, so we can change it's session id
                if (!SafeNativeMethods.DuplicateTokenEx(processToken, (uint)SafeNativeMethods.ACCESS_MASK.MAXIMUM_ALLOWED, IntPtr.Zero,
                     SafeNativeMethods.SECURITY_IMPERSONATION_LEVEL.SecurityIdentification, SafeNativeMethods.TOKEN_TYPE.TokenPrimary, out duplicateToken))
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "DuplicateTokenEx");

                // Poke the session id we want into our new token
                if (!SafeNativeMethods.SetTokenInformation(duplicateToken, SafeNativeMethods.TOKEN_INFORMATION_CLASS.TokenSessionId, ref sessionId, Marshal.SizeOf(sessionId)))
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "SetTokenInformation");

                return StartProcessWithToken(duplicateToken, cmdLine);
            }
            finally
            {
                SafeNativeMethods.CloseHandle(processToken);
                SafeNativeMethods.CloseHandle(duplicateToken);                
            }            
        }

        public static System.Diagnostics.Process StartUserProcessInSession(int sessionId, string cmdLine)
        {
            IntPtr processToken = IntPtr.Zero;            

            try
            {
                // Get user's token from session id, WTSQueryUserToken already returns a primary token for us
                //  so all we then have to do is use it to start the process.
                if (!SafeNativeMethods.WTSQueryUserToken(sessionId, out processToken))
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "WTSQueryUserToken");

                return StartProcessWithToken(processToken, cmdLine);
            }
            finally
            {
                SafeNativeMethods.CloseHandle(processToken);                
            }
        }

        public static System.Diagnostics.Process StartProcessWithToken(IntPtr token, string cmdLine)
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
                startInfo.lpDesktop = "Winsta0\\Default";   // TBD: Support other desktops?

                SafeNativeMethods.PROCESS_INFORMATION procInfo = new SafeNativeMethods.PROCESS_INFORMATION();
                if (!SafeNativeMethods.CreateProcessAsUser(token, null, cmdLine,
                                    ref defSec, ref defSec, false, SafeNativeMethods.CREATE_UNICODE_ENVIRONMENT,
                                    environmentBlock, null, ref startInfo, out procInfo))
                {
                    int lastError = Marshal.GetLastWin32Error();
                    throw new Win32Exception(lastError, "CreateProcessAsUser");
                }

                // We made it, process is running! Closing our handles to it ensures it doesn't orphan,
                //  then we just use its pid to return a process object
                SafeNativeMethods.CloseHandle(procInfo.hProcess);
                SafeNativeMethods.CloseHandle(procInfo.hThread);

                return System.Diagnostics.Process.GetProcessById((int)procInfo.dwProcessId);
            }
            finally
            {
                SafeNativeMethods.CloseHandle(environmentBlock);
            }
        }

        public static NetworkCredential GetCredentials(string caption, string message)
        {
            SafeNativeMethods.CREDUI_INFO uiInfo = new SafeNativeMethods.CREDUI_INFO();
            uiInfo.cbSize = Marshal.SizeOf(uiInfo);
            uiInfo.pszCaptionText = caption;
            uiInfo.pszMessageText = message;            
            
            uint authPackage = 0;
            IntPtr outCredBuffer = new IntPtr();
            uint outCredSize;
            bool save = false;
            int result = SafeNativeMethods.CredUIPromptForWindowsCredentials(ref uiInfo, 0, ref authPackage,
                                                           IntPtr.Zero, 0, out outCredBuffer, out outCredSize, ref save, 0);
                                                            

            var usernameBuf = new StringBuilder(100);
            var passwordBuf = new StringBuilder(100);
            var domainBuf = new StringBuilder(100);
            int maxUserName = 100;
            int maxDomain = 100;
            int maxPassword = 100;

            if (result == 0)
            {
                if (SafeNativeMethods.CredUnPackAuthenticationBuffer(0, outCredBuffer, outCredSize, usernameBuf, ref maxUserName,
                                                                     domainBuf, ref maxDomain, passwordBuf, ref maxPassword))
                {
                    SafeNativeMethods.CoTaskMemFree(outCredBuffer);
                    return new NetworkCredential()
                    {
                        UserName = usernameBuf.ToString(),
                        Password = passwordBuf.ToString(),
                        Domain = domainBuf.ToString()
                    };                    
                }
            }

            return null;
        }

        /// <summary>
        /// Map a network drive.
        /// </summary>
        /// <param name="unc">The full UNC path.</param>
        /// <param name="drive">The drive letter (e.g. "Z:", "X:", etc.)</param>
        /// <param name="user">The username, null if you want the current user.</param>
        /// <param name="password">The password, null to use the default password.</param>
        /// <returns>The error code of the WNetAddConnection2 function.</returns>
        public static int MapNetworkDrive(string unc, string drive, string user, string password)
        {
            SafeNativeMethods.NETRESOURCE myNetResource = new SafeNativeMethods.NETRESOURCE();
            myNetResource.lpLocalName = drive;
            myNetResource.lpRemoteName = unc;
            int result = SafeNativeMethods.WNetAddConnection2(myNetResource, password, user, 0);
            return result;
        }

        public static bool LogoffSession(int sessionId)
        {
            return SafeNativeMethods.WTSLogoffSession(SafeNativeMethods.WTS_CURRENT_SERVER_HANDLE, sessionId, false);
        }

        /// <summary>
        /// Attempts to validate the user's credentials for a local account using 
        /// a pInvoke to LogonUser.
        /// </summary>
        /// <param name="username">The username</param>
        /// <param name="password">The password</param>
        /// <returns>True if the account credentials are valid</returns>
        public static bool ValidateCredentials(string username, string password)
        {
            return ValidateCredentials(username,"",password);
        }

        /// <summary>
        /// Attempts to validate the user's credentials using 
        /// a pInvoke to LogonUser.
        /// </summary>
        /// <param name="username">The username</param>
        /// <param name="domain">The domain</param>
        /// <param name="password">The password</param>
        /// <returns>True if the account credentials are valid</returns>
        public static bool ValidateCredentials(string username, string domain, string password)
        {
            IntPtr hToken = IntPtr.Zero;
            bool result = SafeNativeMethods.LogonUser(username, domain, password,
                (int)SafeNativeMethods.LogonType.LOGON32_LOGON_NETWORK,
                (int)SafeNativeMethods.LogonProvider.LOGON32_PROVIDER_DEFAULT,
                out hToken);

            if (hToken != IntPtr.Zero) CloseHandle(hToken);

            return result;
        }
    }
}
