using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Abstractions.WindowsApi;

namespace Abstractions.Process
{
    public static class Execution
    {
        public static bool StartInSession(int sessionId, string application, string[] arguments, bool sessionUserContext)
        {
            // If sessionUserContext:
            //  WTSQueryUserToken() to get user's token (with correct session id already in place)
            //  CreateProcessWithToken()
            // else
            //  GetCurrent process token
            //  Duplicate to get a primary token
            //  SetTokenInformation to adjust session id
            //  CreateProcessWithToken()            
            return false;
        }

        public static bool StartAsUserInSession(int sessionId, string application, string[] arguments)
        {
            IntPtr userToken = pInvokes.WTSQueryUserToken(sessionId);
            if (userToken == IntPtr.Zero)
                return false;            
        


            //  WTSQueryUserToken() to get user's token (with correct session id already in place)
            //  CreateProcessWithToken()
            return true;
        }

        public static bool StartInSession(int sessionId, string application, string[] arguments)
        {
            return false;
            //  GetCurrent process token
            //  Duplicate to get a primary token
            //  SetTokenInformation to adjust session id
            //  CreateProcessWithToken()            
        }
    }
}
