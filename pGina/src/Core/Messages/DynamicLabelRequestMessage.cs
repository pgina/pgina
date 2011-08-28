using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

namespace pGina.Core.Messages
{
    public class DynamicLabelRequestMessage : MessageBase
    {
        public string Name { get; set; }

        public DynamicLabelRequestMessage(dynamic expandoVersion)
        {
            FromExpando(expandoVersion);
        }

        public DynamicLabelRequestMessage()
        {
        }

        public override void FromExpando(dynamic expandoVersion)
        {
            Name = expandoVersion.Name;
        }

        public override dynamic ToExpando()
        {
            dynamic exp = new ExpandoObject();
            exp.Name = this.Name;
            exp.MessageType = (byte) MessageType.DynLabelRequest;
            return exp;
        }
    }
}
