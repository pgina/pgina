using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

using log4net;
using log4net.Config;

namespace pGina.Core
{
    public static class Logging
    {
        public static void Init()
        {
            string curPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string l4nConfig = string.Format("{0}\\{1}", curPath, "log4net.config");

            if (!File.Exists(l4nConfig))
            {
                string curDir = Directory.GetCurrentDirectory();
                l4nConfig = string.Format("{0}\\{1}", curDir, "log4net.config");
            }

            XmlConfigurator.ConfigureAndWatch(new FileInfo(l4nConfig));
            LogManager.GetLogger("Startup").InfoFormat("Starting up, log4net configured from: {0}", l4nConfig);
        }
    }
}
