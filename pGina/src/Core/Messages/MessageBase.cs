using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace pGina.Core.Messages
{
    public abstract class MessageBase
    {
        public abstract void FromExpando(dynamic expandoVersion);
        public abstract dynamic ToExpando();
    }
}
