using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ServiceProcess;

using log4net;
using Abstractions.WindowsApi;

namespace Service
{
    public partial class HiddenForm : Form
    {
        private ILog m_logger = LogManager.GetLogger("pGina.Service.HiddenForm");
        private pGina.Service.Impl.ServiceThread m_serviceThreadObj = null;

        public HiddenForm(pGina.Service.Impl.ServiceThread serviceThreadObj)
        {
            m_serviceThreadObj = serviceThreadObj;
            InitializeComponent();
        }

        private void HiddenForm_Load(object sender, EventArgs e)
        {
            if (!Abstractions.WindowsApi.pInvokes.WTSRegister(this.Handle))
            {
                m_logger.ErrorFormat("no session events are available");
            }
        }

        private void HiddenForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Abstractions.WindowsApi.pInvokes.WTSUnRegister(this.Handle);
        }

        protected override void WndProc(ref Message m)
        {
            //m_logger.InfoFormat("WndProc " + m.Msg.ToString());
            switch (m.Msg)
            {
                case (int)Abstractions.WindowsApi.pInvokes.structenums.WM.DEVICECHANGE:
                    //m_logger.InfoFormat("WM_DEVICECHANGE " + m.WParam.ToInt32());
                    /*switch (m.WParam.ToInt32())
                    {
                        case (int)Abstractions.WindowsApi.pInvokes.structenums.DBT.CONFIGCHANGECANCELED:
                            m_logger.InfoFormat("DBT_CONFIGCHANGECANCELED");
                            break;
                        case (int)Abstractions.WindowsApi.pInvokes.structenums.DBT.CONFIGCHANGED:
                            m_logger.InfoFormat("DBT_CONFIGCHANGED");
                            break;
                        case (int)Abstractions.WindowsApi.pInvokes.structenums.DBT.CUSTOMEVENT:
                            m_logger.InfoFormat("DBT_CUSTOMEVENT");
                            break;
                        case (int)Abstractions.WindowsApi.pInvokes.structenums.DBT.DEVICEARRIVAL:
                            m_logger.InfoFormat("DBT_DEVICEARRIVAL");
                            break;
                        case (int)Abstractions.WindowsApi.pInvokes.structenums.DBT.DEVICEQUERYREMOVE:
                            m_logger.InfoFormat("DBT_DEVICEQUERYREMOVE");
                            break;
                        case (int)Abstractions.WindowsApi.pInvokes.structenums.DBT.DEVICEQUERYREMOVEFAILED:
                            m_logger.InfoFormat("DBT_DEVICEQUERYREMOVEFAILED");
                            break;
                        case (int)Abstractions.WindowsApi.pInvokes.structenums.DBT.DEVICEREMOVECOMPLETE:
                            m_logger.InfoFormat("DBT_DEVICEREMOVECOMPLETE");
                            break;
                        case (int)Abstractions.WindowsApi.pInvokes.structenums.DBT.DEVICEREMOVEPENDING:
                            m_logger.InfoFormat("DBT_DEVICEREMOVEPENDING");
                            break;
                        case (int)Abstractions.WindowsApi.pInvokes.structenums.DBT.DEVICETYPESPECIFIC:
                            m_logger.InfoFormat("DBT_DEVICETYPESPECIFIC");
                            break;
                        case (int)Abstractions.WindowsApi.pInvokes.structenums.DBT.DEVNODES_CHANGED:
                            m_logger.InfoFormat("DBT_DEVNODES_CHANGED");
                            break;
                        case (int)Abstractions.WindowsApi.pInvokes.structenums.DBT.QUERYCHANGECONFIG:
                            m_logger.InfoFormat("DBT_QUERYCHANGECONFIG");
                            break;
                        case (int)Abstractions.WindowsApi.pInvokes.structenums.DBT.USERDEFINED:
                            m_logger.InfoFormat("DBT_USERDEFINED");
                            break;
                    }*/
                    break;
                case (int)Abstractions.WindowsApi.pInvokes.structenums.WM.POWERBROADCAST:
                    //m_logger.InfoFormat("WM_POWERBROADCAST " + m.WParam.ToInt32());
                    /*switch (m.WParam.ToInt32())
                    {
                        case (int)Abstractions.WindowsApi.pInvokes.structenums.PBT.APMRESUMEAUTOMATIC:
                            m_logger.InfoFormat("PBT_APMRESUMEAUTOMATIC");
                            break;
                        case (int)Abstractions.WindowsApi.pInvokes.structenums.PBT.APMSUSPEND:
                            m_logger.InfoFormat("PBT_APMSUSPEND");
                            break;
                        case (int)Abstractions.WindowsApi.pInvokes.structenums.PBT.APMRESUMESUSPEND:
                            m_logger.InfoFormat("PBT_APMRESUMESUSPEND");
                            break;
                        case (int)Abstractions.WindowsApi.pInvokes.structenums.PBT.APMRESUMECRITICAL:
                            m_logger.InfoFormat("PBT_APMRESUMECRITICAL");
                            break;
                    }*/
                    break;
                case (int)Abstractions.WindowsApi.pInvokes.structenums.WM.WTSSESSION_CHANGE:
                    //m_logger.InfoFormat("WM_WTSSESSION_CHANGE " + m.WParam.ToInt32());
                    /*switch (m.WParam.ToInt32())
                    {
                        case (int)Abstractions.WindowsApi.pInvokes.structenums.WTS.CONSOLE_CONNECT:
                            m_logger.InfoFormat(String.Format("WTS_repl:{0}", m.LParam.ToInt32()));
                            break;
                        case (int)Abstractions.WindowsApi.pInvokes.structenums.WTS.CONSOLE_DISCONNECT:
                            m_logger.InfoFormat(String.Format("WTS_CONSOLE_DISCONNECT:{0}", m.LParam.ToInt32()));
                            break;
                        case (int)Abstractions.WindowsApi.pInvokes.structenums.WTS.REMOTE_CONNECT:
                            m_logger.InfoFormat(String.Format("WTS_REMOTE_CONNECT:{0}", m.LParam.ToInt32()));
                            break;
                        case (int)Abstractions.WindowsApi.pInvokes.structenums.WTS.REMOTE_DISCONNECT:
                            m_logger.InfoFormat(String.Format("WTS_REMOTE_DISCONNECT:{0}", m.LParam.ToInt32()));
                            break;
                        case (int)Abstractions.WindowsApi.pInvokes.structenums.WTS.SESSION_LOGON:
                            m_logger.InfoFormat(String.Format("WTS_SESSION_LOGON:{0}", m.LParam.ToInt32()));
                            break;
                        case (int)Abstractions.WindowsApi.pInvokes.structenums.WTS.SESSION_LOGOFF:
                            m_logger.InfoFormat(String.Format("WTS_SESSION_LOGOFF:{0}", m.LParam.ToInt32()));
                            break;
                        case (int)Abstractions.WindowsApi.pInvokes.structenums.WTS.SESSION_LOCK:
                            m_logger.InfoFormat(String.Format("WTS_SESSION_LOCK:{0}", m.LParam.ToInt32()));
                            break;
                        case (int)Abstractions.WindowsApi.pInvokes.structenums.WTS.SESSION_UNLOCK:
                            m_logger.InfoFormat(String.Format("WTS_SESSION_UNLOCK:{0}", m.LParam.ToInt32()));
                            break;
                        case (int)Abstractions.WindowsApi.pInvokes.structenums.WTS.SESSION_REMOTE_CONTROL:
                            m_logger.InfoFormat(String.Format("WTS_SESSION_REMOTE_CONTROL:{0}", m.LParam.ToInt32()));
                            break;
                        case (int)Abstractions.WindowsApi.pInvokes.structenums.WTS.SESSION_CREATE:
                            m_logger.InfoFormat(String.Format("WTS_SESSION_CREATE:{0}", m.LParam.ToInt32()));
                            break;
                        case (int)Abstractions.WindowsApi.pInvokes.structenums.WTS.SESSION_TERMINATE:
                            m_logger.InfoFormat(String.Format("WTS_SESSION_TERMINATE:{0}", m.LParam.ToInt32()));
                            break;
                    }*/
                    m_serviceThreadObj.SessionChange(m.LParam.ToInt32(), (SessionChangeReason)m.WParam.ToInt32());
                    break;
            }
            base.WndProc(ref m);
        }
    }
    partial class HiddenForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(0, 0);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "HiddenForm";
            this.Text = "HiddenForm";
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.Load += new System.EventHandler(this.HiddenForm_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.HiddenForm_FormClosing);
            this.ResumeLayout(false);
        }
    }
}
