/*
	Copyright (c) 2014, pGina Team
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
using System.Web.Script.Serialization;

namespace pGina.Plugin.MultiEmail
{
    public partial class Configuration : Form
    {
        private static string popSslPort = "995";
        private static string popPort = "110";
        private static string imapPort = "143";
        private static string imapSslPort = "993";

        private string[] m_protocolOptions;
        private BindingList<ServersList> m_servers;
        private JavaScriptSerializer m_jsonSerializer = new JavaScriptSerializer();

        public Configuration()
        {
            InitializeComponent();
            InitUi();
            LoadSettings();
        }

        private void InitUi()
        {
            m_protocolOptions = new string[]
            {
                ServersList.ProtocolOption.POP3.ToString(),
                ServersList.ProtocolOption.IMAP.ToString()
            };
            protocolComboBox.Items.AddRange(m_protocolOptions);
            protocolComboBox.SelectedIndex = 1;

            DisableAndClearAllEditFields();

            m_serversListDgv.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "Protocol",
                DataPropertyName = "Protocol",
                HeaderText = "Protocol",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                ReadOnly = true
            });
            m_serversListDgv.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "Server",
                DataPropertyName = "Server",
                HeaderText = "Server",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ReadOnly = true
            });
            m_serversListDgv.Columns.Add(new DataGridViewCheckBoxColumn()
            {
                Name = "UseSsl",
                DataPropertyName = "UseSsl",
                HeaderText = "SSL",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                ReadOnly = true
            });
            m_serversListDgv.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "Port",
                DataPropertyName = "Port",
                HeaderText = "Port",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                ReadOnly = true
            });
            m_serversListDgv.Columns.Add(new DataGridViewCheckBoxColumn()
            {
                Name = "AppendDomain",
                DataPropertyName = "AppendDomain",
                HeaderText = "Append",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                ReadOnly = true
            });
            m_serversListDgv.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "Domain",
                DataPropertyName = "Domain",
                HeaderText = "Domain",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                ReadOnly = true
            });
            m_serversListDgv.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "Timeout",
                DataPropertyName = "Timeout",
                HeaderText = "Timeout",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                ReadOnly = true
            });

            m_serversListDgv.RowValidating += m_serversListDgv_RowValidating;
            m_serversListDgv.SelectionChanged += m_serversListDgv_SelectionChanged;
            protocolComboBox.LostFocus += protocolComboBox_LostFocus;
            serverTextBox.LostFocus += serverTextBox_LostFocus;
            portTextBox.LostFocus += portTextBox_LostFocus;
            domainTextBox.LostFocus += domainTextBox_LostFocus;
            timeoutTextBox.LostFocus += timeoutTextBox_LostFocus;
            portTextBox.Validating += portTextBox_Validating;

        }

        private void EnableEditFields()
        {
            protocolLabel.Enabled = true;
            protocolComboBox.Enabled = true;
            serverLabel.Enabled = true;
            serverTextBox.Enabled = true;
            portLabel.Enabled = true;
            portTextBox.Enabled = true;
            sslCheckBox.Enabled = true;
            appendDomainCheckBox.Enabled = true;
            if (appendDomainCheckBox.Checked == true)
            {
                domainLabel.Enabled = true;
                domainTextBox.Enabled = true;
            }
            else
            {
                domainLabel.Enabled = false;
                domainTextBox.Enabled = false;
            }
            timeoutLabel.Enabled = true;
            timeoutTextBox.Enabled = true;
        }

        private void SetEditFields(ServersList server)
        {
            protocolComboBox.SelectedItem = server.Protocol.ToString();
            serverTextBox.Text = server.Server;
            portTextBox.Text = server.Port;
            if (server.UseSsl == true)
                sslCheckBox.Checked = true;
            else
                sslCheckBox.Checked = false;
            if (server.AppendDomain == false)
                appendDomainCheckBox.Checked = false;
            else
                appendDomainCheckBox.Checked = true;
            domainTextBox.Text = server.Domain;
            timeoutTextBox.Text = server.Timeout;
        }

        private void DisableAndClearAllEditFields()
        {
            protocolLabel.Enabled = false;
            protocolComboBox.Enabled = false;
            serverLabel.Enabled = false;
            serverTextBox.Enabled = false;
            serverTextBox.Text = "";
            portLabel.Enabled = false;
            portTextBox.Enabled = false;
            sslCheckBox.Enabled = false;
            appendDomainCheckBox.Enabled = false;
            domainLabel.Enabled = false;
            domainTextBox.Enabled = false;
            timeoutLabel.Enabled = false;
            timeoutTextBox.Enabled = false;
        }

        private void LoadSettings()
        {
            string jsonData = Settings.Store.Servers.ToString();
            List<ServersList> servers = new List<ServersList>();
            if (!String.IsNullOrEmpty(jsonData))
            {
                servers = m_jsonSerializer.Deserialize<List<ServersList>>(jsonData);
            }
            m_servers = new BindingList<ServersList>(servers);
            m_serversListDgv.DataSource = m_servers;
        }

        void protocolComboBox_LostFocus(object sender, EventArgs e)
        {
            if (m_serversListDgv.SelectedRows.Count > 0)
            {
                DataGridViewRow row = m_serversListDgv.SelectedRows[0];
                int idx = row.Index;
                ServersList server = (ServersList)row.DataBoundItem;
                server.Protocol = (ServersList.ProtocolOption)Enum.Parse(typeof(ServersList.ProtocolOption), protocolComboBox.SelectedItem.ToString());
                m_servers.ResetItem(idx);
            }
        }

        void serverTextBox_LostFocus(object sender, EventArgs e)
        {
            if (m_serversListDgv.SelectedRows.Count > 0)
            {
                DataGridViewRow row = m_serversListDgv.SelectedRows[0];
                int idx = row.Index;
                ServersList server = (ServersList)row.DataBoundItem;
                server.Server = serverTextBox.Text;
                m_servers.ResetItem(idx);
            }
        }

        void portTextBox_LostFocus(object sender, EventArgs e)
        {
            if (m_serversListDgv.SelectedRows.Count > 0)
            {
                DataGridViewRow row = m_serversListDgv.SelectedRows[0];
                int idx = row.Index;
                ServersList server = (ServersList)row.DataBoundItem;
                server.Port = portTextBox.Text;
                m_servers.ResetItem(idx);
            }
        }

        void domainTextBox_LostFocus(object sender, EventArgs e)
        {
            if (m_serversListDgv.SelectedRows.Count > 0)
            {
                DataGridViewRow row = m_serversListDgv.SelectedRows[0];
                int idx = row.Index;
                ServersList server = (ServersList)row.DataBoundItem;
                server.Domain = domainTextBox.Text;
                m_servers.ResetItem(idx);
            }
        }

        void timeoutTextBox_LostFocus(object sender, EventArgs e)
        {
            if (m_serversListDgv.SelectedRows.Count > 0)
            {
                DataGridViewRow row = m_serversListDgv.SelectedRows[0];
                int idx = row.Index;
                ServersList server = (ServersList)row.DataBoundItem;
                server.Timeout = timeoutTextBox.Text;
                m_servers.ResetItem(idx);
            }
        }

        void m_serversListDgv_SelectionChanged(object sender, EventArgs e)
        {
            int numRowsSelected = m_serversListDgv.SelectedRows.Count;
            if (numRowsSelected == 0)
                DisableAndClearAllEditFields();
            else
            {
                DataGridViewRow row = m_serversListDgv.SelectedRows[0];
                ServersList server = (ServersList)row.DataBoundItem;
                SetEditFields(server);
                EnableEditFields();
            }
        }

        private void m_serversListDgv_RowValidating(object sender, DataGridViewCellCancelEventArgs e)
        {
            // MessageBox.Show(m_serversListDgv.CurrentRow.Index.ToString());
            if (m_serversListDgv.Focused == true)
                if (! validateFields())
                    e.Cancel = true;
        }

        private void protocolComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (protocolComboBox.SelectedItem.ToString() == ServersList.ProtocolOption.POP3.ToString())
                portTextBox.Text = (sslCheckBox.Checked ? popSslPort : popPort);
            else
                portTextBox.Text = (sslCheckBox.Checked ? imapSslPort : imapPort);

            portTextBox_LostFocus(sender, e);
        }

        private void sslCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (protocolComboBox.SelectedItem.ToString() == ServersList.ProtocolOption.POP3.ToString())
                portTextBox.Text = (sslCheckBox.Checked ? popSslPort : popPort);
            else
                portTextBox.Text = (sslCheckBox.Checked ? imapSslPort : imapPort);

            if (m_serversListDgv.SelectedRows.Count > 0)
            {
                DataGridViewRow row = m_serversListDgv.SelectedRows[0];
                int idx = row.Index;
                ServersList server = (ServersList)row.DataBoundItem;
                if (sslCheckBox.Checked == true)
                    server.UseSsl = true;
                else
                    server.UseSsl = false;
                portTextBox_LostFocus(sender,e);
                m_servers.ResetItem(idx);
            }
        }

        private void serverTextBox_TextChanged(object sender, EventArgs e)
        {
            if (m_serversListDgv.SelectedRows.Count > 0 && serverTextBox.Text.Trim().Length == 0)
            {
                MessageBox.Show("Server address can not be blank.");
            }
        }

        private void portTextBox_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                int port = Convert.ToInt32(portTextBox.Text);
                if (port < 0) throw new FormatException();
            }
            catch (FormatException)
            {
                MessageBox.Show("Port must be an integer greater than 0.");
                e.Cancel = true;
            }
        }


        private void domainAppendCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (appendDomainCheckBox.Checked)
            {
                domainLabel.Enabled = true;
                domainTextBox.Enabled = true;
            }
            else
            {
                domainLabel.Enabled = false;
                domainTextBox.Enabled = false;
            }

            if (m_serversListDgv.SelectedRows.Count > 0)
            {
                DataGridViewRow row = m_serversListDgv.SelectedRows[0];
                int idx = row.Index;
                ServersList server = (ServersList)row.DataBoundItem;
                if (appendDomainCheckBox.Checked == true)
                    server.AppendDomain = true;
                else
                    server.AppendDomain = false;
                m_servers.ResetItem(idx);
            }
        }

        private void domainTextBox_TextChanged(object sender, EventArgs e)
        {
            if (appendDomainCheckBox.Checked && domainTextBox.Text.Trim().Length == 0)
            {
                MessageBox.Show("A domain must be entered if \"Append domain to username\" is checked.");
            }
        }

        private void newServerButton_Click(object sender, EventArgs e)
        {
            if (validateFields())
            {
                ServersList server = new ServersList();

                m_servers.Add(server);
                protocolComboBox_SelectedIndexChanged(sender, e);

                m_serversListDgv.Rows[m_serversListDgv.Rows.Count - 1].Selected = true;
                m_serversListDgv.CurrentCell = m_serversListDgv.Rows[m_serversListDgv.Rows.Count - 1].Cells[0];
            }
        }

        private void m_deleteServerButton_Click(object sender, EventArgs e)
        {
            int numRowsSelected = m_serversListDgv.SelectedRows.Count;
            if (numRowsSelected > 0)
            {
                m_servers.RemoveAt(m_serversListDgv.SelectedRows[0].Index);
            }
        }

        private void m_saveCloseButton_Click(object sender, EventArgs e)
        {
            if (validateFields())
            {
                List<ServersList> servers = m_servers.ToList<ServersList>();
                Settings.Store.Servers = m_jsonSerializer.Serialize(servers).ToString();
                this.Close();
            }
        }

        private void m_cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private Boolean validateFields()
        {
            Boolean state = true;
            if (m_serversListDgv.SelectedRows.Count > 0 && (serverTextBox.Text.Trim().Length == 0 || (appendDomainCheckBox.Checked && domainTextBox.Text.Trim().Length == 0)))
            {
                state = false;
                if (serverTextBox.Text.Trim().Length == 0)
                    MessageBox.Show("Server address can not be blank.");
                if (appendDomainCheckBox.Checked && domainTextBox.Text.Trim().Length == 0)
                    MessageBox.Show("A domain must be entered if \"Append domain to username\" is checked.");
            }

            return state;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void settingsChanged(object sender, EventArgs e)
        {
            updateSettings();
        }

        private void updateSettings()
        {
            domainLabel.Enabled = appendDomainCheckBox.Checked;
            domainTextBox.Enabled = appendDomainCheckBox.Checked;
        }

        private void Btn_help(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://mutonufoai.github.io/pgina/documentation/plugins/multiemail.html");
        }
    }
}
