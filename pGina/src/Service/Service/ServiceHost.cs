using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

using pGina.Service.Impl;

using log4net;

namespace Service
{
    public partial class pGinaServiceHost : ServiceBase
    {
        pGina.Service.Impl.Service m_service = null;
        ILog m_logger = LogManager.GetLogger("pGina.Service.ServiceHost");

        public pGinaServiceHost()
        {
            InitializeComponent();

            m_service = new pGina.Service.Impl.Service();
            m_logger.DebugFormat("Service created, using plugin directories: ");
            foreach (string dir in m_service.PluginDirectories)
                m_logger.DebugFormat("  {0}", dir);            
        }

        protected override void OnStart(string[] args)
        {
            m_service.Start();
        }

        protected override void OnStop()
        {
            m_service.Stop();
        }

        protected override void OnSessionChange(SessionChangeDescription changeDescription)
        {
            base.OnSessionChange(changeDescription);
        }
    }
}
