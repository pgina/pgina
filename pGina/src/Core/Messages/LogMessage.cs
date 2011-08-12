using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Dynamic;

namespace pGina.Core.Messages
{
    public class LogMessage : MessageBase
    {
        public string LoggerName { get; set; }
        public string LoggedMessage { get; set; }
        public string Level { get; set; }

        public LogMessage(dynamic expandoVersion)
        {
            FromExpando(expandoVersion);
        }        

        public LogMessage()
        {
        }

        public override void FromExpando(dynamic expandoVersion)
        {
            LoggerName = expandoVersion.LoggerName;
            Level = expandoVersion.Level;
            LoggedMessage = expandoVersion.LoggedMessage;
        }

        public override dynamic ToExpando()
        {
            dynamic exp = new ExpandoObject();
            exp.LoggerName = this.LoggerName;
            exp.Level = this.Level;
            exp.LoggedMessage = this.LoggedMessage;
            exp.MessageType = (byte) MessageType.Log;
            return exp;
        }
    }
}
