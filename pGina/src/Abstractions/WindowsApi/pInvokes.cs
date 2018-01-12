/*
	Copyright (c) 2018, pGina Team
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
using Microsoft.Win32;
using System.Security.AccessControl;

using Abstractions.Logging;

namespace Abstractions.WindowsApi
{
    public static class pInvokes
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
                LOGON32_PROVIDER_WINNT35 = 1,
                LOGON32_PROVIDER_WINNT40 = 2,
                LOGON32_PROVIDER_WINNT50 = 3,
                LOGON32_PROVIDER_VIRTUAL = 4
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

            [DllImport("kernel32.dll")]
            [return: MarshalAs(UnmanagedType.U4)]
            public static extern int WTSGetActiveConsoleSessionId();
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
                                                           ref SECURITY_ATTRIBUTES lpThreadAttributes, bool bInheritHandles, CreationFlags dwCreationFlags, IntPtr lpEnvironment,
                                                           string lpCurrentDirectory, ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

            [DllImport("advapi32.dll", SetLastError = true)]
            public static extern bool LogonUser(
                string lpszUsername, string domain, string password,
                int dwLogonType, int dwLogonProvider,
                out IntPtr phToken);
            #endregion

            #region ntdll.dll
            [DllImport("ntdll.dll", SetLastError = true)]
            internal static extern int RtlGetVersion(ref structenums.OSVERSIONINFOW version);
            #endregion

            #region Netapi32.dll
            [DllImport("Netapi32.dll", SetLastError = true)]
            internal static extern int NetUserAdd([MarshalAs(UnmanagedType.LPWStr)]string servername, UInt32 level, ref SafeNativeMethods.USER_INFO_1 buf, out Int32 parm_err);

            [DllImport("Netapi32.dll", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true)]
            internal extern static int NetUserGetInfo([MarshalAs(UnmanagedType.LPWStr)] string ServerName, [MarshalAs(UnmanagedType.LPWStr)] string UserName, int level, out IntPtr BufPtr);

            [DllImport("netapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern int NetUserSetInfo([MarshalAs(UnmanagedType.LPWStr)] string servername, string username, int level, ref structenums.USER_INFO_4 buf, out Int32 parm_err);

            [DllImport("Netapi32.dll", SetLastError = true)]
            internal static extern int NetApiBufferFree(IntPtr Buffer);

            [DllImport("Netapi32.dll", SetLastError = true)]
            internal static extern int NetUserDel([MarshalAs(UnmanagedType.LPWStr)] string servername, [MarshalAs(UnmanagedType.LPWStr)] string username);

            [DllImport("Netapi32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
            internal static extern int NetUserChangePassword([MarshalAs(UnmanagedType.LPWStr)] string domainname, [MarshalAs(UnmanagedType.LPWStr)] string username, [MarshalAs(UnmanagedType.LPWStr)] string oldpassword, [MarshalAs(UnmanagedType.LPWStr)] string newpassword);

            [DllImport("Netapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            internal static extern int DsGetDcName([MarshalAs(UnmanagedType.LPTStr)] string ComputerName, [MarshalAs(UnmanagedType.LPTStr)] string DomainName, [In] int DomainGuid, [MarshalAs(UnmanagedType.LPTStr)] string SiteName, [MarshalAs(UnmanagedType.U4)] DSGETDCNAME_FLAGS flags, out IntPtr pDOMAIN_CONTROLLER_INFO);

            [DllImport("netapi32.dll", SetLastError = true)]
            internal static extern int NetWkstaGetInfo(string servername, int level, out IntPtr bufptr);
            #endregion

            #region wtsapi32.dll
            [DllImport("wtsapi32.dll", SetLastError = true)]
            internal static extern bool WTSSendMessage(IntPtr hServer, [MarshalAs(UnmanagedType.I4)] int SessionId, String pTitle, [MarshalAs(UnmanagedType.U4)] int TitleLength, String pMessage, [MarshalAs(UnmanagedType.U4)] int MessageLength, [MarshalAs(UnmanagedType.U4)] int Style, [MarshalAs(UnmanagedType.U4)] int Timeout, [MarshalAs(UnmanagedType.U4)] out int pResponse, bool bWait);
            #endregion

            [DllImport("Mpr.dll", SetLastError = true)]
            internal static extern int WNetUseConnection(IntPtr hwndOwner, NETRESOURCE lpNetResource, string lpPassword, string lpUserID, int dwFlags, string lpAccessName, string lpBufferSize, string lpResult);

            #region kernel32.dll
            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            internal static extern bool GetDiskFreeSpaceEx(string drive, out long freeBytesForUser, out long totalBytes, out long freeBytes);
            #endregion

            #region kernel32.dll
            [DllImport("kernel32.dll", SetLastError = true)]
            internal static extern IntPtr GetCurrentProcess();
            #endregion

            #region advapi32.dll
            [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool LookupPrivilegeValue(string lpSystemName, string lpName, out LUID lpLuid);

            [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
            internal static extern bool AdjustTokenPrivileges(IntPtr htok, bool disableAllPrivileges, ref TokPriv1Luid newState, int len, IntPtr prev, IntPtr relen);

            [DllImport("advapi32.dll", SetLastError = true)]
            internal static extern int RegLoadKey(UInt32 hKey, String lpSubKey, String lpFile);

            [DllImport("advapi32.dll", SetLastError = true)]
            internal static extern int RegUnLoadKey(UInt32 hKey, string lpSubKey);

            [DllImport("userenv.dll", SetLastError = true, CharSet = CharSet.Auto)]
            internal static extern uint CreateProfile([MarshalAs(UnmanagedType.LPWStr)] String pszUserSid, [MarshalAs(UnmanagedType.LPWStr)] String pszUserName, [Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder pszProfilePath, uint cchProfilePath);

            [DllImport("userenv.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            internal static extern bool GetUserProfileDirectory(IntPtr hToken, StringBuilder lpProfileDir, [MarshalAs(UnmanagedType.U4)] ref uint lpcchSize);

            [DllImport("shell32.dll", SetLastError = true)]
            internal static extern int SHGetFolderPath(IntPtr hwndOwner, int nFolder, IntPtr hToken, uint dwFlags, [Out] StringBuilder pszPath);

            [DllImport("Advapi32.dll", SetLastError = true)]
            internal static extern int RegCopyTree(UIntPtr hsKey, string lpSubKey, UIntPtr hdKey);

            [DllImport("Advapi32.dll", SetLastError = true)]
            internal static extern int RegCloseKey(UIntPtr hKey);

            [DllImport("Advapi32.dll", SetLastError = true)]
            internal static extern int RegCreateKey(structenums.baseKey hKey, [MarshalAs(UnmanagedType.LPStr)]string subKey, ref UIntPtr phkResult);

            [DllImport("Advapi32.dll", SetLastError = true)]
            internal static extern int RegOpenKey(structenums.baseKey hKey, [MarshalAs(UnmanagedType.LPStr)]string subKey, ref UIntPtr phkResult);

            [DllImport("Advapi32.dll", SetLastError = true)]
            internal static extern int RegDeleteTree(structenums.baseKey hKey, [MarshalAs(UnmanagedType.LPStr)]string subKey);
            #endregion

            [DllImport("advapi32.dll", SetLastError = true)]
            internal static extern bool GetTokenInformation(IntPtr TokenHandle, TOKEN_INFORMATION_CLASS TokenInformationClass, IntPtr TokenInformation, int TokenInformationLength, out int ReturnLength);

            [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            internal static extern bool LookupAccountSid(string lpSystemName, IntPtr Sid, StringBuilder lpName, ref uint cchName, StringBuilder ReferencedDomainName, ref uint cchReferencedDomainName, out SID_NAME_USE peUse);

            [DllImport("wtsapi32.dll", SetLastError = true)]
            internal static extern bool WTSRegisterSessionNotification(IntPtr hWnd, [MarshalAs(UnmanagedType.U4)] int dwFlags);

            [DllImport("WtsApi32.dll", SetLastError = true)]
            internal static extern bool WTSUnRegisterSessionNotification(IntPtr hWnd);

            [DllImport("advapi32.dll", SetLastError = true)]
            internal static extern bool SetServiceStatus(IntPtr hServiceStatus, ref structenums.SERVICE_STATUS lpServiceStatus);

            [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool ChangeServiceConfig2(SafeHandle hService, int dwInfoLevel, IntPtr lpInfo);

            [DllImport("userenv.dll", SetLastError = true, CharSet = CharSet.Auto)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool LoadUserProfile(IntPtr hToken, ref PROFILEINFO lpProfileInfo);

            [DllImport("userenv.dll", SetLastError = true, CharSet = CharSet.Auto)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool UnloadUserProfile(IntPtr hToken, IntPtr hProfile);

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool CreateProcess(string lpApplicationName, string lpCommandLine, ref SECURITY_ATTRIBUTES lpProcessAttributes, ref SECURITY_ATTRIBUTES lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, [In] ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            internal static extern Int32 WaitForSingleObject(IntPtr Handle, Int32 Wait);

            [DllImport("kernel32.dll", SetLastError = true)]
            internal static extern bool GetExitCodeProcess(IntPtr hProcess, out uint lpExitCode);

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            internal struct USER_INFO_1052
            {
                internal IntPtr profile;
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            internal struct USER_INFO_1
            {
                internal string usri1_name;
                internal string usri1_password;
                internal int usri1_password_age;
                internal pInvokes.structenums.UserPrivileges usri1_priv;
                internal string usri1_home_dir;
                internal string comment;
                internal int usri1_flags;
                internal string usri1_script_path;
            }

            internal struct Connect
            {
                internal const int CONNECT_INTERACTIVE = 0x00000008;
                internal const int CONNECT_PROMPT = 0x00000010;
                internal const int CONNECT_REDIRECT = 0x00000080;
                internal const int CONNECT_UPDATE_PROFILE = 0x00000001;
                internal const int CONNECT_COMMANDLINE = 0x00000800;
                internal const int CONNECT_CMD_SAVECRED = 0x00001000;
                internal const int CONNECT_LOCALDRIVE = 0x00000100;
            }

            internal struct TokenAccessRights
            {
                internal const string SE_RESTORE_NAME = "SeRestorePrivilege";
                internal const string SE_BACKUP_NAME = "SeBackupPrivilege";
                internal const UInt32 SE_PRIVILEGE_ENABLED = 0x00000002;
                internal const UInt32 STANDARD_RIGHTS_REQUIRED = 0x000F0000;
                internal const UInt32 STANDARD_RIGHTS_READ = 0x00020000;
                internal const UInt32 TOKEN_ASSIGN_PRIMARY = 0x0001;
                internal const UInt32 TOKEN_DUPLICATE = 0x0002;
                internal const UInt32 TOKEN_IMPERSONATE = 0x0004;
                internal const UInt32 TOKEN_QUERY = 0x0008;
                internal const UInt32 TOKEN_QUERY_SOURCE = 0x0010;
                internal const UInt32 TOKEN_ADJUST_PRIVILEGES = 0x0020;
                internal const UInt32 TOKEN_ADJUST_GROUPS = 0x0040;
                internal const UInt32 TOKEN_ADJUST_DEFAULT = 0x0080;
                internal const UInt32 TOKEN_ADJUST_SESSIONID = 0x0100;
                internal const UInt32 TOKEN_READ = (STANDARD_RIGHTS_READ | TOKEN_QUERY);
                internal const UInt32 TOKEN_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED | TOKEN_ASSIGN_PRIMARY |
                    TOKEN_DUPLICATE | TOKEN_IMPERSONATE | TOKEN_QUERY | TOKEN_QUERY_SOURCE |
                    TOKEN_ADJUST_PRIVILEGES | TOKEN_ADJUST_GROUPS | TOKEN_ADJUST_DEFAULT |
                    TOKEN_ADJUST_SESSIONID);
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct LUID
            {
                internal uint LowPart;
                internal int HighPart;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct LUID_AND_ATTRIBUTES
            {
                internal LUID pLuid;
                internal UInt32 Attributes;
            }

            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            internal struct TokPriv1Luid
            {
                internal int Count;
                internal LUID Luid;
                internal UInt32 Attr;
            }

            internal struct TOKEN_USER
            {
                #pragma warning disable 0649
                internal SID_AND_ATTRIBUTES User;
                #pragma warning restore 0649
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct SID_AND_ATTRIBUTES
            {
                internal IntPtr Sid;
                internal int Attributes;
            }

            internal enum SID_NAME_USE
            {
                SidTypeUser = 1,
                SidTypeGroup,
                SidTypeDomain,
                SidTypeAlias,
                SidTypeWellKnownGroup,
                SidTypeDeletedAccount,
                SidTypeInvalid,
                SidTypeUnknown,
                SidTypeComputer
            }

            internal enum State
            {
                SERVICE_STOPPED = 0x00000001,
                SERVICE_START_PENDING = 0x00000002,
                SERVICE_STOP_PENDING = 0x00000003,
                SERVICE_RUNNING = 0x00000004,
                SERVICE_CONTINUE_PENDING = 0x00000005,
                SERVICE_PAUSE_PENDING = 0x00000006,
                SERVICE_PAUSED = 0x00000007,
            }
            internal enum INFO_LEVEL : uint
            {
                SERVICE_CONFIG_DESCRIPTION = 0x00000001,
                SERVICE_CONFIG_FAILURE_ACTIONS = 0x00000002,
                SERVICE_CONFIG_DELAYED_AUTO_START_INFO = 0x00000003,
                SERVICE_CONFIG_FAILURE_ACTIONS_FLAG = 0x00000004,
                SERVICE_CONFIG_SERVICE_SID_INFO = 0x00000005,
                SERVICE_CONFIG_REQUIRED_PRIVILEGES_INFO = 0x00000006,
                SERVICE_CONFIG_PRESHUTDOWN_INFO = 0x00000007,
                SERVICE_CONFIG_TRIGGER_INFO = 0x00000008,
                SERVICE_CONFIG_PREFERRED_NODE = 0x00000009
            }
            [StructLayout(LayoutKind.Sequential)]
            internal struct SERVICE_PRESHUTDOWN_INFO
            {
                internal UInt32 dwPreshutdownTimeout;
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            internal struct DOMAIN_CONTROLLER_INFO
            {
                [MarshalAs(UnmanagedType.LPTStr)]
                public string DomainControllerName;
                [MarshalAs(UnmanagedType.LPTStr)]
                public string DomainControllerAddress;
                public uint DomainControllerAddressType;
                public Guid DomainGuid;
                [MarshalAs(UnmanagedType.LPTStr)]
                public string DomainName;
                [MarshalAs(UnmanagedType.LPTStr)]
                public string DnsForestName;
                public uint Flags;
                [MarshalAs(UnmanagedType.LPTStr)]
                public string DcSiteName;
                [MarshalAs(UnmanagedType.LPTStr)]
                public string ClientSiteName;
            }

            [Flags]
            internal enum DSGETDCNAME_FLAGS : uint
            {
                DS_FORCE_REDISCOVERY = 0x00000001,
                DS_DIRECTORY_SERVICE_REQUIRED = 0x00000010,
                DS_DIRECTORY_SERVICE_PREFERRED = 0x00000020,
                DS_GC_SERVER_REQUIRED = 0x00000040,
                DS_PDC_REQUIRED = 0x00000080,
                DS_BACKGROUND_ONLY = 0x00000100,
                DS_IP_REQUIRED = 0x00000200,
                DS_KDC_REQUIRED = 0x00000400,
                DS_TIMESERV_REQUIRED = 0x00000800,
                DS_WRITABLE_REQUIRED = 0x00001000,
                DS_GOOD_TIMESERV_PREFERRED = 0x00002000,
                DS_AVOID_SELF = 0x00004000,
                DS_ONLY_LDAP_NEEDED = 0x00008000,
                DS_IS_FLAT_NAME = 0x00010000,
                DS_IS_DNS_NAME = 0x00020000,
                DS_RETURN_DNS_NAME = 0x40000000,
                DS_RETURN_FLAT_NAME = 0x80000000
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            internal struct WKSTA_INFO_100
            {
                public int platform_id;
                public string computer_name;
                public string lan_group;
                public int ver_major;
                public int ver_minor;
            }

            [Flags]
            internal enum CreationFlags
            {
                CREATE_BREAKAWAY_FROM_JOB = 0x01000000,
                CREATE_DEFAULT_ERROR_MODE = 0x04000000,
                CREATE_NEW_CONSOLE = 0x00000010,
                CREATE_NEW_PROCESS_GROUP = 0x00000200,
                CREATE_NO_WINDOW = 0x08000000,
                CREATE_PROTECTED_PROCESS = 0x00040000,
                CREATE_PRESERVE_CODE_AUTHZ_LEVEL = 0x02000000,
                CREATE_SEPARATE_WOW_VDM = 0x00000800,
                CREATE_SHARED_WOW_VDM = 0x00001000,
                CREATE_SUSPENDED = 0x00000004,
                CREATE_UNICODE_ENVIRONMENT = 0x00000400,
                DEBUG_ONLY_THIS_PROCESS = 0x00000002,
                DEBUG_PROCESS = 0x00000001,
                DETACHED_PROCESS = 0x00000008,
                EXTENDED_STARTUPINFO_PRESENT = 0x00080000,
                INHERIT_PARENT_AFFINITY = 0x00010000,
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            internal struct STARTUPINFOEX
            {
                public STARTUPINFO StartupInfo;
                public IntPtr lpAttributeList;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct PROFILEINFO
            {
                internal int dwSize;
                internal int dwFlags;
                [MarshalAs(UnmanagedType.LPTStr)]
                internal String lpUserName;
                [MarshalAs(UnmanagedType.LPTStr)]
                internal String lpProfilePath;
                [MarshalAs(UnmanagedType.LPTStr)]
                internal String lpDefaultPath;
                [MarshalAs(UnmanagedType.LPTStr)]
                internal String lpServerName;
                [MarshalAs(UnmanagedType.LPTStr)]
                internal String lpPolicyPath;
                internal IntPtr hProfile;
            }

            internal static Boolean RegistryLoadPrivilegeSet = false;
        }

        public class structenums
        {
            [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
            public struct OSVERSIONINFOW
            {
                [MarshalAs(UnmanagedType.U4)]
                internal int dwOSVersionInfoSize;
                [MarshalAs(UnmanagedType.U4)]
                public int dwMajorVersion;
                [MarshalAs(UnmanagedType.U4)]
                public int dwMinorVersion;
                [MarshalAs(UnmanagedType.U4)]
                public int dwBuildNumber;
                [MarshalAs(UnmanagedType.U4)]
                public int dwPlatformId;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
                public char[] szCSDVersion;
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct USER_INFO_4
            {
                public string name;
                public string password;
                public int password_age;
                public UserPrivileges priv;
                public string home_dir;
                public string comment;
                public UserFlags flags;
                public string script_path;
                public AuthFlags auth_flags;
                public string full_name;
                public string usr_comment;
                public string parms;
                public string workstations;
                public int last_logon;
                public int last_logoff;
                public int acct_expires;
                public int max_storage;
                public int units_per_week;
                public IntPtr logon_hours;    // This is a PBYTE
                public int bad_pw_count;
                public int num_logons;
                public string logon_server;
                public int country_code;
                public int code_page;
                public IntPtr user_sid;     // This is a PSID
                public int primary_group_id;
                public string profile;
                public string home_dir_drive;
                public int password_expired;
            }
            public enum UserPrivileges
            {
                USER_PRIV_GUEST = 0x0,
                USER_PRIV_USER = 0x1,
                USER_PRIV_ADMIN = 0x2,
            }
            public enum UserFlags
            {
                UF_SCRIPT = 0x0001,
                UF_ACCOUNTDISABLE = 0x0002,
                UF_HOMEDIR_REQUIRED = 0x0008,
                UF_LOCKOUT = 0x0010,
                UF_PASSWD_NOTREQD = 0x0020,
                UF_PASSWD_CANT_CHANGE = 0x0040,
                UF_ENCRYPTED_TEXT_PASSWORD_ALLOWED = 0x0080,
                UF_TEMP_DUPLICATE_ACCOUNT = 0x0100,
                UF_NORMAL_ACCOUNT = 0x0200,
                UF_INTERDOMAIN_TRUST_ACCOUNT = 0x0800,
                UF_WORKSTATION_TRUST_ACCOUNT = 0x1000,
                UF_SERVER_TRUST_ACCOUNT = 0x2000,
                UF_DONT_EXPIRE_PASSWD = 0x10000,
                UF_MNS_LOGON_ACCOUNT = 0x20000,
                UF_SMARTCARD_REQUIRED = 0x40000,
                UF_TRUSTED_FOR_DELEGATION = 0x80000,
                UF_NOT_DELEGATED = 0x100000,
                UF_USE_DES_KEY_ONLY = 0x200000,
                UF_DONT_REQUIRE_PREAUTH = 0x400000,
                UF_PASSWORD_EXPIRED = 0x800000,
                UF_TRUSTED_TO_AUTHENTICATE_FOR_DELEGATION = 0x1000000,
                UF_PARTIAL_SECRETS_ACCOUNT = 0x04000000,
            }
            public enum AuthFlags
            {
                AF_OP_PRINT = 0x1,
                AF_OP_COMM = 0x2,
                AF_OP_SERVER = 0x4,
                AF_OP_ACCOUNTS = 0x8,
            }

            public enum RegistryLocation
            {
                HKEY_CLASSES_ROOT,
                HKEY_CURRENT_USER,
                HKEY_LOCAL_MACHINE,
                HKEY_USERS,
                HKEY_CURRENT_CONFIG
            }

            public enum baseKey : uint
            {
                HKEY_CLASSES_ROOT = 0x80000000,
                HKEY_CURRENT_USER = 0x80000001,
                HKEY_LOCAL_MACHINE = 0x80000002,
                HKEY_USERS = 0x80000003,
                HKEY_CURRENT_CONFIG = 0x80000005
            }

            public enum PBT
            {
                // Vista++ && XP++
                APMPOWERSTATUSCHANGE = 0xA, //Power status has changed.
                APMRESUMEAUTOMATIC = 0x12, //Operation is resuming automatically from a low-power state. This message is sent every time the system resumes.
                APMRESUMESUSPEND = 0x7, //Operation is resuming from a low-power state. This message is sent after PBT_APMRESUMEAUTOMATIC if the resume is triggered by user input, such as pressing a key.
                APMSUSPEND = 0x4, //System is suspending operation.
                POWERSETTINGCHANGE = 0x8013, //A power setting change event has been received.
                // XP++ only
                APMBATTERYLOW = 0x9, //Battery power is low. In Windows Server 2008 and Windows Vista, use PBT_APMPOWERSTATUSCHANGE instead.
                APMOEMEVENT = 0xB, //OEM-defined event occurred. In Windows Server 2008 and Windows Vista, this event is not available because these operating systems support only ACPI; APM BIOS events are not supported.
                APMQUERYSUSPEND = 0x0, //Request for permission to suspend. In Windows Server 2008 and Windows Vista, use the SetThreadExecutionState function instead.
                APMQUERYSUSPENDFAILED = 0x2, //Suspension request denied. In Windows Server 2008 and Windows Vista, use SetThreadExecutionState instead.
                APMRESUMECRITICAL = 0x6, //Operation resuming after critical suspension. In Windows Server 2008 and Windows Vista, use PBT_APMRESUMEAUTOMATIC instead.
            }

            public enum WTS
            {
                CONSOLE_CONNECT = 0x1, //The session identified by lParam was connected to the console terminal or RemoteFX session.
                CONSOLE_DISCONNECT = 0x2, //The session identified by lParam was disconnected from the console terminal or RemoteFX session.
                REMOTE_CONNECT = 0x3, //The session identified by lParam was connected to the remote terminal.
                REMOTE_DISCONNECT = 0x4, //The session identified by lParam was disconnected from the remote terminal.
                SESSION_LOGON = 0x5, //A user has logged on to the session identified by lParam.
                SESSION_LOGOFF = 0x6, //A user has logged off the session identified by lParam.
                SESSION_LOCK = 0x7, //The session identified by lParam has been locked.
                SESSION_UNLOCK = 0x8, //The session identified by lParam has been unlocked.
                SESSION_REMOTE_CONTROL = 0x9, //The session identified by lParam has changed its remote controlled status. To determine the status, call GetSystemMetrics and check the SM_REMOTECONTROL metric.
                SESSION_CREATE = 0xA, //Reserved for future use.
                SESSION_TERMINATE = 0xB, //Reserved for future use.
            }

            public enum DBT
            {
                CONFIGCHANGECANCELED = 0x0019, //A request to change the current configuration (dock or undock) has been canceled.
                CONFIGCHANGED = 0x0018, //The current configuration has changed, due to a dock or undock.
                CUSTOMEVENT = 0x8006, //A custom event has occurred.
                DEVICEARRIVAL = 0x8000, //A device or piece of media has been inserted and is now available.
                DEVICEQUERYREMOVE = 0x8001, //Permission is requested to remove a device or piece of media. Any application can deny this request and cancel the removal.
                DEVICEQUERYREMOVEFAILED = 0x8002, //A request to remove a device or piece of media has been canceled.
                DEVICEREMOVECOMPLETE = 0x8004, //A device or piece of media has been removed.
                DEVICEREMOVEPENDING = 0x8003, //A device or piece of media is about to be removed. Cannot be denied.
                DEVICETYPESPECIFIC = 0x8005, //A device-specific event has occurred.
                DEVNODES_CHANGED = 0x0007, //A device has been added to or removed from the system.
                QUERYCHANGECONFIG = 0x0017, //Permission is requested to change the current configuration (dock or undock).
                USERDEFINED = 0xFFFF, //The meaning of this message is user-defined.
            }

            public enum WM
            {
                POWERBROADCAST = 0x0218, //PBT enums
                WTSSESSION_CHANGE = 0x02B1, //WTS enums
                DEVICECHANGE = 0x0219, //DBT enums
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct SERVICE_STATUS
            {
                public int serviceType;
                public int currentState;
                public int controlsAccepted;
                public int win32ExitCode;
                public int serviceSpecificExitCode;
                public int checkPoint;
                public int waitHint;
            }
            public enum ServiceControl
            {
                SERVICE_CONTROL_DEVICEEVENT = 0x0000000B,
                SERVICE_CONTROL_HARDWAREPROFILECHANGE = 0x0000000C,
                SERVICE_CONTROL_POWEREVENT = 0x0000000D,
                SERVICE_CONTROL_SESSIONCHANGE = 0x0000000E,
                SERVICE_CONTROL_TIMECHANGE = 0x00000010,
                SERVICE_CONTROL_TRIGGEREVENT = 0x00000020,
                SERVICE_CONTROL_USERMODEREBOOT = 0x00000040,
                SERVICE_CONTROL_CONTINUE = 0x00000003,
                SERVICE_CONTROL_INTERROGATE = 0x00000004,
                SERVICE_CONTROL_NETBINDADD = 0x00000007,
                SERVICE_CONTROL_NETBINDDISABLE = 0x0000000A,
                SERVICE_CONTROL_NETBINDENABLE = 0x00000009,
                SERVICE_CONTROL_NETBINDREMOVE = 0x00000008,
                SERVICE_CONTROL_PARAMCHANGE = 0x00000006,
                SERVICE_CONTROL_PAUSE = 0x00000002,
                SERVICE_CONTROL_PRESHUTDOWN = 0x0000000F,
                SERVICE_CONTROL_SHUTDOWN = 0x00000005,
                SERVICE_CONTROL_STOP = 0x00000001,
            }
            public enum ServiceAccept
            {
                SERVICE_ACCEPT_NETBINDCHANGE = 0x00000010,
                SERVICE_ACCEPT_PARAMCHANGE = 0x00000008,
                SERVICE_ACCEPT_PAUSE_CONTINUE = 0x00000002,
                SERVICE_ACCEPT_PRESHUTDOWN = 0x00000100,
                SERVICE_ACCEPT_SHUTDOWN = 0x00000004,
                SERVICE_ACCEPT_STOP = 0x00000001,
                SERVICE_ACCEPT_HARDWAREPROFILECHANGE = 0x00000020,
                SERVICE_ACCEPT_POWEREVENT = 0x00000040,
                SERVICE_ACCEPT_SESSIONCHANGE = 0x00000080,
                SERVICE_ACCEPT_TIMECHANGE = 0x00000200,
                SERVICE_ACCEPT_TRIGGEREVENT = 0x00000400,
                SERVICE_ACCEPT_USERMODEREBOOT = 0x00000800, //windows 8
            }
        }

        public static bool SetPreshutdownTimeout(SafeHandle handle, ref structenums.SERVICE_STATUS myServiceStatus)
        {
            SafeNativeMethods.SERVICE_PRESHUTDOWN_INFO spi = new SafeNativeMethods.SERVICE_PRESHUTDOWN_INFO();
            spi.dwPreshutdownTimeout = 3600 * 1000;
            IntPtr lpInfo = Marshal.AllocHGlobal(Marshal.SizeOf(spi));
            if (lpInfo == IntPtr.Zero)
            {
                string errorMessage = LastError();
                LibraryLogging.Error("Unable to allocate memory for service action, error: {0}", errorMessage);
                EventLog.WriteEntry("pGina", String.Format("Unable to allocate memory for service action\nerror:{0}", errorMessage), EventLogEntryType.Warning);

                return false;
            }
            else
            {
                Marshal.StructureToPtr(spi, lpInfo, false);
                if (!SafeNativeMethods.ChangeServiceConfig2(handle, (int)SafeNativeMethods.INFO_LEVEL.SERVICE_CONFIG_PRESHUTDOWN_INFO, lpInfo))
                {
                    string errorMessage = LastError();
                    LibraryLogging.Error("ChangeServiceConfig2 error: {0}", errorMessage);
                    EventLog.WriteEntry("pGina", String.Format("ChangeServiceConfig2\nThe service will be forced to stop during a shutdown within 3 minutes\nerror:{0}", errorMessage), EventLogEntryType.Warning);

                    return false;
                }
                Marshal.FreeHGlobal(lpInfo);
            }

            return true;
        }

        public static bool ShutdownPending(IntPtr handle, ref structenums.SERVICE_STATUS myServiceStatus, TimeSpan wait)
        {
            myServiceStatus.checkPoint++;
            myServiceStatus.currentState = (int)SafeNativeMethods.State.SERVICE_STOP_PENDING;
            myServiceStatus.waitHint = wait.Milliseconds;
            if (!SafeNativeMethods.SetServiceStatus(handle, ref myServiceStatus))
            {
                LibraryLogging.Error("SetServiceStatus error:{0}", LastError());
                return false;
            }

            return true;
        }

        public static bool SetServiceStopped(IntPtr handle, ref structenums.SERVICE_STATUS myServiceStatus)
        {
            myServiceStatus.checkPoint++;
            myServiceStatus.currentState = (int)SafeNativeMethods.State.SERVICE_STOPPED;
            myServiceStatus.waitHint = 0;
            if (!SafeNativeMethods.SetServiceStatus(handle, ref myServiceStatus))
            {
                LibraryLogging.Error("SetServiceStatus error:{0}", LastError());
                return false;
            }

            return true;
        }

        public static int GetSessionId()
        {
            int ret=0;
            try
            {
                ret = SafeNativeMethods.WTSGetActiveConsoleSessionId();
            }
            catch
            {
                LibraryLogging.Error("GetSessionId() WTSGetActiveConsoleSessionId Error:{0}", LastError());
            }
            return ret;
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
                Int64 currentSession = (Int64) sessionInfoList;

                for(int x = 0; x < sessionCount; x++)
                {
                    SafeNativeMethods.WTS_SESSION_INFO sessionInfo = (SafeNativeMethods.WTS_SESSION_INFO)Marshal.PtrToStructure((IntPtr)currentSession, typeof(SafeNativeMethods.WTS_SESSION_INFO));
                    currentSession += dataSize;

                    uint bytes = 0;
                    IntPtr userInfo = IntPtr.Zero;
                    IntPtr domainInfo = IntPtr.Zero;
                    bool sResult = SafeNativeMethods.WTSQuerySessionInformation(SafeNativeMethods.WTS_CURRENT_SERVER_HANDLE, sessionInfo.SessionID, SafeNativeMethods.WTS_INFO_CLASS.WTSUserName, out userInfo, out bytes);
                    if (!sResult)
                    {
                        LibraryLogging.Error("GetInteractiveUserList() WTSQuerySessionInformation WTSUserName Error:{0}", LastError());
                    }
                    string user = Marshal.PtrToStringAnsi(userInfo);
                    SafeNativeMethods.WTSFreeMemory(userInfo);
                    /*
                    sResult = SafeNativeMethods.WTSQuerySessionInformation(SafeNativeMethods.WTS_CURRENT_SERVER_HANDLE, sessionInfo.SessionID, SafeNativeMethods.WTS_INFO_CLASS.WTSDomainName, out domainInfo, out bytes);
                    if (!sResult)
                    {
                        LibraryLogging.Error("GetInteractiveUserList() WTSQuerySessionInformation WTSDomainName Error:{0}", LastError());
                    }
                    string domain = Marshal.PtrToStringAnsi(domainInfo);
                    SafeNativeMethods.WTSFreeMemory(domainInfo);
                    */
                    /*
                    if (!string.IsNullOrEmpty(domain))
                    {
                        result.Add(string.Format("{0}\\{1}", domain, user));
                    }*/
                    if (!string.IsNullOrEmpty(user))
                    {
                        result.Add(string.Format("{0}\\{1}", sessionInfo.SessionID, user));
                    }
                }

                SafeNativeMethods.WTSFreeMemory(sessionInfoList);
            }
            //LibraryLogging.Info("InteractiveUsers:{0}", String.Join<string>(", ", result));

            return result;
        }

        public static bool IsSessionLoggedOFF(int session)
        {
            SafeNativeMethods.WTS_SESSION_INFO sessionInfo = new SafeNativeMethods.WTS_SESSION_INFO();
            // if WTSQuerySessionInformation returns false than the session is already closed
            // WTSQuerySessionInformation returns zero if the session is logged off.
            sessionInfo.State = SafeNativeMethods.WTS_CONNECTSTATE_CLASS.WTSReset;
            uint bytes = 0;
            IntPtr userInfo = IntPtr.Zero;

            bool sResult = SafeNativeMethods.WTSQuerySessionInformation(SafeNativeMethods.WTS_CURRENT_SERVER_HANDLE, session, SafeNativeMethods.WTS_INFO_CLASS.WTSConnectState, out userInfo, out bytes);
            if (sResult)
            {
                int lData = Marshal.ReadInt32(userInfo);
                sessionInfo.State = (SafeNativeMethods.WTS_CONNECTSTATE_CLASS)Enum.ToObject(typeof(SafeNativeMethods.WTS_CONNECTSTATE_CLASS), lData);
                /*
                switch (sessionInfo.State)
                {
                    case SafeNativeMethods.WTS_CONNECTSTATE_CLASS.WTSActive:
                        ret = "WTSActive";
                        break;
                    case SafeNativeMethods.WTS_CONNECTSTATE_CLASS.WTSConnected:
                        ret = "WTSConnected";
                        break;
                    case SafeNativeMethods.WTS_CONNECTSTATE_CLASS.WTSConnectQuery:
                        ret = "WTSConnectQuery";
                        break;
                    case SafeNativeMethods.WTS_CONNECTSTATE_CLASS.WTSDisconnected:
                        ret = "WTSDisconnected";
                        break;
                    case SafeNativeMethods.WTS_CONNECTSTATE_CLASS.WTSDown:
                        ret = "WTSDown";
                        break;
                    case SafeNativeMethods.WTS_CONNECTSTATE_CLASS.WTSIdle:
                        ret = "WTSIdle";
                        break;
                    case SafeNativeMethods.WTS_CONNECTSTATE_CLASS.WTSInit:
                        ret = "WTSInit";
                        break;
                    case SafeNativeMethods.WTS_CONNECTSTATE_CLASS.WTSListen:
                        ret = "WTSListen";
                        break;
                    case SafeNativeMethods.WTS_CONNECTSTATE_CLASS.WTSReset:
                        ret = "WTSReset";
                        break;
                    case SafeNativeMethods.WTS_CONNECTSTATE_CLASS.WTSShadow:
                        ret = "WTSShadow";
                        break;
                }
                */
            }
            SafeNativeMethods.WTSFreeMemory(userInfo);
            return (sessionInfo.State == SafeNativeMethods.WTS_CONNECTSTATE_CLASS.WTSReset) ? true : false;
        }

        public static string GetUserDomain(int sessionId)
        {
            uint bytes = 0;
            IntPtr userInfo = IntPtr.Zero;

            bool result = SafeNativeMethods.WTSQuerySessionInformation(SafeNativeMethods.WTS_CURRENT_SERVER_HANDLE, sessionId, SafeNativeMethods.WTS_INFO_CLASS.WTSDomainName, out userInfo, out bytes);
            if (!result)
            {
                LibraryLogging.Error("GetUserDomain({1}) WTSQuerySessionInformation WTSUserName Error:{0}", LastError(), sessionId);
            }
            string userName = Marshal.PtrToStringAnsi(userInfo);
            SafeNativeMethods.WTSFreeMemory(userInfo);

            return userName;
        }

        public static string GetUserName(int sessionId)
        {
            uint bytes = 0;
            IntPtr userInfo = IntPtr.Zero;

            bool result = SafeNativeMethods.WTSQuerySessionInformation(SafeNativeMethods.WTS_CURRENT_SERVER_HANDLE, sessionId, SafeNativeMethods.WTS_INFO_CLASS.WTSUserName, out userInfo, out bytes);
            if (!result)
            {
                LibraryLogging.Error("GetUserName({1}) WTSQuerySessionInformation WTSUserName Error:{0}", LastError(), sessionId);
            }
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
                    {
                        LibraryLogging.Error("StartProcessInSession({1}, {2}) OpenProcessToken Error:{0}", LastError(), sessionId, cmdLine);
                    }
                }

                // Duplicate it, so we can change it's session id
                if (!SafeNativeMethods.DuplicateTokenEx(processToken, (uint)SafeNativeMethods.ACCESS_MASK.MAXIMUM_ALLOWED, IntPtr.Zero, SafeNativeMethods.SECURITY_IMPERSONATION_LEVEL.SecurityIdentification, SafeNativeMethods.TOKEN_TYPE.TokenPrimary, out duplicateToken))
                {
                    LibraryLogging.Error("StartProcessInSession({1}, {2}) DuplicateTokenEx Error:{0}", LastError(), sessionId, cmdLine);
                }

                // Poke the session id we want into our new token
                if (!SafeNativeMethods.SetTokenInformation(duplicateToken, SafeNativeMethods.TOKEN_INFORMATION_CLASS.TokenSessionId, ref sessionId, Marshal.SizeOf(sessionId)))
                {
                    LibraryLogging.Error("StartProcessInSession({1}, {2}) SetTokenInformation Error:{0}", LastError(), sessionId, cmdLine);
                }

                return StartProcessWithToken(duplicateToken, cmdLine);
            }
            finally
            {
                SafeNativeMethods.CloseHandle(processToken);
                SafeNativeMethods.CloseHandle(duplicateToken);
            }
        }

        public static bool StartProcessInSessionWait(int sessionId, string cmdLine)
        {
            try
            {
                using (Process p = StartProcessInSession(sessionId, cmdLine))
                {
                    p.WaitForExit(); // trow exception if error
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static bool StartUserProcessInSession(int sessionId, string cmdLine)
        {
            IntPtr userToken = IntPtr.Zero;

            try
            {
                // Get user's token from session id, WTSQueryUserToken already returns a primary token for us
                //  so all we then have to do is use it to start the process.
                if (!SafeNativeMethods.WTSQueryUserToken(sessionId, out userToken))
                {
                    LibraryLogging.Error("StartUserProcessInSession({1}, {2}) WTSQueryUserToken Error:{0}", LastError(), sessionId, cmdLine);
                }

                using (Process p = StartProcessWithToken(userToken, cmdLine))
                {
                    bool tmp = p.HasExited; // trow exception if error
                }
            }
            catch
            {
                return false;
            }
            finally
            {
                SafeNativeMethods.CloseHandle(userToken);
            }

            return true;
        }

        public static bool StartUserProcessInSessionWait(int sessionId, string cmdLine)
        {
            IntPtr userToken = IntPtr.Zero;

            try
            {
                // Get user's token from session id, WTSQueryUserToken already returns a primary token for us
                //  so all we then have to do is use it to start the process.
                if (!SafeNativeMethods.WTSQueryUserToken(sessionId, out userToken))
                {
                    LibraryLogging.Error("StartUserProcessInSession({1}, {2}) WTSQueryUserToken Error:{0}", LastError(), sessionId, cmdLine);
                }

                using (Process p = StartProcessWithToken(userToken, cmdLine))
                {
                    p.WaitForExit(); // trow exception if error
                }
            }
            catch
            {
                return false;
            }
            finally
            {
                SafeNativeMethods.CloseHandle(userToken);
            }

            return true;
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
                {
                    LibraryLogging.Error("StartProcessWithToken(IntPtr, {1}) CreateEnvironmentBlock Error:{0}", LastError(), cmdLine);
                }

                // Now we can finally get into the business at hand and setup our process info
                SafeNativeMethods.STARTUPINFO startInfo = new SafeNativeMethods.STARTUPINFO();
                startInfo.cb = Marshal.SizeOf(startInfo);
                //startInfo.wShowWindow = 0;
                //startInfo.lpDesktop = "Winsta0\\Default";   // TBD: Support other desktops?

                SafeNativeMethods.PROCESS_INFORMATION procInfo = new SafeNativeMethods.PROCESS_INFORMATION();
                if (!SafeNativeMethods.CreateProcessAsUser(token, null, cmdLine, ref defSec, ref defSec, false, SafeNativeMethods.CreationFlags.CREATE_UNICODE_ENVIRONMENT, environmentBlock, null, ref startInfo, out procInfo))
                {
                    LibraryLogging.Error("StartProcessWithToken(IntPtr, {1}) CreateProcessAsUser Error:{0}", LastError(), cmdLine);
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

        public static bool StartProcessAsUserWait(string username, string domain, string password, string command)
        {
            IntPtr hToken = IntPtr.Zero;

            if (SafeNativeMethods.LogonUser(username, domain, password, (int)SafeNativeMethods.LogonType.LOGON32_LOGON_INTERACTIVE, (int)SafeNativeMethods.LogonProvider.LOGON32_PROVIDER_DEFAULT, out hToken))
            {
                SafeNativeMethods.PROFILEINFO pinfo = new SafeNativeMethods.PROFILEINFO();
                pinfo.dwSize = Marshal.SizeOf(pinfo);
                pinfo.lpUserName = username;

                if (SafeNativeMethods.LoadUserProfile(hToken, ref pinfo))
                {
                    using (Process proc = StartProcessWithToken(hToken, command))
                    {
                        try
                        {
                            proc.WaitForExit();
                        }
                        catch
                        {
                            if (!SafeNativeMethods.UnloadUserProfile(hToken, pinfo.hProfile))
                            {
                                LibraryLogging.Error("StartProcessAsUserWait({0}, {1}, ****, {2}) UnloadUserProfile Error:{3}", username, domain, command, LastError());
                            }
                            CloseHandle(hToken);
                            return false;
                        }
                    }
                    if (!SafeNativeMethods.UnloadUserProfile(hToken, pinfo.hProfile))
                    {
                        LibraryLogging.Error("StartProcessAsUserWait({0}, {1}, ****, {2}) UnloadUserProfile Error:{3}", username, domain, command, LastError());
                    }
                }
                else
                {
                    LibraryLogging.Error("StartProcessAsUserWait({0}, {1}, ****, {2}) LoadUserProfile Error:{3}", username, domain, command, LastError());
                    CloseHandle(hToken);
                    return false;
                }

                CloseHandle(hToken);
            }
            else
            {
                LibraryLogging.Error("StartProcessAsUserWait({0}, {1}, ****, {2}) LogonUser Error:{3}", username, domain, command, LastError());
                return false;
            }

            return true;
        }

        /// <summary>
        /// Run app as system
        /// </summary>
        /// <param name="Application"></param>
        /// <param name="CommandLine"></param>
        /// <returns>return code of the app or uint.MaxValue if exception</returns>
        public static uint CProcess(string Application, string CommandLine)
        {
            SafeNativeMethods.PROCESS_INFORMATION pInfo = new SafeNativeMethods.PROCESS_INFORMATION();
            SafeNativeMethods.STARTUPINFO sInfo = new SafeNativeMethods.STARTUPINFO();
            SafeNativeMethods.SECURITY_ATTRIBUTES pSec = new SafeNativeMethods.SECURITY_ATTRIBUTES();
            SafeNativeMethods.SECURITY_ATTRIBUTES tSec = new SafeNativeMethods.SECURITY_ATTRIBUTES();
            pSec.nLength = Marshal.SizeOf(pSec);
            tSec.nLength = Marshal.SizeOf(tSec);

            if (SafeNativeMethods.CreateProcess(Application, CommandLine, ref pSec, ref tSec, false, 0x0020, IntPtr.Zero, null, ref sInfo, out pInfo))
            {
                if (SafeNativeMethods.WaitForSingleObject(pInfo.hProcess, -1) == 0)
                {
                    uint ExitCode;
                    if (!SafeNativeMethods.GetExitCodeProcess(pInfo.hProcess, out ExitCode))
                    {
                        LibraryLogging.Error(String.Format("CProcess() GetExitCodeProcess Error:{0}", LastError()));
                        return uint.MaxValue;
                    }
                    return ExitCode;
                }
                else
                {
                    LibraryLogging.Error(String.Format("CProcess() WaitForSingleObject Error:{0}", LastError()));
                }
            }
            else
            {
                LibraryLogging.Error(String.Format("CreateProcess Error:{0}", LastError()));
            }

            return uint.MaxValue;
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
            return ValidateCredentials(username, "", password);
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
            int ret = LogonUser(username, domain, password);
            if (ret == 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Attempts to validate the user's credentials using
        /// a pInvoke to LogonUser but ignores password change response.
        /// </summary>
        /// <param name="username">The username</param>
        /// <param name="domain">The domain</param>
        /// <param name="password">The password</param>
        /// <returns>True if the account credentials are valid</returns>
        public static bool ValidateUser(string username, string domain, string password)
        {
            int ret = LogonUser(username, domain, password);
            // ERROR_PASSWORD_EXPIRED 1330
            // ERROR_PASSWORD_MUST_CHANGE 1907
            // ERROR_PASSWORD_CHANGE_REQUIRED 1938
            if (new int[] { 0, 1330, 1907, 1938 }.Any(a => a == ret))
            {
                return true;
            }

            return false;
        }

        internal static int LogonUser(string username, string domain, string password)
        {
            int error = 0;
            IntPtr hToken = IntPtr.Zero;
            bool result = SafeNativeMethods.LogonUser(username, domain, password,
                (int)SafeNativeMethods.LogonType.LOGON32_LOGON_NETWORK,
                (int)SafeNativeMethods.LogonProvider.LOGON32_PROVIDER_DEFAULT,
                out hToken);
            if (!result)
            {
                error = Marshal.GetLastWin32Error();
                Abstractions.Logging.LibraryLogging.Debug("LogonUser:{0} {1} {2}", result, error, LastError());
            }

            if (hToken != IntPtr.Zero) CloseHandle(hToken);

            return error;
        }

        public static bool WTSRegister(IntPtr handle)
        {
            if (!SafeNativeMethods.WTSRegisterSessionNotification(handle, 1/*NOTIFY_FOR_ALL_SESSIONS*/))
            {
                LibraryLogging.Error("WTSRegisterSessionNotification error:{0}", LastError());
                return false;
            }

            return true;
        }

        public static bool WTSUnRegister(IntPtr handle)
        {
            if (!SafeNativeMethods.WTSUnRegisterSessionNotification(handle))
            {
                LibraryLogging.Error("WTSUnRegisterSessionNotification error:{0}", LastError());
                return false;
            }

            return true;
        }

        /// <summary>
        /// return username based on context in which a program is running.
        /// a program running as administrator will add administrator to the list
        /// session ID -1 retuns all sessions instead of a specific one
        /// </summary>
        /// <param name="sessionID">the seesion ID or -1 for all sessions</param>
        /// <returns>is a lower username like administrator</returns>
        public static List<string> GetSessionContext(int sessionID)
        {
            List<string> ret = new List<string>();

            Dictionary<int, List<string>> contextALL = GetSessionContext();
            foreach (KeyValuePair<int, List<string>> pair in contextALL)
            {
                if (pair.Key == sessionID || sessionID == -1)
                {
                    foreach (string user in pair.Value)
                    {
                        if (!ret.Any(s => s.Equals(user, StringComparison.CurrentCultureIgnoreCase)))
                        {
                            ret.Add(user);
                        }
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// return username based on context in which a program is running.
        /// a program running as administrator will add administrator to the list
        /// session ID -1 retuns all sessions instead of a specific one
        /// </summary>
        /// <param name="sessionID">the seesion ID or -1 for all sessions</param>
        /// <param name="contextALL">a GetSessionContext() Directory</param>
        /// <returns>is a lower username like administrator</returns>
        public static List<string> GetSessionContextParser(int sessionID, Dictionary<int, List<string>> contextALL)
        {
            List<string> ret = new List<string>();

            foreach (KeyValuePair<int, List<string>> pair in contextALL)
            {
                if (pair.Key == sessionID || sessionID == -1)
                {
                    foreach (string user in pair.Value)
                    {
                        if (!ret.Any(s => s.Equals(user, StringComparison.CurrentCultureIgnoreCase)))
                        {
                            ret.Add(user);
                        }
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// returns a Dictionary of SessionID Keys and a List of usernames based on context in which a program is running.
        /// a program running as administrator will add administrator to the list
        /// </summary>
        /// <returns>is a lower username like administrator</returns>
        public static Dictionary<int, List<string>> GetSessionContext()
        {
            Dictionary<int, List<string>> ret = new Dictionary<int, List<string>>();

            foreach (Process process in Process.GetProcesses())
            {
                try { var test = process.Handle; }
                catch { continue; }

                //Console.WriteLine("process:{0}", process.ProcessName);
                IntPtr tokenHandle;
                // Get the Process Token
                if (!SafeNativeMethods.OpenProcessToken(process.Handle, SafeNativeMethods.TokenAccessRights.TOKEN_READ, out tokenHandle))
                {
                    LibraryLogging.Error("OpenProcessToken error:{0}", LastError());
                    return ret;
                }

                int TokenInfLength = 0;
                if (!SafeNativeMethods.GetTokenInformation(tokenHandle, SafeNativeMethods.TOKEN_INFORMATION_CLASS.TokenUser, IntPtr.Zero, TokenInfLength, out TokenInfLength))
                {
                    int error = Marshal.GetLastWin32Error();
                    if (error != 122 /*ERROR_INSUFFICIENT_BUFFER*/)
                    {
                        LibraryLogging.Error("GetTokenInformation error:{0} {1}", error, LastError());
                    }
                    else
                    {
                        IntPtr TokenInformation = Marshal.AllocHGlobal(TokenInfLength);
                        if (!SafeNativeMethods.GetTokenInformation(tokenHandle, SafeNativeMethods.TOKEN_INFORMATION_CLASS.TokenUser, TokenInformation, TokenInfLength, out TokenInfLength))
                        {
                            LibraryLogging.Error("GetTokenInformation error:{0}", LastError());
                        }
                        else
                        {
                            SafeNativeMethods.TOKEN_USER TokenUser = (SafeNativeMethods.TOKEN_USER)Marshal.PtrToStructure(TokenInformation, typeof(SafeNativeMethods.TOKEN_USER));

                            StringBuilder name = new StringBuilder();
                            uint cchName = (uint)name.Capacity;
                            StringBuilder referencedDomainName = new StringBuilder();
                            uint cchReferencedDomainName = (uint)referencedDomainName.Capacity;
                            SafeNativeMethods.SID_NAME_USE sidUse;


                            bool WinAPI = SafeNativeMethods.LookupAccountSid(null, TokenUser.User.Sid, name, ref cchName, referencedDomainName, ref cchReferencedDomainName, out sidUse);
                            if (!WinAPI)
                            {
                                error = Marshal.GetLastWin32Error();
                                if (error == 122 /*ERROR_INSUFFICIENT_BUFFER*/)
                                {
                                    name.EnsureCapacity((int)cchName);
                                    referencedDomainName.EnsureCapacity((int)cchReferencedDomainName);
                                    WinAPI = SafeNativeMethods.LookupAccountSid(null, TokenUser.User.Sid, name, ref cchName, referencedDomainName, ref cchReferencedDomainName, out sidUse);
                                    if (!WinAPI)
                                    {
                                        LibraryLogging.Error("LookupAccountSid error:{0}", LastError());
                                    }
                                }
                                else
                                {
                                    LibraryLogging.Error("LookupAccountSid error:{0}", LastError());
                                }
                            }
                            if (WinAPI)
                            {
                                //LibraryLogging.Info("Found process:{0} in session:{1} account:{2}", process.ProcessName, process.SessionId, name.ToString());
                                if (!ret.ContainsKey(process.SessionId))
                                {
                                    ret.Add(process.SessionId, new List<string>() { name.ToString() });
                                }
                                else
                                {
                                    if (!ret[process.SessionId].Contains(name.ToString()))
                                    {
                                        ret[process.SessionId].Add(name.ToString());
                                    }
                                }
                            }
                        }
                        Marshal.FreeHGlobal(TokenInformation);
                    }
                }
                SafeNativeMethods.CloseHandle(tokenHandle);
            }

            return ret;
        }

        /// <summary>
        /// Create a handle to the user token
        /// make sure to close the handle!!!
        /// </summary>
        /// <param name="username"></param>
        /// <param name="domain"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static IntPtr GetUserToken(string username, string domain, string password)
        {
            IntPtr hToken = IntPtr.Zero;
            bool result = SafeNativeMethods.LogonUser(username, domain, password,
                (int)SafeNativeMethods.LogonType.LOGON32_LOGON_NETWORK,
                (int)SafeNativeMethods.LogonProvider.LOGON32_PROVIDER_DEFAULT,
                out hToken);
            if (!result)
            {
                LibraryLogging.Error("LogonUser error:{0}", LastError());
            }

            return hToken;
        }

        /// <summary>
        /// calls API CreateProfile
        /// an empty string on error
        /// a null string on already exists
        /// a string on success
        /// </summary>
        /// <param name="hToken"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        public static string CreateUserProfileDir(IntPtr hToken, string username)
        {
            StringBuilder path = new StringBuilder(260);
            uint size = Convert.ToUInt32(path.Capacity);
            uint hResult = 0;

            using (System.Security.Principal.WindowsIdentity i = new System.Security.Principal.WindowsIdentity(hToken))
            {
                hResult = SafeNativeMethods.CreateProfile(i.Owner.Value, username, path, size);
            }
            if (hResult == 2147942583)
            {
                LibraryLogging.Error("CreateProfile already exists:{0}", LastError());
                return null;
            }
            else if (hResult == 0)
            {
                return path.ToString();
            }
            else
            {
                LibraryLogging.Error("CreateProfile error:{0} {1} {2}", hResult, LastError(), path.ToString());
            }

            return "";
        }

        /// <summary>
        /// get or create user profile directory
        /// only if the ProfileList regkey is not of SID.bak (Abstractions.User.FixProfileList)
        /// an empty or null string means error
        /// </summary>
        /// <param name="username"></param>
        /// <param name="domain"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string GetOrSetUserProfileDir(string username, string domain, string password)
        {
            string ret = "";
            StringBuilder path = new StringBuilder(260);
            uint path_size = Convert.ToUInt32(path.Capacity);

            IntPtr hToken = GetUserToken(username, domain, password);
            if (hToken == IntPtr.Zero)
            {
                LibraryLogging.Error("GetOrSetUserProfileDir can't get userToken");
                return "";
            }

            ret = GetUserProfileDir(hToken);
            if (String.IsNullOrEmpty(ret))
            {
                ret = CreateUserProfileDir(hToken, username);
                if (String.IsNullOrEmpty(ret))
                {
                    LibraryLogging.Error("GetOrSetUserProfileDir failed to get and create profile error:{0}", LastError());
                }
            }
            SafeNativeMethods.CloseHandle(hToken);

            return ret;
        }

        /// <summary>
        /// returns userprofile based on GetUserProfileDirectory
        /// only if the ProfileList regkey is not of SID.bak (Abstractions.User.FixProfileList)
        /// empty string means error
        /// </summary>
        /// <param name="hToken"></param>
        /// <returns></returns>
        public static string GetUserProfileDir(IntPtr hToken)
        {
            string ret = "";
            StringBuilder path = new StringBuilder(260);
            uint path_size = Convert.ToUInt32(path.Capacity);

            if (SafeNativeMethods.GetUserProfileDirectory(hToken, path, ref path_size))
            {
                ret = path.ToString();
            }
            else
            {
                LibraryLogging.Error("GetUserProfileDirectory error:{0}", LastError());
            }

            return ret;
        }

        /// <summary>
        /// returns userprofile based on SHGetFolderPath
        /// only if the profile realy exists and the ProfileList regkey is not of SID.bak (Abstractions.User.FixProfileList)
        /// empty string means error
        /// </summary>
        /// <param name="hToken"></param>
        /// <returns></returns>
        public static string GetUserProfilePath(IntPtr hToken)
        {
            const int MaxPath = 260;
            StringBuilder sb = new StringBuilder(MaxPath);

            int hResult = SafeNativeMethods.SHGetFolderPath(IntPtr.Zero, 0x0028/*CSIDL_PROFILE*/, hToken, 0x0000, sb);
            if (hResult != 0)
            {
                LibraryLogging.Error("SHGetFolderPath error:{0}", LastError());
                return "";
            }

            return sb.ToString();
        }

        /// <summary>
        /// get the real windows version
        /// http://www.codeproject.com/Articles/678606/Part-Overcoming-Windows-s-deprecation-of-GetVe
        /// </summary>
        /// <returns></returns>
        public static structenums.OSVERSIONINFOW VersionsInfo()
        {
            structenums.OSVERSIONINFOW ret = new structenums.OSVERSIONINFOW();
            ret.dwOSVersionInfoSize = Marshal.SizeOf(ret);

            if (SafeNativeMethods.RtlGetVersion(ref ret) != 0)
            {
                LibraryLogging.Error("VersionsInfo error:{0}", LastError());
            }

            return ret;
        }

        /// <summary>
        /// return true if a local user exists
        /// </summary>
        /// <param name="username"></param>
        /// <param name="domain"></param>
        /// <returns></returns>
        public static Boolean UserExists(string username, string domain)
        {
            structenums.USER_INFO_4 userinfo4 = new structenums.USER_INFO_4();
            if (UserGet(username, domain, ref userinfo4))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// return true if a local user exists
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public static Boolean UserExists(string username)
        {
            return UserExists(username, null);
        }

        /// <summary>
        /// add user to the local system
        /// based on http://social.msdn.microsoft.com/forums/en-us/csharpgeneral/thread/B70B79D9-971F-4D6F-8462-97FC126DE0AD
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        public static Boolean UserADD(string username, string password, string comment)
        {
            SafeNativeMethods.USER_INFO_1 NewUser = new SafeNativeMethods.USER_INFO_1();

            NewUser.usri1_name = username; // Allocates the username
            NewUser.usri1_password = password; // allocates the password
            NewUser.usri1_priv = structenums.UserPrivileges.USER_PRIV_USER; // Sets the account type to USER_PRIV_USER
            NewUser.usri1_home_dir = null; // We didn't supply a Home Directory
            NewUser.comment = comment;// "pGina created pgSMB"; // Comment on the User
            NewUser.usri1_script_path = null; // We didn't supply a Logon Script Path

            Int32 ret;
            SafeNativeMethods.NetUserAdd(Environment.MachineName, 1, ref NewUser, out ret);
            //2224 The user account already exists. NERR_UserExists
            if ((ret != 0) && (ret != 2224)) // If the call fails we get a non-zero value
            {
                LibraryLogging.Error("NetUserAdd error:{0} {1}", ret, LastError());
                return false;
            }

            return true;
        }

        /// <summary>
        /// modify local user settings based on a USER_INFO_4 struct
        /// based on http://social.msdn.microsoft.com/forums/en-us/csharpgeneral/thread/B70B79D9-971F-4D6F-8462-97FC126DE0AD
        /// </summary>
        /// <param name="username"></param>
        /// <param name="userInfo4"></param>
        /// <returns></returns>
        public static Boolean UserMod(string username, structenums.USER_INFO_4 userInfo4)
        {
            Int32 ret = 1;

            SafeNativeMethods.NetUserSetInfo(null, username, 4, ref userInfo4, out ret);
            if (ret != 0) // If the call fails we get a non-zero value
            {
                LibraryLogging.Error("NetUserSetInfo error:{0} {1}", ret, LastError());
                return false;
            }

            return true;
        }

        /// <summary>
        /// returns local user settings as a ref of an USER_INFO_4 struct
        /// based on http://social.msdn.microsoft.com/forums/en-us/csharpgeneral/thread/B70B79D9-971F-4D6F-8462-97FC126DE0AD
        /// </summary>
        /// <param name="username"></param>
        /// <param name="userinfo4">a ref of an USER_INFO_4 struct</param>
        /// <returns>bool true 4 success</returns>
        public static Boolean UserGet(string username, ref structenums.USER_INFO_4 userinfo4)
        {
            return UserGet(username, null, ref userinfo4);
        }

        /// <summary>
        /// returns local user settings as a ref of an USER_INFO_4 struct
        /// based on http://social.msdn.microsoft.com/forums/en-us/csharpgeneral/thread/B70B79D9-971F-4D6F-8462-97FC126DE0AD
        /// </summary>
        /// <param name="username"></param>
        /// <param name="domain"></param>
        /// <param name="userinfo4">a ref of an USER_INFO_4 struct</param>
        /// <returns>bool true 4 success</returns>
        public static Boolean UserGet(string username, string domain, ref structenums.USER_INFO_4 userinfo4)
        {
            IntPtr bufPtr;

            int lngReturn = SafeNativeMethods.NetUserGetInfo((String.IsNullOrEmpty(domain)) ? null : domain, username, 4, out bufPtr);
            if (lngReturn == 0)
            {
                try
                {
                    userinfo4 = (structenums.USER_INFO_4)Marshal.PtrToStructure(bufPtr, typeof(structenums.USER_INFO_4));
                }
                catch (Exception ex)
                {
                    LibraryLogging.Error("UserGet Marshal.PtrToStructure error:{0}", ex.ToString());
                    return false;
                }
            }
            else if (lngReturn == 2221)
            {
                // user does not exist
            }
            else
            {
                string errorMessage = new Win32Exception(Marshal.GetLastWin32Error()).ToString();
                LibraryLogging.Error("NetUserGetInfo error:{0} {1}", lngReturn, errorMessage);
            }

            SafeNativeMethods.NetApiBufferFree(bufPtr);

            if (lngReturn == 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// delete a local user
        /// based on http://social.msdn.microsoft.com/forums/en-us/csharpgeneral/thread/B70B79D9-971F-4D6F-8462-97FC126DE0AD
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public static Boolean UserDel(string username)
        {
            Int32 ret;

            ret = SafeNativeMethods.NetUserDel(null, username);
            if ((ret != 0) && (ret != 2221)) // If the call fails we get a non-zero value
            {
                LibraryLogging.Error("NetUserDel error:{0} {1}", ret, LastError());
                return false;
            }

            return true;
        }

        public static string UserChangePassword(string domainname, string username, string oldpassword, string newpassword)
        {
            int result = pInvokes.SafeNativeMethods.NetUserChangePassword((String.IsNullOrEmpty(domainname))? Environment.MachineName : domainname, username, oldpassword, newpassword);
            if (result != 0)
            {
                LibraryLogging.Error("NetUserChangePassword({0}, {1}, {2}, {3}) Error:{4} {5}", (String.IsNullOrEmpty(domainname)) ? Environment.MachineName : domainname, username, "***", "***", result, LastError(result));
                return String.Format("Password change failed for user {0} with error {1}\n{2}", username, result, LastError(result));
            }

            return "";
        }

        /// <summary>
        /// send a messagebox to a session
        /// </summary>
        /// <param name="sessionID"></param>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static Boolean SendMessageToUser(int sessionID, string title, string message)
        {
            int resp = 0;
            bool result = SafeNativeMethods.WTSSendMessage(IntPtr.Zero, sessionID, title, title.Length, message, message.Length, 0, 0, out resp, false);
            if (!result)
            {
                LibraryLogging.Error("WTSSendMessage error:{0} {1}", result, LastError());
                return false;
            }

            return true;
        }

        /// <summary>
        /// connect to a remote computer
        /// based on http://social.msdn.microsoft.com/Forums/et-EE/netfxbcl/thread/58159e0e-aa45-4d46-a128-596c3d23ff5c
        /// </summary>
        /// <param name="remoteUNC"></param>
        /// <param name="username">better use the domainname\username format</param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static Boolean MapNetworkDrive(string remoteUNC, string username, string password)
        {
            return MapNetworkDrive(remoteUNC, username, password, false);
        }

        /// <summary>
        /// connect to a remote computer
        /// based on http://social.msdn.microsoft.com/Forums/et-EE/netfxbcl/thread/58159e0e-aa45-4d46-a128-596c3d23ff5c
        /// </summary>
        /// <param name="remoteUNC"></param>
        /// <param name="username">better use the domainname\username format</param>
        /// <param name="password"></param>
        /// <param name="promptUser">true to for a promt, if needed</param>
        /// <returns></returns>
        public static Boolean MapNetworkDrive(string remoteUNC, string username, string password, bool promptUser)
        {
            SafeNativeMethods.NETRESOURCE nr = new SafeNativeMethods.NETRESOURCE();
            nr.dwType = SafeNativeMethods.ResourceType.RESOURCETYPE_DISK;
            nr.lpRemoteName = remoteUNC;
            // nr.lpLocalName = "F:";

            int ret;
            if (promptUser)
            {
                ret = SafeNativeMethods.WNetUseConnection(IntPtr.Zero, nr, "", "", SafeNativeMethods.Connect.CONNECT_INTERACTIVE | SafeNativeMethods.Connect.CONNECT_PROMPT, null, null, null);
            }
            else
            {
                ret = SafeNativeMethods.WNetUseConnection(IntPtr.Zero, nr, password, username, 0, null, null, null);
            }

            if (ret != 0)
            {
                LibraryLogging.Error("Unable to connect to {0} as {1} Error:{2} {3}", remoteUNC, username, ret, LastError());
                return false;
            }

            return true;
        }

        /// <summary>
        /// disconnecting an UNC share
        /// </summary>
        /// <param name="remoteUNC"></param>
        /// <returns></returns>
        public static Boolean DisconnectNetworkDrive(string remoteUNC)
        {
            int ret = SafeNativeMethods.WNetCancelConnection2(remoteUNC, SafeNativeMethods.Connect.CONNECT_UPDATE_PROFILE, false);
            if ((ret != 0) && (ret != 2250/*This network connection does not exist*/))
            {
                LibraryLogging.Error("Unable to disConnect from {0} Error:{1} {2}", remoteUNC, ret, LastError());
                return false;
            }

            return true;
        }

        /// <summary>
        /// Retrieves information about the amount of space that is available on a disk volume, which is the total amount of space, the total amount of free space, and the total amount of free space available to the user that is associated with the calling thread.
        /// based on http://msdn.microsoft.com/en-us/library/windows/desktop/aa364937(v=vs.85).aspx
        /// </summary>
        /// <param name="share"></param>
        /// <returns>An array[freeBytesForUser,totalBytes,freeBytes]. On error all values are -1</returns>
        public static long[] GetFreeShareSpace(string share)
        {
            long freeBytesForUser = -1, totalBytes = -1, freeBytes = -1;

            if (!SafeNativeMethods.GetDiskFreeSpaceEx(share, out freeBytesForUser, out totalBytes, out freeBytes))
            {
                LibraryLogging.Error("Unable to enumerate free space on {0} Error:{1}", share, LastError());
                return new long[] { -1, -1, -1 };
            }

            return new long[] { freeBytesForUser, totalBytes, freeBytes };
        }

        /// <summary>
        /// get RegistryKey from structenums.RegistryLocation
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public static RegistryKey GetRegistryLocation(structenums.RegistryLocation location)
        {
            switch (location)
            {
                case structenums.RegistryLocation.HKEY_CLASSES_ROOT:
                    return Registry.ClassesRoot;

                case structenums.RegistryLocation.HKEY_CURRENT_USER:
                    return Registry.CurrentUser;

                case structenums.RegistryLocation.HKEY_LOCAL_MACHINE:
                    return Registry.LocalMachine;

                case structenums.RegistryLocation.HKEY_USERS:
                    return Registry.Users;

                case structenums.RegistryLocation.HKEY_CURRENT_CONFIG:
                    return Registry.CurrentConfig;

                default:
                    return null;
            }
        }

        // based on http://stackoverflow.com/questions/7894909/load-registry-hive-from-c-sharp-fails
        internal static Boolean RegistryLoadSetPrivilege()
        {
            IntPtr _myToken;
            SafeNativeMethods.TokPriv1Luid _tokenPrivileges = new SafeNativeMethods.TokPriv1Luid();
            SafeNativeMethods.TokPriv1Luid _tokenPrivileges2 = new SafeNativeMethods.TokPriv1Luid();
            SafeNativeMethods.LUID _restoreLuid;
            SafeNativeMethods.LUID _backupLuid;

            if (!SafeNativeMethods.OpenProcessToken(SafeNativeMethods.GetCurrentProcess(), SafeNativeMethods.TokenAccessRights.TOKEN_ADJUST_PRIVILEGES | SafeNativeMethods.TokenAccessRights.TOKEN_QUERY, out _myToken))
            {
                LibraryLogging.Error("SetPrivilege() OpenProcess Error: {0}", LastError());
                return false;
            }

            if (!SafeNativeMethods.LookupPrivilegeValue(null, SafeNativeMethods.TokenAccessRights.SE_RESTORE_NAME, out _restoreLuid))
            {
                LibraryLogging.Error("SetPrivilege() LookupPrivilegeValue Error: {0}", LastError());
                return false;
            }

            if (!SafeNativeMethods.LookupPrivilegeValue(null, SafeNativeMethods.TokenAccessRights.SE_BACKUP_NAME, out _backupLuid))
            {
                LibraryLogging.Error("SetPrivilege() LookupPrivilegeValue Error: {0}", LastError());
                return false;
            }

            _tokenPrivileges.Attr = SafeNativeMethods.TokenAccessRights.SE_PRIVILEGE_ENABLED;
            _tokenPrivileges.Luid = _restoreLuid;
            _tokenPrivileges.Count = 1;

            _tokenPrivileges2.Attr = SafeNativeMethods.TokenAccessRights.SE_PRIVILEGE_ENABLED;
            _tokenPrivileges2.Luid = _backupLuid;
            _tokenPrivileges2.Count = 1;

            if (!SafeNativeMethods.AdjustTokenPrivileges(_myToken, false, ref _tokenPrivileges, 0, IntPtr.Zero, IntPtr.Zero))
            {
                LibraryLogging.Error("SetPrivilege() AdjustTokenPrivileges Error: {0}", LastError());
                return false;
            }

            if (!SafeNativeMethods.AdjustTokenPrivileges(_myToken, false, ref _tokenPrivileges2, 0, IntPtr.Zero, IntPtr.Zero))
            {
                LibraryLogging.Error("SetPrivilege() AdjustTokenPrivileges Error: {0}", LastError());
                return false;
            }

            if (!CloseHandle(_myToken))
            {
                LibraryLogging.Warn("Can't close handle _myToken");
            }

            SafeNativeMethods.RegistryLoadPrivilegeSet = true;
            return true;
        }

        /// <summary>
        /// load a regfile at hklm or hku
        /// </summary>
        /// <param name="where">hklm or hku</param>
        /// <param name="name">The name of the key to be created under hKey. This subkey is where the registration information from the file will be loaded</param>
        /// <param name="file"></param>
        /// <returns></returns>
        public static Boolean RegistryLoad(structenums.RegistryLocation where, string name, string file)
        {
            if (where != structenums.RegistryLocation.HKEY_LOCAL_MACHINE && where != structenums.RegistryLocation.HKEY_USERS)
            {
                LibraryLogging.Error("unable to load regfile at {0}", where);
                return false;
            }

            if (!SafeNativeMethods.RegistryLoadPrivilegeSet)
            {
                if (!RegistryLoadSetPrivilege())
                    return false;
            }

            int ret = SafeNativeMethods.RegLoadKey((uint)Enum.Parse(typeof(structenums.baseKey), where.ToString()), name, file);
            if (ret != 0)
            {
                LibraryLogging.Error("Unable to load regfile {0} error:{1} {2}", file, ret, LastError(ret));
                return false;
            }
            return true;
        }

        /// <summary>
        /// unload regfile from regkey
        /// </summary>
        /// <param name="where">hklm or hku</param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Boolean RegistryUnLoad(structenums.RegistryLocation where, string name)
        {
            if (!SafeNativeMethods.RegistryLoadPrivilegeSet)
            {
                if (!RegistryLoadSetPrivilege())
                    return false;
            }

            int ret = SafeNativeMethods.RegUnLoadKey((uint)Enum.Parse(typeof(structenums.baseKey), where.ToString()), name);
            if (ret != 0)
            {
                LibraryLogging.Error("Unable to unload regkey {0} error:{1} {2}", name, ret, LastError(ret));
                return false;
            }
            return true;
        }

        public static UIntPtr RegistryCreateKey(structenums.baseKey RootKey, string SubKey)
        {
            UIntPtr hKey = UIntPtr.Zero;
            int Result = SafeNativeMethods.RegCreateKey(RootKey, SubKey, ref hKey);
            if (Result != 0)
            {
                Console.WriteLine("RegCreateKey error:{0}", LastError(Result));
            }

            return hKey;
        }

        public static UIntPtr RegistryOpenKey(structenums.baseKey RootKey, string SubKey)
        {
            UIntPtr hKey = UIntPtr.Zero;
            int Result = SafeNativeMethods.RegOpenKey(RootKey, SubKey, ref hKey);
            if (Result != 0)
            {
                Console.WriteLine("RegOpenKey error:{0}", LastError(Result));
            }

            return hKey;
        }

        public static Boolean RegistryCloseKey(UIntPtr hKey)
        {
            int Result = SafeNativeMethods.RegCloseKey(hKey);
            if (Result != 0)
            {
                Console.WriteLine("RegCloseKey error:{0}", LastError(Result));
                return false;
            }

            return true;
        }

        public static Boolean RegistryCopyKey(UIntPtr hKey_src, string SubKey, UIntPtr hKey_dst)
        {
            int Result = SafeNativeMethods.RegCopyTree(hKey_src, SubKey, hKey_dst);
            if (Result != 0)
            {
                Console.WriteLine("CopyKey error:{0}", LastError(Result));
                return false;
            }

            return true;
        }

        public static Boolean RegistryDeleteTree(structenums.baseKey RootKey, string SubKey)
        {
            int Result = SafeNativeMethods.RegDeleteTree(RootKey, SubKey);
            if (Result != 0)
            {
                Console.WriteLine("CopyKey error:{0}", LastError(Result));
                return false;
            }

            return true;
        }
        /// <summary>
        /// query if the domain is part of a domain tree in which the local computer is a member of
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        public static bool DomainMember(string domain)
        {
            if (String.IsNullOrEmpty(domain))
                return false;

            return !String.IsNullOrEmpty(GetDomainInfo(domain).DomainName);
        }

        internal static pInvokes.SafeNativeMethods.DOMAIN_CONTROLLER_INFO GetDomainInfo(string domain)
        {
            pInvokes.SafeNativeMethods.DOMAIN_CONTROLLER_INFO domainInfo = new pInvokes.SafeNativeMethods.DOMAIN_CONTROLLER_INFO();
            IntPtr pDCI = IntPtr.Zero;

            int ret = pInvokes.SafeNativeMethods.DsGetDcName(null, domain, 0, "", 0, out pDCI);
            if (ret == 0)
            {
                domainInfo = (pInvokes.SafeNativeMethods.DOMAIN_CONTROLLER_INFO)Marshal.PtrToStructure(pDCI, typeof(pInvokes.SafeNativeMethods.DOMAIN_CONTROLLER_INFO));
            }
            else
            {
                LibraryLogging.Error("GetDomainInfo({0}) Error:{1} {2}", domain, ret, LastError(ret));
            }

            if (pDCI != IntPtr.Zero)
            {
                pInvokes.SafeNativeMethods.NetApiBufferFree(pDCI);
            }

            return domainInfo;
        }

        /// <summary>
        /// get local machine domain membership as DNS name
        /// </summary>
        /// <returns></returns>
        public static string GetMachineDomainMembershipEX()
        {
            return GetDomainInfo("").DomainName;
        }

        /// <summary>
        /// get local machine domain membership as NETBIOS name
        /// </summary>
        /// <returns></returns>
        public static string GetMachineDomainMembership()
        {
            IntPtr buffer = IntPtr.Zero;
            pInvokes.SafeNativeMethods.WKSTA_INFO_100 wksta_info = new pInvokes.SafeNativeMethods.WKSTA_INFO_100();

            int result = pInvokes.SafeNativeMethods.NetWkstaGetInfo(null, 100, out buffer);
            if (result == 0)
            {
                wksta_info = (pInvokes.SafeNativeMethods.WKSTA_INFO_100)Marshal.PtrToStructure(buffer, typeof(pInvokes.SafeNativeMethods.WKSTA_INFO_100));
            }
            else
            {
                LibraryLogging.Error("GetMachineDomainMembership() Error:{0} {1}", result, LastError(result));
            }

            if (buffer != IntPtr.Zero)
            {
                pInvokes.SafeNativeMethods.NetApiBufferFree(buffer);
            }

            return wksta_info.lan_group;
        }

        /// <summary>
        /// returns GetLastWin32Error as string
        /// </summary>
        /// <returns></returns>
        public static string LastError(int error)
        {
            return new Win32Exception(error).Message;

        }

        /// <summary>
        /// returns GetLastWin32Error as string
        /// </summary>
        /// <returns></returns>
        public static string LastError()
        {
            return new Win32Exception(Marshal.GetLastWin32Error()).Message;
        }
    }
}
