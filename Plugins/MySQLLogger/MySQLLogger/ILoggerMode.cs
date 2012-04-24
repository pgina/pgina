using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace pGina.Plugin.MySqlLogger
{
    interface ILoggerMode
    {
        bool Log(System.ServiceProcess.SessionChangeDescription changeDescription, pGina.Shared.Types.SessionProperties properties);
        string TestTable();
        string CreateTable();
    
    }

    class LoggerModeFactory
    {
        private LoggerModeFactory() { }
        public static ILoggerMode getLoggerMode(LoggerMode mode)
        {
            if (mode == LoggerMode.EVENT)
                return new EventLoggerMode();
            else if (mode == LoggerMode.SESSION)
                return new SessionLogger();
            throw new ArgumentException("Invalid LoggerMode");
        }
    }
}
