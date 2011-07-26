using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using log4net;

namespace pGina.Service.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            ILog logger = LogManager.GetLogger("pGina.Console");
            pGina.Service.Impl.Service service = new pGina.Service.Impl.Service();
            logger.DebugFormat("Service created, using plugin directories: ");
            foreach (string dir in service.PluginDirectories)
                logger.DebugFormat("  {0}", dir);

            service.Start();
        }
    }
}
