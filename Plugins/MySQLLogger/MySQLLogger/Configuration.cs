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
            string host = Settings.Store.Host;
            this.hostTB.Text = host;
            int port = Settings.Store.Port;
            this.portTB.Text = Convert.ToString(port);
            string db = Settings.Store.Database;
            this.dbTB.Text = db;
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
                int port = Convert.ToInt32(this.portTB.Text);
                Settings.Store.Port = port;
            }
            catch (FormatException)
            {
                MessageBox.Show("Invalid port number.");
                return false;
            }

            Settings.Store.Host = this.hostTB.Text.Trim();
            Settings.Store.Database = this.dbTB.Text.Trim();
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
            return true;
        }

        private void testButton_Click(object sender, EventArgs e)
        {
            string connStr = this.BuildConnectionString();
            if (connStr == null) return;
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand("SHOW TABLES", conn);
                    bool tableExists = false;
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        if (Convert.ToString(rdr[0]) == PluginImpl.TABLE_NAME)
                            tableExists = true;
                    }
                    rdr.Close();
                    if (tableExists)
                        MessageBox.Show("Connection successful and table exists.");
                    else
                    {
                        string message = "Connection was successful, but no table exists.  Click \"Create Table\" to create the required table.";
                        MessageBox.Show(message);
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(String.Format("Connection failed: {0}", ex.Message));
            }
        }

        private void createTableBtn_Click(object sender, EventArgs e)
        {
            string connStr = this.BuildConnectionString();
            if (connStr == null) return;
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    string sql = string.Format(
                        "CREATE TABLE {0} (" +
                        "   TimeStamp DATETIME, " +
                        "   Host TINYTEXT, " +
                        "   Ip VARCHAR(15), " +
                        "   Machine TINYTEXT, " +
                        "   Message TEXT )", PluginImpl.TABLE_NAME);
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.ExecuteNonQuery();
                    
                    MessageBox.Show("Table created.");
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(String.Format("Error: {0}", ex.Message));
            }
        }

        private void showPassCB_CheckedChanged(object sender, EventArgs e)
        {
            this.passwdTB.UseSystemPasswordChar = !this.showPassCB.Checked;
        }

        private string BuildConnectionString()
        {

            uint port = 0;
            try
            {
                port = Convert.ToUInt32(this.portTB.Text);
            }
            catch (FormatException)
            {
                MessageBox.Show("Invalid port number.");
                return null;
            }

            MySqlConnectionStringBuilder bldr = new MySqlConnectionStringBuilder();
            bldr.Server = this.hostTB.Text.Trim();
            bldr.Port = port;
            bldr.UserID = this.userTB.Text.Trim();
            bldr.Database = this.dbTB.Text.Trim();
            bldr.Password = this.passwdTB.Text;

            return bldr.GetConnectionString(true);
        }

    }
}
