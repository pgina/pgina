using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Dynamic;

namespace pGina.Core.Messages
{
    public class LoginRequestMessage : MessageBase
    {        
        public string Username { get; set; }
        public string Password { get; set; }
        public string Domain { get; set; }
        public int Session { get; set; }

        public LoginRequestMessage(dynamic expandoVersion)
        {
            FromExpando(expandoVersion);
        }

        public LoginRequestMessage()
        {
        }

        public override void FromExpando(dynamic expandoVersion)
        {
            Username = expandoVersion.Username;
            Password = expandoVersion.Password;
            Domain = expandoVersion.Domain;
            Session = expandoVersion.Session;
        }

        public override dynamic ToExpando()
        {
            dynamic exp = new ExpandoObject();
            exp.Username = this.Username;
            exp.Password = this.Password;
            exp.Domain = this.Domain;
            exp.Session = this.Session;
            exp.MessageType = (byte) MessageType.LoginRequest;
            return exp;
        }
    }
}
