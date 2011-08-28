using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

namespace pGina.Core.Messages
{
    public class DynamicLabelResponseMessage : DynamicLabelRequestMessage
    {
        public string Text { get; set; }

        public DynamicLabelResponseMessage(dynamic expandoVersion)            
        {
            FromExpando(expandoVersion);
        }

        public DynamicLabelResponseMessage()
        {
        }

        public override void FromExpando(dynamic expandoVersion)
        {
            base.FromExpando((ExpandoObject) expandoVersion);
            Text = expandoVersion.Text;
        }

        public override dynamic ToExpando()
        {
            dynamic exp = base.ToExpando();
            exp.Text = this.Text;
            exp.MessageType = (byte) MessageType.DynLabelResponse;
            return exp;
        }
    }
}
