using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Dynamic;

namespace pGina.Core.Messages
{
    public class HelperInfoRequestMessage : MessageBase
    {
        public int Key { get; set; }
        
        public HelperInfoRequestMessage(dynamic expandoVersion)
        {
            FromExpando(expandoVersion);
        }

        public HelperInfoRequestMessage()
        {
        }

        public override void FromExpando(dynamic expandoVersion)
        {
            Key = expandoVersion.Key;            
        }

        public override dynamic ToExpando()
        {
            dynamic exp = new ExpandoObject();
            exp.Key = this.Key;
            exp.MessageType = (byte)MessageType.InfoRequest;
            return exp;
        }
    }
}
