using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Principal;

namespace pGina.Interfaces
{
    public class GroupInformation
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public SecurityIdentifier SID { get; set; }
    }
    
    public class UserInformation
    {
        public List<GroupInformation> Groups { get; set; }
        // Currently ignored if plugin sets this.. but possibly useful if we go LSA in the future...
        public SecurityIdentifier SID { get; set; } 
        public string Username { get; set; }
        public string Password { get; set; }
        public string Description { get; set; }
    }
}
