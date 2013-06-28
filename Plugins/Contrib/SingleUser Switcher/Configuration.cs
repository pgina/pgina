/*
    Written by Florian Rohmer (2013)
     
    Distribued under the pGina license.
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

using pGina.Shared.Settings;
using log4net;

namespace pGina.Plugin.SingleUserSwitcher
{
    public partial class Configuration : Form
    {

        private ILog m_logger = LogManager.GetLogger("SingleUserSwitcher_Configuration");


        public Configuration()
        {
            InitializeComponent();
            SettingsToUi();

            // making the windows form unresizable
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
        }

        public void SettingsToUi()
        {
            m_txtUser.Text = Settings.Store.Username;
            m_txtDomain.Text = Settings.Store.Domain;
            m_txtPass.Text = Settings.Store.GetEncryptedSetting("Password", null);
            substituteCB.Checked = Settings.Store.RequirePlugins;
            allRB.Checked = Settings.Store.RequireAllPlugins;
            anyRB.Checked = !allRB.Checked;
            string[] plugins = Settings.Store.RequiredPluginList;

            foreach (string uuid in plugins)
            {
                m_dgv.Rows.Add(new string[] { uuid });
            }

            maskUI();

            // session list
            List<Session> sessionLst = Session.GetSessions();
            foreach (Session sess in sessionLst)
                this.sessionsList.Items.Add(sess);
        

  
        }

        public bool SaveSettings()
        {

            /* storing default session information */
            Settings.Store.Username = m_txtUser.Text;
            Settings.Store.Domain = m_txtDomain.Text;
            Settings.Store.SetEncryptedSetting("Password", m_txtPass.Text, null);
            Settings.Store.RequirePlugins = substituteCB.Checked;
            Settings.Store.RequireAllPlugins = allRB.Checked;
            

            List<string> uuids = new List<string>();
            foreach (DataGridViewRow row in m_dgv.Rows)
            {
                if (row.Cells[0].Value != null)
                    uuids.Add((string) row.Cells[0].Value);
            }

            Settings.Store.RequiredPluginList = uuids.ToArray();

            // sessions
            List<Session> sessionList = new List<Session>();
            foreach (Object item in this.sessionsList.Items)
            {
                sessionList.Add(item as Session);
                m_logger.DebugFormat("Saving session : {0}", item); 
            }
            Session.SaveSessions(sessionList);

            return true;
        }



        private void btnOk_Click(object sender, EventArgs e)
        {
            if (SaveSettings())
            {
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void requirePluginCheckChange(object sender, EventArgs e)
        {
            maskUI();
        }

        private void maskUI()
        {
            anyRB.Enabled = substituteCB.Checked;
            allRB.Enabled = substituteCB.Checked;
            m_dgv.Enabled = substituteCB.Checked;
        }

        private void addSession_Click(object sender, EventArgs e)
        {
            string ldapgroup = ldapGroup.Text.Trim();
            string username = altUsername.Text.Trim();
            string password = altPassword.Text.Trim();

            if (string.IsNullOrEmpty(ldapgroup))
            {
                MessageBox.Show("Please enter a group name");
                return;
            }

            if (string.IsNullOrEmpty(username))
            {
                MessageBox.Show("Please enter a username");
                return;
            }

            this.sessionsList.Items.Add(new Session(ldapgroup, username, password));

            // making textboxes empty
            ldapGroup.Text = "";
            altUsername.Text = "";
            altPassword.Text = "";

        }

        private void sessionDeleteButton_Click(object sender, EventArgs e)
        {
            int idx = this.sessionsList.SelectedIndex;
            if (idx >= 0 && idx < this.sessionsList.Items.Count)
                this.sessionsList.Items.RemoveAt(idx);
        }
    }
}
