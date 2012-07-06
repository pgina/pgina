/*
	Copyright (c) 2011, pGina Team
	All rights reserved.

	Redistribution and use in source and binary forms, with or without
	modification, are permitted provided that the following conditions are met:
		* Redistributions of source code must retain the above copyright
		  notice, this list of conditions and the following disclaimer.
		* Redistributions in binary form must reproduce the above copyright
		  notice, this list of conditions and the following disclaimer in the
		  documentation and/or other materials provided with the distribution.
		* Neither the name of the pGina Team nor the names of its contributors 
		  may be used to endorse or promote products derived from this software without 
		  specific prior written permission.

	THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
	ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
	WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
	DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY
	DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
	(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
	LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
	ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
	(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
	SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using MySql.Data.MySqlClient;

namespace pGina.Plugin.MySqlLogger
{
    public partial class Configuration : Form
    {
        public Configuration()
        {
            InitializeComponent();
            InitUI();
        }

        private void InitUI()
        {
            this.sessionModeCB.Checked = Settings.Store.SessionMode;
            this.eventModeCB.Checked = Settings.Store.EventMode;

            string host = Settings.Store.Host;
            this.hostTB.Text = host;
            string port = string.Format("{0}", Settings.Store.Port); //Might be stored as an int
            this.portTB.Text = port;
            string db = Settings.Store.Database;
            this.dbTB.Text = db;

            string sessionTable = Settings.Store.SessionTable;
            this.sessionTableTB.Text = sessionTable;
            string eventTable = Settings.Store.EventTable;
            this.eventTableTB.Text = eventTable;
            string user = Settings.Store.User;
            this.userTB.Text = user;
            string pass = Settings.Store.GetEncryptedSetting("Password");
            this.passwdTB.Text = pass;

            bool setting = Settings.Store.EvtLogon;
            this.logonEvtCB.Checked = setting;
            setting = Settings.Store.EvtLogoff;
            this.logoffEvtCB.Checked = setting;
            setting = Settings.Store.EvtLock;
            this.lockEvtCB.Checked = setting;
            setting = Settings.Store.EvtUnlock;
            this.unlockEvtCB.Checked = setting;
            setting = Settings.Store.EvtConsoleConnect;
            this.consoleConnectEvtCB.Checked = setting;
            setting = Settings.Store.EvtConsoleDisconnect;
            this.consoleDisconnectEvtCB.Checked = setting;
            setting = Settings.Store.EvtRemoteControl;
            this.remoteControlEvtCB.Checked = setting;
            setting = Settings.Store.EvtRemoteConnect;
            this.remoteConnectEvtCB.Checked = setting;
            setting = Settings.Store.EvtRemoteDisconnect;
            this.remoteDisconnectEvtCB.Checked = setting;

            this.useModNameCB.Checked = Settings.Store.UseModifiedName;

            updateUIOnModeChange();
        }

        private void okBtn_Click(object sender, EventArgs e)
        {
            if (Save())
            {
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
            }
        }

        private void cancelBtn_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private bool Save()
        {
            try
            {
                int port = Convert.ToInt32((String)this.portTB.Text.Trim());
                Settings.Store.Port = this.portTB.Text.Trim();
            }
            catch (FormatException)
            {
                MessageBox.Show("Invalid port number.");
                return false;
            }

            if (sessionModeCB.Checked && eventModeCB.Checked
                && sessionTableTB.Text.Trim() == eventTableTB.Text.Trim())
            {
                MessageBox.Show("The Event Table must be different from the Session Table.");
                return false;
            }

            Settings.Store.SessionMode = sessionModeCB.Checked;
            Settings.Store.EventMode = eventModeCB.Checked;

            Settings.Store.Host = this.hostTB.Text.Trim();
            Settings.Store.Database = this.dbTB.Text.Trim();
            Settings.Store.EventTable = this.eventTableTB.Text.Trim();
            Settings.Store.SessionTable = this.sessionTableTB.Text.Trim();
            Settings.Store.User = this.userTB.Text.Trim();
            Settings.Store.SetEncryptedSetting("Password", this.passwdTB.Text);

            Settings.Store.EvtLogon = this.logonEvtCB.Checked;
            Settings.Store.EvtLogoff = this.logoffEvtCB.Checked;
            Settings.Store.EvtLock = this.lockEvtCB.Checked;
            Settings.Store.EvtUnlock = this.unlockEvtCB.Checked;
            Settings.Store.EvtConsoleConnect = this.consoleConnectEvtCB.Checked;
            Settings.Store.EvtConsoleDisconnect = this.consoleDisconnectEvtCB.Checked;
            Settings.Store.EvtRemoteControl = this.remoteControlEvtCB.Checked;
            Settings.Store.EvtRemoteConnect = this.remoteConnectEvtCB.Checked;
            Settings.Store.EvtRemoteDisconnect = this.remoteDisconnectEvtCB.Checked;

            Settings.Store.UseModifiedName = this.useModNameCB.Checked;

            return true;
        }

        private void testButton_Click(object sender, EventArgs e)
        {
            if (!Save()) //Will pop up a message box with appropriate error.
                return;
            try
            {
                string sessionModeMsg = null;
                string eventModeMsg = null;
                
                if ((bool)Settings.Store.SessionMode)
                {
                    ILoggerMode mode = LoggerModeFactory.getLoggerMode(LoggerMode.SESSION);
                    sessionModeMsg = mode.TestTable();
                }

                if ((bool)Settings.Store.EventMode)
                {
                    ILoggerMode mode = LoggerModeFactory.getLoggerMode(LoggerMode.EVENT);
                    eventModeMsg = mode.TestTable();
                }

                //Show one or both messages
                if (sessionModeMsg != null && eventModeMsg != null)
                {
                    MessageBox.Show(String.Format("Event Mode Table: {0}\nSession Mode Table: {1}", eventModeMsg, sessionModeMsg));
                } 
                else
                {
                    MessageBox.Show(sessionModeMsg ?? eventModeMsg);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("The following error occurred: {0}", ex.Message));
            }

            //Since the server info may change, close the connection
            LoggerModeFactory.closeConnection();
        }

        private void createTableBtn_Click(object sender, EventArgs e)
        {
            if (!Save())
                return;
            try
            {
                string sessionModeMsg = null;
                string eventModeMsg = null;
                
                if ((bool)Settings.Store.SessionMode)
                {
                    ILoggerMode mode = LoggerModeFactory.getLoggerMode(LoggerMode.SESSION);
                    sessionModeMsg = mode.CreateTable();
                }

                if ((bool)Settings.Store.EventMode)
                {
                    ILoggerMode mode = LoggerModeFactory.getLoggerMode(LoggerMode.EVENT);
                    eventModeMsg = mode.CreateTable();
                }

                //Show one or both messages
                if (sessionModeMsg != null && eventModeMsg != null)
                {
                    MessageBox.Show(String.Format("Event Mode Table: {0}\nSession Mode Table: {1}", eventModeMsg, sessionModeMsg));
                } 
                else
                {
                    MessageBox.Show(sessionModeMsg ?? eventModeMsg);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("The following error occurred: {0}", ex.Message);
            }

            //Since the server info may change, close the current connection
            LoggerModeFactory.closeConnection();

        }

        private void updateUIOnModeChange()
        {   //Enables/disables the events box based on the mode selected.
            eventsBox.Enabled = eventModeCB.Checked;
            eventTableTB.Enabled = eventModeCB.Checked;
            sessionTableTB.Enabled = sessionModeCB.Checked;
        }

        private void showPassCB_CheckedChanged(object sender, EventArgs e)
        {
            this.passwdTB.UseSystemPasswordChar = !this.showPassCB.Checked;
        }

        private void ModeChange(object sender, EventArgs e)
        {
            updateUIOnModeChange();
        }



    }
}
