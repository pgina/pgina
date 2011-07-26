using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Principal;

namespace pGina.Shared.Types
{
    public class GroupInformation
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public SecurityIdentifier SID { get; set; }
    }    
}
