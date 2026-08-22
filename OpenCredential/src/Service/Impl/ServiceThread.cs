using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceProcess;
using System.Threading;

namespace OpenCredential.Service.Impl
{
    public class ServiceThread
    {
        private OpenCredential.Service.Impl.Service m_service = null;
        private readonly ManualResetEventSlim m_initialized = new ManualResetEventSlim(false);
        private Exception m_startupException = null;

        public ServiceThread() {    }

        public Exception StartupException
        {
            get { return m_startupException; }
        }

        public bool WaitForInitialization(int millisecondsTimeout)
        {
            return m_initialized.Wait(millisecondsTimeout);
        }

        public void Start()
        {
            try
            {
                m_service = new OpenCredential.Service.Impl.Service();
                m_service.Start();
            }
            catch (Exception ex)
            {
                m_startupException = ex;
            }
            finally
            {
                m_initialized.Set();
            }
        }

        public void Stop()
        {
            if (m_service != null)
                m_service.Stop();
        }

        public void SessionChange(SessionChangeDescription desc)
        {
            if (m_service != null)
                m_service.SessionChange(desc);
        }
    }
}
