using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using log4net;
using log4net.Appender;

namespace pGina.Shared.Logging
{
    public class InProcAppender : AppenderSkeleton
    {
        public delegate void MessageHandler(string message);

        private static event MessageHandler Message;

        private static object s_mutex = new object();

        public void AddListener(MessageHandler handler)
        {
            lock (s_mutex)
            {
                Message += handler;
            }
        }

        public void RemoveListener(MessageHandler handler)
        {
            lock (s_mutex)
            {
                Message -= handler;
            }
        }

        protected override void Append(log4net.Core.LoggingEvent loggingEvent)        
        {
            lock (s_mutex)
            {
                if (Message != null)
                {
                    string message = RenderLoggingEvent(loggingEvent);
                    Message(message);
                }
            }
        }
    }
}
