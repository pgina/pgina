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

namespace OpenCredential.Plugin.DatabaseLogger
{
    public partial class Configuration : Form
    {
        private ComboBox m_providerCB;
        private Label m_providerLabel;
        private CheckBox m_presenceTrackingEnabledCB;
        private Label m_heartbeatIntervalLabel;
        private TextBox m_heartbeatIntervalTB;
        private Label m_presenceStatePathLabel;
        private TextBox m_presenceStatePathTB;
        private bool m_hasStoredPassword;

        public Configuration()
        {
            InitializeComponent();
            InitializeProviderControls();
            InitializePresenceControls();
            InitUI();
        }

        private void InitializeProviderControls()
        {
            m_providerLabel = new Label();
            m_providerLabel.AutoSize = true;
            m_providerLabel.Location = new Point(206, 48);
            m_providerLabel.Name = "providerLabel";
            m_providerLabel.Size = new Size(49, 13);
            m_providerLabel.Text = "Provider:";

            m_providerCB = new ComboBox();
            m_providerCB.DropDownStyle = ComboBoxStyle.DropDownList;
            m_providerCB.FormattingEnabled = true;
            m_providerCB.Location = new Point(260, 45);
            m_providerCB.Name = "providerCB";
            m_providerCB.Size = new Size(139, 21);
            m_providerCB.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            m_providerCB.Items.Add(Settings.DatabaseProvider.MySql.ToString());
            m_providerCB.Items.Add(Settings.DatabaseProvider.PostgreSql.ToString());

            this.groupBox2.Controls.Add(m_providerLabel);
            this.groupBox2.Controls.Add(m_providerCB);
            m_providerLabel.BringToFront();
            m_providerCB.BringToFront();
        }

        private void InitializePresenceControls()
        {
            m_presenceTrackingEnabledCB = new CheckBox();
            m_presenceTrackingEnabledCB.AutoSize = true;
            m_presenceTrackingEnabledCB.Location = new Point(7, 100);
            m_presenceTrackingEnabledCB.Name = "presenceTrackingEnabledCB";
            m_presenceTrackingEnabledCB.Size = new Size(163, 17);
            m_presenceTrackingEnabledCB.Text = "Enable session presence tracking";
            m_presenceTrackingEnabledCB.UseVisualStyleBackColor = true;
            m_presenceTrackingEnabledCB.CheckedChanged += new EventHandler(this.ModeChange);

            m_heartbeatIntervalLabel = new Label();
            m_heartbeatIntervalLabel.AutoSize = true;
            m_heartbeatIntervalLabel.Location = new Point(6, 126);
            m_heartbeatIntervalLabel.Name = "heartbeatIntervalLabel";
            m_heartbeatIntervalLabel.Size = new Size(116, 13);
            m_heartbeatIntervalLabel.Text = "Heartbeat every (secs):";

            m_heartbeatIntervalTB = new TextBox();
            m_heartbeatIntervalTB.Location = new Point(128, 123);
            m_heartbeatIntervalTB.Name = "heartbeatIntervalTB";
            m_heartbeatIntervalTB.Size = new Size(45, 20);

            m_presenceStatePathLabel = new Label();
            m_presenceStatePathLabel.AutoSize = true;
            m_presenceStatePathLabel.Location = new Point(6, 152);
            m_presenceStatePathLabel.Name = "presenceStatePathLabel";
            m_presenceStatePathLabel.Size = new Size(52, 13);
            m_presenceStatePathLabel.Text = "State file:";

            m_presenceStatePathTB = new TextBox();
            m_presenceStatePathTB.Location = new Point(73, 149);
            m_presenceStatePathTB.Name = "presenceStatePathTB";
            m_presenceStatePathTB.Size = new Size(415, 20);

            this.optionsBox.Controls.Add(m_presenceTrackingEnabledCB);
            this.optionsBox.Controls.Add(m_heartbeatIntervalLabel);
            this.optionsBox.Controls.Add(m_heartbeatIntervalTB);
            this.optionsBox.Controls.Add(m_presenceStatePathLabel);
            this.optionsBox.Controls.Add(m_presenceStatePathTB);

            this.optionsBox.Height = 180;
            this.testButton.Top += 65;
            this.createTableBtn.Top += 65;
            this.cancelBtn.Top += 65;
            this.okBtn.Top += 65;
            this.ClientSize = new Size(this.ClientSize.Width, this.ClientSize.Height + 65);
        }

        private void InitUI()
        {
            this.sessionModeCB.Checked = Settings.GetSessionMode();
            this.eventModeCB.Checked = Settings.GetEventMode();

            string host = Convert.ToString(Settings.Store.Host);
            this.hostTB.Text = host;
            string port = Convert.ToString(Settings.GetPort());
            this.portTB.Text = port;
            m_providerCB.SelectedItem = Settings.GetDatabaseProvider().ToString();
            string db = Convert.ToString(Settings.Store.Database);
            this.dbTB.Text = db;

            string sessionTable = Convert.ToString(Settings.Store.SessionTable);
            this.sessionTableTB.Text = sessionTable;
            string eventTable = Convert.ToString(Settings.Store.EventTable);
            this.eventTableTB.Text = eventTable;
            string user = Convert.ToString(Settings.Store.User);
            this.userTB.Text = user;
            string pass = Settings.Store.GetEncryptedSetting("Password");
            m_hasStoredPassword = !string.IsNullOrEmpty(pass);
            this.passwdTB.Text = string.Empty;
            this.passwdTB.UseSystemPasswordChar = true;
            this.showPassCB.Checked = false;
            this.showPassCB.Visible = false;

            bool setting = Settings.GetEvtLogon();
            this.logonEvtCB.Checked = setting;
            setting = Settings.GetEvtLogoff();
            this.logoffEvtCB.Checked = setting;
            setting = Settings.GetEvtLock();
            this.lockEvtCB.Checked = setting;
            setting = Settings.GetEvtUnlock();
            this.unlockEvtCB.Checked = setting;
            setting = Settings.GetEvtConsoleConnect();
            this.consoleConnectEvtCB.Checked = setting;
            setting = Settings.GetEvtConsoleDisconnect();
            this.consoleDisconnectEvtCB.Checked = setting;
            setting = Settings.GetEvtRemoteControl();
            this.remoteControlEvtCB.Checked = setting;
            setting = Settings.GetEvtRemoteConnect();
            this.remoteConnectEvtCB.Checked = setting;
            setting = Settings.GetEvtRemoteDisconnect();
            this.remoteDisconnectEvtCB.Checked = setting;

            this.useModNameCB.Checked = Settings.GetUseModifiedName();
            this.offlineQueueEnabledCB.Checked = Settings.IsOfflineQueueEnabled();
            this.healthCheckTB.Text = Convert.ToString(Settings.GetHealthCheckSeconds());
            this.flushBatchTB.Text = Convert.ToString(Settings.GetFlushBatchSize());
            this.offlineQueuePathTB.Text = Settings.GetOfflineQueuePath();
            this.m_presenceTrackingEnabledCB.Checked = Settings.IsPresenceTrackingEnabled();
            this.m_heartbeatIntervalTB.Text = Convert.ToString(Settings.GetHeartbeatIntervalSeconds());
            this.m_presenceStatePathTB.Text = Settings.GetPresenceStatePath();

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
            int healthCheckSeconds = 0;
            int flushBatchSize = 0;
            int heartbeatIntervalSeconds = 0;
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

            try
            {
                healthCheckSeconds = Convert.ToInt32(this.healthCheckTB.Text.Trim());
                flushBatchSize = Convert.ToInt32(this.flushBatchTB.Text.Trim());
                heartbeatIntervalSeconds = Convert.ToInt32(this.m_heartbeatIntervalTB.Text.Trim());
            }
            catch (FormatException)
            {
                MessageBox.Show("Heartbeat, health check and flush batch must be positive integers.");
                return false;
            }

            if (healthCheckSeconds < 5)
            {
                MessageBox.Show("Health check must be at least 5 seconds.");
                return false;
            }

            if (flushBatchSize < 1)
            {
                MessageBox.Show("Flush batch must be at least 1.");
                return false;
            }

            if (heartbeatIntervalSeconds < 15)
            {
                MessageBox.Show("Heartbeat interval must be at least 15 seconds.");
                return false;
            }

            if (sessionModeCB.Checked && eventModeCB.Checked
                && sessionTableTB.Text.Trim() == eventTableTB.Text.Trim())
            {
                MessageBox.Show("The Event Table must be different from the Session Table.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(this.passwdTB.Text) && !m_hasStoredPassword)
            {
                MessageBox.Show("Please enter a database password.");
                return false;
            }

            Settings.DatabaseProvider provider;
            if (!Enum.TryParse(Convert.ToString(m_providerCB.SelectedItem), out provider))
            {
                MessageBox.Show("Please select a valid database provider.");
                return false;
            }

            Settings.Store.SessionMode = sessionModeCB.Checked;
            Settings.Store.EventMode = eventModeCB.Checked;
            Settings.Store.DatabaseProvider = (int)provider;

            Settings.Store.Host = this.hostTB.Text.Trim();
            Settings.Store.Database = this.dbTB.Text.Trim();
            Settings.Store.EventTable = this.eventTableTB.Text.Trim();
            Settings.Store.SessionTable = this.sessionTableTB.Text.Trim();
            Settings.Store.User = this.userTB.Text.Trim();
            string newPassword = this.passwdTB.Text;
            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                Settings.Store.SetEncryptedSetting("Password", newPassword);
                m_hasStoredPassword = true;
                this.passwdTB.Text = string.Empty;
            }

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
            Settings.Store.OfflineQueueEnabled = this.offlineQueueEnabledCB.Checked;
            Settings.Store.PresenceTrackingEnabled = this.m_presenceTrackingEnabledCB.Checked;
            Settings.Store.HeartbeatIntervalSeconds = heartbeatIntervalSeconds;
            Settings.Store.HealthCheckSeconds = healthCheckSeconds;
            Settings.Store.FlushBatchSize = flushBatchSize;
            Settings.Store.OfflineQueuePath = this.offlineQueuePathTB.Text.Trim();
            Settings.Store.PresenceStatePath = this.m_presenceStatePathTB.Text.Trim();

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
                
                if (Settings.GetSessionMode())
                {
                    ILoggerMode mode = LoggerModeFactory.getLoggerMode(LoggerMode.SESSION);
                    sessionModeMsg = mode.TestTable();
                }

                if (Settings.GetEventMode())
                {
                    ILoggerMode mode = LoggerModeFactory.getLoggerMode(LoggerMode.EVENT);
                    eventModeMsg = mode.TestTable();
                }

                //Show one or both messages
                if (sessionModeMsg != null && eventModeMsg != null)
                {
                    MessageBox.Show(
                        String.Format(
                            "Event Mode Table: {0}\nSession Mode Table: {1}\n\n{2}",
                            eventModeMsg,
                            sessionModeMsg,
                            OfflineLogQueue.TestConfiguration()));
                } 
                else
                {
                    MessageBox.Show((sessionModeMsg ?? eventModeMsg) + "\n\n" + OfflineLogQueue.TestConfiguration());
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
                
                if (Settings.GetSessionMode())
                {
                    ILoggerMode mode = LoggerModeFactory.getLoggerMode(LoggerMode.SESSION);
                    sessionModeMsg = mode.CreateTable();
                }

                if (Settings.GetEventMode())
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
                MessageBox.Show(string.Format("The following error occurred: {0}", ex.Message));
            }

            //Since the server info may change, close the current connection
            LoggerModeFactory.closeConnection();

        }

        private void updateUIOnModeChange()
        {   //Enables/disables the events box based on the mode selected.
            eventsBox.Enabled = eventModeCB.Checked;
            eventTableTB.Enabled = eventModeCB.Checked;
            sessionTableTB.Enabled = sessionModeCB.Checked;
            m_presenceTrackingEnabledCB.Enabled = sessionModeCB.Checked;
            m_heartbeatIntervalLabel.Enabled = sessionModeCB.Checked && m_presenceTrackingEnabledCB.Checked;
            m_heartbeatIntervalTB.Enabled = sessionModeCB.Checked && m_presenceTrackingEnabledCB.Checked;
            m_presenceStatePathLabel.Enabled = sessionModeCB.Checked && m_presenceTrackingEnabledCB.Checked;
            m_presenceStatePathTB.Enabled = sessionModeCB.Checked && m_presenceTrackingEnabledCB.Checked;
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

