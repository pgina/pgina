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
using System.DirectoryServices;

using log4net;

//based on http://social.msdn.microsoft.com/forums/en-us/csharpgeneral/thread/B70B79D9-971F-4D6F-8462-97FC126DE0AD
namespace pGina.Plugin.pgSMB
{
    class UserGroup
    {
        private static ILog m_logger = LogManager.GetLogger("pgSMB[UserGroup]");

        #region Structs/Enums

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct USER_INFO_1052
        {
            public IntPtr profile;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct USER_INFO_1
        {
            public string usri1_name;
            public string usri1_password;
            public int usri1_password_age;
            public UserPrivileges usri1_priv;
            public string usri1_home_dir;
            public string comment;
            public int usri1_flags;
            public string usri1_script_path;
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
            UF_PASSWD_NOTREQD = 0x0020,
            UF_PASSWD_CANT_CHANGE = 0x0040,
            UF_LOCKOUT = 0x0010,
            UF_DONT_EXPIRE_PASSWD = 0x10000,
            UF_ENCRYPTED_TEXT_PASSWORD_ALLOWED = 0x0080,
            UF_NOT_DELEGATED = 0x100000,
            UF_SMARTCARD_REQUIRED = 0x40000,
            UF_USE_DES_KEY_ONLY = 0x200000,
            UF_DONT_REQUIRE_PREAUTH = 0x400000,
            UF_TRUSTED_FOR_DELEGATION = 0x80000,
            UF_PASSWORD_EXPIRED = 0x800000,
            UF_TRUSTED_TO_AUTHENTICATE_FOR_DELEGATION = 0x1000000,
            UF_NORMAL_ACCOUNT = 0x0200,
            UF_TEMP_DUPLICATE_ACCOUNT = 0x0100,
            UF_WORKSTATION_TRUST_ACCOUNT = 0x1000,
            UF_SERVER_TRUST_ACCOUNT = 0x2000,
            UF_INTERDOMAIN_TRUST_ACCOUNT = 0x0800,
        }
        public enum AuthFlags
        {
            AF_OP_PRINT = 0x1,
            AF_OP_COMM = 0x2,
            AF_OP_SERVER = 0x4,
            AF_OP_ACCOUNTS = 0x8,
        }
        #endregion

        #region Netapi32.dll
        [DllImport("Netapi32.dll", SetLastError = true)]
        private static extern int NetUserAdd([MarshalAs(UnmanagedType.LPWStr)]string servername, UInt32 level, ref USER_INFO_1 buf, out Int32 parm_err);

        [DllImport("Netapi32.dll", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true)]
        private extern static int NetUserGetInfo([MarshalAs(UnmanagedType.LPWStr)] string ServerName, [MarshalAs(UnmanagedType.LPWStr)] string UserName, int level, out IntPtr BufPtr);

        [DllImport("netapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern int NetUserSetInfo([MarshalAs(UnmanagedType.LPWStr)] string servername, string username, int level, ref USER_INFO_4 buf, out Int32 parm_err);

        [DllImport("Netapi32.dll", SetLastError = true)]
        private static extern int NetApiBufferFree(IntPtr Buffer);

        [DllImport("Netapi32.dll", SetLastError = true)]
        private static extern int NetUserDel([MarshalAs(UnmanagedType.LPWStr)] string servername, [MarshalAs(UnmanagedType.LPWStr)] string username);

        [DllImport("Netapi32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        private static extern int NetUserChangePassword([MarshalAs(UnmanagedType.LPWStr)] string domainname, [MarshalAs(UnmanagedType.LPWStr)] string username, [MarshalAs(UnmanagedType.LPWStr)] string oldpassword, [MarshalAs(UnmanagedType.LPWStr)] string newpassword);
        #endregion

        public static Boolean UserExists(string username)
        {
            try
            {
                return DirectoryEntry.Exists("WinNT://" + Environment.MachineName + "/" + username);
            }
            catch
            {
                return false;
            }
        }

        public static Boolean UserADD(string username, string password)
        {
            m_logger.InfoFormat("Add user {0}", username);
            USER_INFO_1 NewUser = new USER_INFO_1();

            NewUser.usri1_name = username; // Allocates the username
            NewUser.usri1_password = password; // allocates the password
            NewUser.usri1_priv = UserPrivileges.USER_PRIV_USER; // Sets the account type to USER_PRIV_USER
            NewUser.usri1_home_dir = null; // We didn't supply a Home Directory
            NewUser.comment = "pGina created pgSMB"; // Comment on the User
            NewUser.usri1_script_path = null; // We didn't supply a Logon Script Path

            Int32 ret;
            NetUserAdd(Environment.MachineName, 1, ref NewUser, out ret);
            //2224 The user account already exists. NERR_UserExists
            if ((ret != 0) && (ret != 2224)) // If the call fails we get a non-zero value
            {
                string errorMessage = new Win32Exception(Marshal.GetLastWin32Error()).ToString();
                m_logger.ErrorFormat("NetUserAdd error:{0} {1}", ret, errorMessage);
                return false;
            }

            return true;
        }

        public static Boolean UserMod(string username, USER_INFO_4 userInfo4)
        {
            m_logger.InfoFormat("modify user settings for {0}", username);
            Int32 ret = 1;

            NetUserSetInfo(null, username, 4, ref userInfo4, out ret);
            if (ret != 0) // If the call fails we get a non-zero value
            {
                string errorMessage = new Win32Exception(Marshal.GetLastWin32Error()).ToString();
                m_logger.ErrorFormat("NetUserSetInfo error:{0} {1}", ret, errorMessage);
                return false;
            }

            return true;
        }

        public static Boolean UserGet(string username, ref USER_INFO_4 userinfo4)
        {
            m_logger.InfoFormat("get user settings for {0}", username);
            IntPtr bufPtr;

            int lngReturn = NetUserGetInfo(null, username, 4, out bufPtr);
            if (lngReturn == 0)
            {
                userinfo4 = (USER_INFO_4)Marshal.PtrToStructure(bufPtr, typeof(USER_INFO_4));
            }
            else
            {
                string errorMessage = new Win32Exception(Marshal.GetLastWin32Error()).ToString();
                m_logger.ErrorFormat("NetUserGetInfo error:{0} {1}", lngReturn, errorMessage);
            }

            NetApiBufferFree(bufPtr);
            bufPtr = IntPtr.Zero;

            if (lngReturn == 0)
                return true;

            return false;
        }

        public static Boolean UserDel(string username)
        {
            m_logger.InfoFormat("delete user {0}", username);
            Int32 ret;

            ret = NetUserDel(null, username);
            if ((ret != 0) && (ret != 2221)) // If the call fails we get a non-zero value
            {
                string errorMessage = new Win32Exception(Marshal.GetLastWin32Error()).ToString();
                m_logger.ErrorFormat("NetUserDel error:{0} {1}", ret, errorMessage);
                return false;
            }

            return true;
        }
    }
}
