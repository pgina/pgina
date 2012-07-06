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
        /// </summary>
        public SecurityIdentifier SID { get; set; }
        
        /// <summary>
        /// The username provided by the user.
        /// </summary>
        public string OriginalUsername { get; set; } 
 
        /// <summary>
        /// The username
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The domain (a null value indicates that this is a local account).
        /// </summary>
        public string Domain { get; set; } 

        /// <summary>
        /// The password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Description of this user
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The full name associated with this user account.
        /// </summary>
        public string Fullname { get; set; }

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
