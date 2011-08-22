using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using log4net;

namespace pGina.Plugin.ScriptRunner
{
    internal abstract class Script
    {
        public string File { get; set; }
        public bool UserSession { get; set; }
        public bool SystemSession { get; set; }
        public string Type
        {
            get
            {
                if (this is BatchScript)
                    return "Batch";
                if (this is PowerShellScript)
                    return "PowerShell";
                return "Unknown";
            }
        }

        public static int Timeout { get; set; }

        static Script()
        {
            Timeout = 30000;
        }
        
        public Script()
        {
            this.File = null;
        }

        public abstract void Run();
    }
}
