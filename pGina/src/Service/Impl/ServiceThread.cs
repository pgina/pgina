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

        public void SessionChange(SessionChangeDescription desc)
        {
            m_service.SessionChange(desc);
        }
    }
}
