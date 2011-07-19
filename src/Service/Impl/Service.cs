using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace pGina.Service.Impl
{
    public class Service
    {
        static Service()
        {
            Logging.InitializeLogging();
            System.Diagnostics.Debug.WriteLine("Huh");
            System.Diagnostics.Trace.WriteLine("Huh2");
        }
    }
}
