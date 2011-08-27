using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Principal;

namespace Abstractions.Windows
{
    public class Security
    {
        public static SecurityIdentifier GetWellknownSID(WellKnownSidType type)
        {
            return new SecurityIdentifier(type, null);
        }

        public static string GetWellKnownName(WellKnownSidType type)
        {
            return GetNameFromSID(GetWellknownSID(type));
        }

        public static string GetNameFromSID(SecurityIdentifier sid)
        {
            NTAccount ntAccount = (NTAccount)sid.Translate(typeof(NTAccount));
            return ntAccount.ToString();
        }

        public static SecurityIdentifier GetSIDFromName(string name)
        {
            NTAccount ntAccount = new NTAccount(name);
            return (SecurityIdentifier)ntAccount.Translate(typeof(SecurityIdentifier));
        }
    }
}
