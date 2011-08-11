using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

namespace pGina.Core.Messages
{
    public class LoginResponseMessage : LoginRequestMessage
    {
        public bool Result { get; set; }
        public string Message { get; set; }

        public LoginResponseMessage(dynamic expandoVersion)            
        {
            FromExpando(expandoVersion);
        }

        public LoginResponseMessage()
        {
        }

        public override void FromExpando(dynamic expandoVersion)
        {
            base.FromExpando((ExpandoObject) expandoVersion);
            Result = expandoVersion.Result;
            Message = expandoVersion.Message;
        }

        public override dynamic ToExpando()
        {
            dynamic exp = base.ToExpando();
            exp.Result = this.Result;
            exp.Message = this.Message;
            exp.MessageType = (byte) MessageType.LoginResponse;
            return exp;
        }
    }
}
