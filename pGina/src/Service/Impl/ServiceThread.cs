using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceProcess;

namespace pGina.Service.Impl
{
    public class ServiceThread
    {
        private pGina.Service.Impl.Service m_service = null;

        public ServiceThread() {    }

        public void Start()
        {
            m_service = new pGina.Service.Impl.Service();
            m_service.Start();
        }

        public void Stop()
        {
            m_service.Stop();
        }

        public Boolean OnCustomCommand()
        {
            return m_service.OnCustomCommand();
        }

        public void SessionChange(int sessionID, SessionChangeReason evnt)
        {
            m_service.SessionChange(sessionID, evnt);
        }
    }
}
