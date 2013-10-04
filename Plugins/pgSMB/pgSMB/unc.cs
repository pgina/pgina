/*
	Copyright (c) 2012, pGina Team
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
using System.ComponentModel;

using log4net;

//based on http://social.msdn.microsoft.com/Forums/et-EE/netfxbcl/thread/58159e0e-aa45-4d46-a128-596c3d23ff5c
namespace pGina.Plugin.pgSMB
{
    public class unc
    {
        private static ILog m_logger = LogManager.GetLogger("pgSMB[unc]");

        #region Structs/Enums

        [StructLayout(LayoutKind.Sequential)]
        private class NETRESOURCE
        {
            public int dwScope = 0;
            public int dwType = 0;
            public int dwDisplayType = 0;
            public int dwUsage = 0;
            public string lpLocalName = "";
            public string lpRemoteName = "";
            public string lpComment = "";
            public string lpProvider = "";
        }
        private struct Resouces
        {
            public const int RESOURCE_CONNECTED = 0x00000001;
            public const int RESOURCE_GLOBALNET = 0x00000002;
            public const int RESOURCE_REMEMBERED = 0x00000003;
            public const int RESOURCETYPE_ANY = 0x00000000;
            public const int RESOURCETYPE_DISK = 0x00000001;
            public const int RESOURCETYPE_PRINT = 0x00000002;
            public const int RESOURCEDISPLAYTYPE_GENERIC = 0x00000000;
            public const int RESOURCEDISPLAYTYPE_DOMAIN = 0x00000001;
            public const int RESOURCEDISPLAYTYPE_SERVER = 0x00000002;
            public const int RESOURCEDISPLAYTYPE_SHARE = 0x00000003;
            public const int RESOURCEDISPLAYTYPE_FILE = 0x00000004;
            public const int RESOURCEDISPLAYTYPE_GROUP = 0x00000005;
            public const int RESOURCEUSAGE_CONNECTABLE = 0x00000001;
            public const int RESOURCEUSAGE_CONTAINER = 0x00000002;
        }
        private struct Connect
        {
            public const int CONNECT_INTERACTIVE = 0x00000008;
            public const int CONNECT_PROMPT = 0x00000010;
            public const int CONNECT_REDIRECT = 0x00000080;
            public const int CONNECT_UPDATE_PROFILE = 0x00000001;
            public const int CONNECT_COMMANDLINE = 0x00000800;
            public const int CONNECT_CMD_SAVECRED = 0x00001000;
            public const int CONNECT_LOCALDRIVE = 0x00000100;
        }
        #endregion

        #region Mpr.dll
        [DllImport("Mpr.dll")]
        private static extern int WNetUseConnection(IntPtr hwndOwner, NETRESOURCE lpNetResource, string lpPassword, string lpUserID, int dwFlags, string lpAccessName, string lpBufferSize, string lpResult);

        [DllImport("Mpr.dll")]
        private static extern int WNetCancelConnection2(string lpName, int dwFlags, bool fForce);
        #endregion

        #region kernel32.dll
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool GetDiskFreeSpaceEx(string drive, out long freeBytesForUser, out long totalBytes, out long freeBytes);
        #endregion

        public static Boolean connectToRemote(string remoteUNC, string username, string password)
        {
            return connectToRemote(remoteUNC, username, password, false);
        }

        public static Boolean connectToRemote(string remoteUNC, string username, string password, bool promptUser)
        {
            m_logger.InfoFormat("Connecting to {0} as {1}", remoteUNC, username);

            NETRESOURCE nr = new NETRESOURCE();
            nr.dwType = Resouces.RESOURCETYPE_DISK;
            nr.lpRemoteName = remoteUNC;
            // nr.lpLocalName = "F:";

            int ret;
            if (promptUser)
                ret = WNetUseConnection(IntPtr.Zero, nr, "", "", Connect.CONNECT_INTERACTIVE | Connect.CONNECT_PROMPT, null, null, null);
            else
                ret = WNetUseConnection(IntPtr.Zero, nr, password, username, 0, null, null, null);

            if (ret != 0)
            {
                string errorMessage = new Win32Exception(Marshal.GetLastWin32Error()).ToString();
                m_logger.ErrorFormat("Unable to connect to {0} as {1} Error:{2} {3}", remoteUNC,  username, ret, errorMessage);
                return false;
            }

            return true;
        }

        public static Boolean disconnectRemote(string remoteUNC)
        {
            m_logger.InfoFormat("Disconnecting from {0}", remoteUNC);

            int ret = WNetCancelConnection2(remoteUNC, Connect.CONNECT_UPDATE_PROFILE, false);
            if ((ret != 0) && (ret != 2250/*This network connection does not exist*/))
            {
                string errorMessage = new Win32Exception(Marshal.GetLastWin32Error()).ToString();
                m_logger.ErrorFormat("Unable to disConnect from {0} Error:{1} {2}", remoteUNC, ret, errorMessage);
                return false;
            }

            return true;
        }

        public static long GetFreeShareSpace(string share)
        {
            long freeBytesForUser = -1, totalBytes = -1,freeBytes = -1;

            if (!GetDiskFreeSpaceEx(share, out freeBytesForUser, out totalBytes, out freeBytes))
            {
                string errorMessage = new Win32Exception(Marshal.GetLastWin32Error()).ToString();
                m_logger.ErrorFormat("Unable to enumerate free space on {0} Error:{1}", share, errorMessage);
                return -1;
            }
            return freeBytesForUser;
        }
    }
}
