/*
	Copyright (c) 2014, pGina Team
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
using System.Security.Principal;

namespace pGina.Shared.Types
{
    public class UserInformation
    {
        /// <summary>
        /// The list of groups for the user account
        /// </summary>
        public List<GroupInformation> Groups { get; set; }

        // Currently ignored if plugin sets this.. but possibly useful if we go LSA in the future...
        /// <summary>
        /// The SID for the user (currently ignored)
        /// default: Nobody
        /// </summary>
        public SecurityIdentifier SID
        {
            get
            {
                return (_SID == null) ? new SecurityIdentifier("S-1-0-0") : _SID;
            }
            set
            {
                _SID = value;
            }
        }
        private SecurityIdentifier _SID;
        /// <summary>
        /// a valid user will return true
        /// </summary>
        public bool HasSID
        {
            get
            {
                return !SID.ToString().Equals("S-1-0-0", StringComparison.CurrentCultureIgnoreCase);
            }
        }

        /// <summary>
        /// The username provided by the user.
        /// </summary>
        public string OriginalUsername
        {
            get
            {
                return (string.IsNullOrEmpty(_OriginalUsername)) ? "" : _OriginalUsername;
            }
            set
            {
                _OriginalUsername = value;
            }
        }
        private string _OriginalUsername;

        /// <summary>
        /// The username
        /// </summary>
        public string Username
        {
            get
            {
                return (string.IsNullOrEmpty(_Username)) ? "" : _Username;
            }
            set
            {
                _Username = value;
            }
        }
        private string _Username;

        /// <summary>
        /// The domain (a null value indicates that this is a local account).
        /// </summary>
        public string Domain
        {
            get
            {
                return (string.IsNullOrEmpty(_Domain)) ? "" : _Domain;
            }
            set
            {
                _Domain = value;
            }
        }
        private string _Domain;

        /// <summary>
        /// The password
        /// </summary>
        public string Password
        {
            get
            {
                return (string.IsNullOrEmpty(_Password)) ? "" : _Password;
            }
            set
            {
                _Password = value;
            }
        }
        private string _Password;

        /// <summary>
        /// The old password
        /// </summary>
        public string oldPassword
        {
            get
            {
                return (string.IsNullOrEmpty(_oldPassword)) ? "" : _oldPassword;
            }
            set
            {
                _oldPassword = value;
            }
        }
        private string _oldPassword;

        /// <summary>
        /// Is password expired
        /// </summary>
        public bool PasswordEXP
        {
            get
            {
                return (_PasswordEXP) ? true : false;
            }
            set
            {
                _PasswordEXP = value;
            }
        }
        private bool _PasswordEXP;

        /// <summary>
        /// how long until the password does expire
        /// </summary>
        public TimeSpan PasswordEXPcntr { get; set; }

        /// <summary>
        /// Description of this user
        /// </summary>
        public string Description
        {
            get
            {
                return (string.IsNullOrEmpty(_Description)) ? "" : _Description;
            }
            set
            {
                _Description = value;
            }
        }
        private string _Description;

        /// <summary>
        /// The full name associated with this user account.
        /// </summary>
        public string Fullname
        {
            get
            {
                return (string.IsNullOrEmpty(_Fullname)) ? "" : _Fullname;
            }
            set
            {
                _Fullname = value;
            }
        }
        private string _Fullname;

        /// <summary>
        /// The email address associated with this user account.
        /// </summary>
        public string Email
        {
            get
            {
                return (string.IsNullOrEmpty(_Email)) ? "" : _Email;
            }
            set
            {
                _Email = value;
            }
        }
        private string _Email;

        /// <summary>
        /// The LoginScript associated with this user account.
        /// </summary>
        public string LoginScript
        {
            get
            {
                return (string.IsNullOrEmpty(_LoginScript)) ? "" : _LoginScript;
            }
            set
            {
                _LoginScript = value;
            }
        }
        private string _LoginScript;

        /// <summary>
        /// User Profile: Full Name
        /// </summary>
        public string usri4_full_name
        {
            get
            {
                return (string.IsNullOrEmpty(_usri4_full_name)) ? "" : _usri4_full_name;
            }
            set
            {
                _usri4_full_name = value;
            }
        }
        private string _usri4_full_name;

        /// <summary>
        /// User Profile: max profile size in kbytes
        /// </summary>
        public UInt32 usri4_max_storage { get; set; }

        /// <summary>
        /// User Profile: Where to store the roaming profile
        /// </summary>
        public string usri4_profile
        {
            get
            {
                return (string.IsNullOrEmpty(_usri4_profile)) ? "" : _usri4_profile;
            }
            set
            {
                _usri4_profile = value;
            }
        }
        private string _usri4_profile;

        /// <summary>
        /// User Profile: Home drive letter
        /// </summary>
        public string usri4_home_dir_drive
        {
            get
            {
                return (string.IsNullOrEmpty(_usri4_home_dir_drive)) ? "" : _usri4_home_dir_drive;
            }
            set
            {
                _usri4_home_dir_drive = value;
            }
        }
        private string _usri4_home_dir_drive;

        /// <summary>
        /// User Profile: Home drive path
        /// </summary>
        public string usri4_home_dir
        {
            get
            {
                return (string.IsNullOrEmpty(_usri4_home_dir)) ? "" : _usri4_home_dir;
            }
            set
            {
                _usri4_home_dir = value;
            }
        }
        private string _usri4_home_dir;

        /// <summary>
        /// User Profile: pgSMB plugin: compressed filename %u.wim
        /// </summary>
        public string pgSMB_Filename
        {
            get
            {
                return (string.IsNullOrEmpty(_pgSMB_Filename)) ? "" : _pgSMB_Filename;
            }
            set
            {
                _pgSMB_Filename = value;
            }
        }
        private string _pgSMB_Filename;

        /// <summary>
        /// User Profile: pgSMB plugin: profile share name
        /// </summary>
        public string pgSMB_SMBshare
        {
            get
            {
                return (string.IsNullOrEmpty(_pgSMB_SMBshare)) ? "" : _pgSMB_SMBshare;
            }
            set
            {
                _pgSMB_SMBshare = value;
            }
        }
        private string _pgSMB_SMBshare;

        public string script_authe_sys
        {
            get
            {
                return (string.IsNullOrEmpty(_script_authe_sys)) ? "" : _script_authe_sys;
            }
            set
            {
                _script_authe_sys = value;
            }
        }
        private string _script_authe_sys;

        public string script_authe_usr
        {
            get
            {
                return (string.IsNullOrEmpty(_script_authe_usr)) ? "" : _script_authe_usr;
            }
            set
            {
                _script_authe_usr = value;
            }
        }
        private string _script_authe_usr;

        public string script_autho_sys
        {
            get
            {
                return (string.IsNullOrEmpty(_script_autho_sys)) ? "" : _script_autho_sys;
            }
            set
            {
                _script_autho_sys = value;
            }
        }
        private string _script_autho_sys;

        public string script_autho_usr
        {
            get
            {
                return (string.IsNullOrEmpty(_script_autho_usr)) ? "" : _script_autho_usr;
            }
            set
            {
                _script_autho_usr = value;
            }
        }
        private string _script_autho_usr;

        public string script_gateway_sys
        {
            get
            {
                return (string.IsNullOrEmpty(_script_gateway_sys)) ? "" : _script_gateway_sys;
            }
            set
            {
                _script_gateway_sys = value;
            }
        }
        private string _script_gateway_sys;

        public string script_gateway_usr
        {
            get
            {
                return (string.IsNullOrEmpty(_script_gateway_usr)) ? "" : _script_gateway_usr;
            }
            set
            {
                _script_gateway_usr = value;
            }
        }
        private string _script_gateway_usr;

        public string script_notication_sys
        {
            get
            {
                return (string.IsNullOrEmpty(_script_notication_sys)) ? "" : _script_notication_sys;
            }
            set
            {
                _script_notication_sys = value;
            }
        }
        private string _script_notication_sys;

        public string script_notication_usr
        {
            get
            {
                return (string.IsNullOrEmpty(_script_notication_usr)) ? "" : _script_notication_usr;
            }
            set
            {
                _script_notication_usr = value;
            }
        }
        private string _script_notication_usr;

        public string script_changepwd_sys
        {
            get
            {
                return (string.IsNullOrEmpty(_script_changepwd_sys)) ? "" : _script_changepwd_sys;
            }
            set
            {
                _script_changepwd_sys = value;
            }
        }
        private string _script_changepwd_sys;

        public string script_changepwd_usr
        {
            get
            {
                return (string.IsNullOrEmpty(_script_changepwd_usr)) ? "" : _script_changepwd_usr;
            }
            set
            {
                _script_changepwd_usr = value;
            }
        }
        private string _script_changepwd_usr;

        public UserInformation()
        {
            Groups = new List<GroupInformation>();
        }

        public bool InGroup(GroupInformation group)
        {
            foreach (GroupInformation exGroup in Groups)
            {
                if (exGroup.Name == group.Name)
                {
                    // Copy new sid if old isn't set
                    if (exGroup.SID == null && group.SID != null)
                        exGroup.SID = group.SID;

                    return true;
                }

                if (exGroup.SID != null && group.SID != null && exGroup.SID == group.SID)
                    return true;
            }

            return false;
        }

        // Adds a group and checks for duplicates (skips if dupl)
        public bool AddGroup(GroupInformation group)
        {
            if (!InGroup(group))
            {
                // No dupl
                Groups.Add(group);
                return true;
            }

            return false;
        }
    }
}
