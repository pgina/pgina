using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Abstractions.Logging
{
    public static class LibraryLogging
    {
        public delegate void MessageHandler(string message, params object[] args);

        public enum Level
        {
            Info,
            Debug,
            Warn,
            Error
        }

        private static object s_mutex = new object();
        private static event MessageHandler InfoEvent;
        private static event MessageHandler DebugEvent;
        private static event MessageHandler WarnEvent;
        private static event MessageHandler ErrorEvent;        

        public static void Info(string message, params object[] args)
        {
            if(InfoEvent != null)
                InfoEvent(message, args);
        }

        public static void Debug(string message, params object[] args)
        {
            if (DebugEvent != null)
                DebugEvent(message, args);
        }

        public static void Warn(string message, params object[] args)
        {
            if (WarnEvent != null)
                WarnEvent(message, args);
        }

        public static void Error(string message, params object[] args)
        {
            if (ErrorEvent != null)
                ErrorEvent(message, args);
        }

        public static void AddListener(Level level, MessageHandler handler)
        {
            lock(s_mutex)
            {
                switch(level)
                {
                    case Level.Info:
                        InfoEvent += handler;
                        break;
                    case Level.Debug:
                        DebugEvent += handler;
                        break;
                    case Level.Warn:
                        WarnEvent += handler;
                        break;
                    case Level.Error:
                        ErrorEvent += handler;
                        break;
                }
            }
        }

        public static void RemoveListener(Level level, MessageHandler handler)
        {
            lock(s_mutex)
            {
                switch(level)
                {
                    case Level.Info:
                        InfoEvent -= handler;
                        break;
                    case Level.Debug:
                        DebugEvent -= handler;
                        break;
                    case Level.Warn:
                        WarnEvent -= handler;
                        break;
                    case Level.Error:
                        ErrorEvent -= handler;
                        break;
                }
            }
        }
    }  
}
