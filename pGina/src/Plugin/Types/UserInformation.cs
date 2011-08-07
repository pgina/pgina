using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Principal;

namespace pGina.Shared.Types
{
    public class UserInformation
    {
        public List<GroupInformation> Groups { get; set; }
        // Currently ignored if plugin sets this.. but possibly useful if we go LSA in the future...
        public SecurityIdentifier SID { get; set; } 
        public string Username { get; set; }
        public string Domain { get; set; }      // Null == local machine
        public string Password { get; set; }
        public string Description { get; set; }

        public UserInformation()
        {
            Groups = new List<GroupInformation>();
        }
    }
}
