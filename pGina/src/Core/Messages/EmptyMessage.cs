using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

namespace pGina.Core.Messages
{
    public class EmptyMessage : MessageBase
    {
        public MessageType MessageType { get; set; }

        public EmptyMessage()
        {
        }

        public EmptyMessage(MessageType type)
        {
            this.MessageType = type;
        }

        public override void FromExpando(dynamic expandoVersion)
        {
            this.MessageType = (MessageType) expandoVersion.MessageType;
        }

        public override dynamic ToExpando()
        {
            dynamic exp = new ExpandoObject();
            exp.MessageType = (byte)this.MessageType;
            return exp;
        }
    }
}
