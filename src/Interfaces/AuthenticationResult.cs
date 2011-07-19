using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace pGina.Interfaces
{
    public abstract class AuthenticationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }        
    }

    // Basic u/p credentials to be serialized on success (for cred prov)
    public class BasicAuthenticationResult : AuthenticationResult
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Domain { get; set; }
    }

    // TBD: Advanced result, could use seperate auth package, or 
    //  provide custom serialized result directly (for cred prov)
}
