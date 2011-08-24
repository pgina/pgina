using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Dynamic;

namespace pGina.Core.Messages
{
    public class HelperInfoResponseMessage : MessageBase
    {
        public bool Success { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Domain { get; set; }

        public HelperInfoResponseMessage(dynamic expandoVersion)
        {
            FromExpando(expandoVersion);
        }

        public HelperInfoResponseMessage()
        {
        }

        public override void FromExpando(dynamic expandoVersion)
        {
            Success = expandoVersion.Success;
            Username = expandoVersion.Username;
            Password = expandoVersion.Password;
            Domain = expandoVersion.Domain;
        }

        public override dynamic ToExpando()
        {
            dynamic exp = new ExpandoObject();
            exp.Username = this.Username;
            exp.Password = this.Password;
            exp.Domain = this.Domain;
            exp.Success = this.Success;
            exp.MessageType = (byte)MessageType.InfoResponse;
            return exp;
        }
    }
}
