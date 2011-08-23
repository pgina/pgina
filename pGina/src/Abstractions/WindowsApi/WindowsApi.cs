using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Reflection;
using System.Net;
using System.Net.Security;

namespace Abstractions.WindowsApi
{
    public class WindowsApi
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

            #region mpr.dll enums
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
            #endregion
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
    }
}
