using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace pGina.Plugin.HttpAuth
{
    public class UInfo
    {
        public string whyCannotLogin;
        public string uname;
        public string fullName;
        public string email;
        public string[] groups;

        public static UInfo parseResponse(string res)
        {
            UInfo u = new UInfo();
            using (StringReader strReader = new StringReader(res))
            {
                // reason why could not login (empty = can login)
                u.whyCannotLogin = strReader.ReadLine();
                u.uname = strReader.ReadLine();
                if (u.uname == null)
                {
                    throw new Exception("Bad response arrived: " + res);
                }
                u.fullName = strReader.ReadLine();
                u.email = strReader.ReadLine();
                u.groups = strReader.ReadLine().Split(';');
                if (u.groups.Length == 1 && u.groups[0].Contains(";"))
                {
                    throw new Exception("Bad response arrived (groups wrong): " + res);
                }
            }

            return u;
        }
    }
}
