/*
	Copyright (c) 2017, pGina Team
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
using System.Text.RegularExpressions;
using System.IO;
using System.DirectoryServices.Protocols;

using pGina.Shared.Settings;
using pGina.Plugin.Ldap;

using log4net;

namespace pGina.Plugin.Ldap
{
    public partial class Configuration : Form
    {
        private ILog m_logger = LogManager.GetLogger("Ldap Configuration");

        public Configuration()
        {
            InitializeComponent();

            InitUI();
            LoadSettings();
            UpdateSslElements();
            UpdateAuthenticationElements();
        }

        private void InitUI()
        {
            this.passwordAttributesDGV.RowHeadersVisible = true;
            this.passwordAttributesDGV.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.passwordAttributesDGV.MultiSelect = false;
            this.passwordAttributesDGV.AllowUserToAddRows = true;

            this.passwordAttributesDGV.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "Attribute Name",
                DataPropertyName = "Name",
                HeaderText = "Attribute Name",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ReadOnly = false
            });
            DataGridViewComboBoxColumn combCol = new DataGridViewComboBoxColumn()
            {
                Name = "Method",
                DataPropertyName = "Method",
                HeaderText = "Method",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                Width = 250,
                DisplayMember = "Name",
                ValueMember = "Method",
            };
            this.passwordAttributesDGV.DefaultValuesNeeded += passwordAttributesDGV_DefaultValuesNeeded;
            combCol.Items.AddRange(AttribMethod.methods.Values.ToArray());
            combCol.Items.AddRange(TimeMethod.methods.Values.ToArray());
            this.passwordAttributesDGV.Columns.Add(combCol);
        }

        void passwordAttributesDGV_DefaultValuesNeeded(object sender, DataGridViewRowEventArgs e)
        {
            e.Row.Cells[1].Value = Methods.PLAIN;
        }

        private void LoadSettings()
        {
            string[] ldapHosts = Settings.Store.LdapHost;
            string hosts = "";
            for (int i = 0; i < ldapHosts.Count(); i++)
            {
                string host = ldapHosts[i];
                if (i < ldapHosts.Count() - 1) hosts += host + " ";
                else hosts += host;
            }
            ldapHostTextBox.Text = hosts;

            int port = Settings.Store.LdapPort;
            ldapPortTextBox.Text = Convert.ToString(port);

            int timeout = Settings.Store.LdapTimeout;
            timeoutTextBox.Text = Convert.ToString(timeout);

            bool useSsl = Settings.Store.UseSsl;
            useSslCheckBox.CheckState = useSsl ? CheckState.Checked : CheckState.Unchecked;

            bool useTls = Settings.Store.UseTls;
            useTlsCheckBox.CheckState = useTls ? CheckState.Checked : CheckState.Unchecked;

            bool reqCert = Settings.Store.RequireCert;
            validateServerCertCheckBox.CheckState = reqCert ? CheckState.Checked : CheckState.Unchecked;

            string serverCertFile = Settings.Store.ServerCertFile;
            sslCertFileTextBox.Text = serverCertFile;

            string searchDn = Settings.Store.SearchDN;
            searchDnTextBox.Text = searchDn;

            string searchPw = Settings.Store.GetEncryptedSetting("SearchPW");
            searchPassTextBox.Text = searchPw;

            // Authentication tab
            bool allowEmpty = Settings.Store.AllowEmptyPasswords;
            this.allowEmptyPwCB.Checked = allowEmpty;

            string dnPattern = Settings.Store.DnPattern;
            dnPatternTextBox.Text = dnPattern;

            bool doSearch = Settings.Store.DoSearch;
            searchForDnCheckBox.CheckState = doSearch ? CheckState.Checked : CheckState.Unchecked;

            string filter = Settings.Store.SearchFilter;
            searchFilterTextBox.Text = filter;

            bool useAuth = Settings.Store.UseAuthBindForAuthzAndGateway;
            useAuthBindForAuthzAndGatewayCb.Checked = useAuth;

            string[] searchContexts = Settings.Store.SearchContexts;
            string ctxs = "";
            for (int i = 0; i < searchContexts.Count(); i++)
            {
                string ctx = searchContexts[i];
                if (i < searchContexts.Count() - 1) ctxs += ctx + "\r\n";
                else ctxs += ctx;
            }
            searchContextsTextBox.Text = ctxs;

            // AttribConverter Grid
            string[] AttribConv = Settings.Store.AttribConv;
            Column1.DataSource = AttribConvert.Attribs.ToArray();
            dataGridView1.ColumnCount = 2;
            for (int x = 0; x < AttribConv.Count(); x++)
            {
                string[] split = AttribConv[x].Split('\t');
                if (split.Count() == 2)
                {
                    split[0] = split[0].Trim();
                    split[1] = split[1].Trim();
                    if (!String.IsNullOrEmpty(split[0]) && !String.IsNullOrEmpty(split[1]))
                    {
                        if (AttribConvert.Attribs.Contains(split[0]))
                        //if (Array.Exists(WinValues(), element => element == split[0]))
                        {
                            int index = AttribConvert.Attribs.IndexOf(split[0]);
                            //int index = Array.FindIndex(WinValues(), item => item == split[0]);

                            DataGridViewRow row = new DataGridViewRow();
                            DataGridViewComboBoxCell CellSample = new DataGridViewComboBoxCell();
                            CellSample.DataSource = AttribConvert.Attribs.ToArray(); // list of the string items that I want to insert in ComboBox.
                            CellSample.Value = AttribConvert.Attribs[index]; // default value for the ComboBox
                            row.Cells.Add(CellSample);

                            row.Cells.Add(new DataGridViewTextBoxCell()
                            {
                                Value = split[1]
                            });
                            dataGridView1.Rows.Add(row);
                        }
                    }
                }
            }

            /////////////// Authorization tab /////////////////
            this.authzRuleMemberComboBox.SelectedIndex = 0;
            this.authzRuleActionComboBox.SelectedIndex = 0;
            this.authzRuleScope.SelectedIndex = 0;
            this.authzDefaultAllowRB.Checked = Settings.Store.AuthzDefault;
            this.authzDefaultDenyRB.Checked = !(bool)Settings.Store.AuthzDefault;
            this.authzRequireAuthCB.Checked = Settings.Store.AuthzRequireAuth;
            this.authzAllowOnErrorCB.Checked = Settings.Store.AuthzAllowOnError;

            List<GroupAuthzRule> lst = GroupRuleLoader.GetAuthzRules();
            foreach (GroupAuthzRule rule in lst)
                this.authzRulesListBox.Items.Add(rule);

            ///////////////// Gateway tab /////////////////
            this.gatewayRuleGroupMemberCB.SelectedIndex = 0;
            this.gatewayRuleScope.SelectedIndex = 0;

            List<GroupGatewayRule> gwLst = GroupRuleLoader.GetGatewayRules();
            foreach (GroupGatewayRule rule in gwLst)
                this.gatewayRulesListBox.Items.Add(rule);

            ////////////// Change Password tab ///////////////
            List<AttributeEntry> attribs = CPAttributeSettings.Load();

            foreach (AttributeEntry entry in attribs)
                this.passwordAttributesDGV.Rows.Add( entry.Name, entry.Method );
        }

        private void sslCertFileBrowseButton_Click(object sender, EventArgs e)
        {
            DialogResult result;
            string fileName;
            using( OpenFileDialog dlg = new OpenFileDialog() )
            {
                result = dlg.ShowDialog();
                fileName = dlg.FileName;
            }

            if( result == DialogResult.OK )
            {
                sslCertFileTextBox.Text = fileName;
            }
        }

        private void useSslCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (this.useSslCheckBox.Checked)
                this.useTlsCheckBox.Checked = false;
            UpdateSslElements();
        }

        private void useTlsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (this.useTlsCheckBox.Checked)
                this.useSslCheckBox.Checked = false;
            UpdateSslElements();
        }

        private void UpdateSslElements()
        {
            if (validateServerCertCheckBox.Checked)
            {
                sslCertFileTextBox.Enabled = true;
                sslCertFileBrowseButton.Enabled = true;
            }
            else if (validateServerCertCheckBox.CheckState == CheckState.Unchecked)
            {
                sslCertFileTextBox.Enabled = false;
                sslCertFileBrowseButton.Enabled = false;
            }

            if (!useSslCheckBox.Checked && !useTlsCheckBox.Checked)
            {
                validateServerCertCheckBox.Enabled = false;
                sslCertFileTextBox.Enabled = false;
                sslCertFileBrowseButton.Enabled = false;
            }
            else
            {
                validateServerCertCheckBox.Enabled = true;
            }
        }

        private void UpdateAuthenticationElements()
        {
            if (searchForDnCheckBox.Checked)
            {
                searchFilterTextBox.Enabled = true;
                searchContextsTextBox.Enabled = true;
                dnPatternTextBox.Enabled = false;
            }
            else
            {
                searchFilterTextBox.Enabled = false;
                searchContextsTextBox.Enabled = false;
                dnPatternTextBox.Enabled = true;
            }
        }

        private void validateServerCertCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            UpdateSslElements();
        }

        private void searchForDnCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            UpdateAuthenticationElements();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            if (ValidateInput())
            {
                StoreSettings();
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
            }
        }

        private bool ValidateInput()
        {
            if (ldapHostTextBox.Text.Trim().Length == 0)
            {
                MessageBox.Show("Please provide at least one LDAP host");
                return false;
            }

            try
            {
                int port = Convert.ToInt32(ldapPortTextBox.Text.Trim());
                if (port <= 0) throw new FormatException();
            }
            catch (FormatException)
            {
                MessageBox.Show("The LDAP port number must be a positive integer > 0.");
                return false;
            }

            try
            {
                int timeout = Convert.ToInt32(timeoutTextBox.Text.Trim());
                if (timeout <= 0) throw new FormatException();
            }
            catch (FormatException)
            {
                MessageBox.Show("The timout be a positive integer > 0.");
                return false;
            }

            if (validateServerCertCheckBox.CheckState == CheckState.Checked &&
                sslCertFileTextBox.Text.Trim().Length > 0 &&
                !(File.Exists(sslCertFileTextBox.Text.Trim())))
            {
                MessageBox.Show("SSL certificate file does not exist."
                    + "Please select a valid certificate file.");
                return false;
            }
            // TODO: Make sure that other input is valid.

            return true;
        }

        private void StoreSettings()
        {
            Settings.Store.LdapHost = Regex.Split(ldapHostTextBox.Text.Trim(), @"\s+");
            Settings.Store.LdapPort = Convert.ToInt32(ldapPortTextBox.Text.Trim());
            Settings.Store.LdapTimeout = Convert.ToInt32(timeoutTextBox.Text.Trim());
            Settings.Store.UseSsl = (useSslCheckBox.CheckState == CheckState.Checked);
            Settings.Store.UseTls = (useTlsCheckBox.CheckState == CheckState.Checked);
            Settings.Store.RequireCert = (validateServerCertCheckBox.CheckState == CheckState.Checked);
            Settings.Store.ServerCertFile = sslCertFileTextBox.Text.Trim();
            Settings.Store.UseAuthBindForAuthzAndGateway = (useAuthBindForAuthzAndGatewayCb.CheckState == CheckState.Checked);
            Settings.Store.SearchDN = searchDnTextBox.Text.Trim();
            Settings.Store.SetEncryptedSetting("SearchPW", searchPassTextBox.Text);

            // Authentication
            Settings.Store.AllowEmptyPasswords = this.allowEmptyPwCB.Checked;
            Settings.Store.DnPattern = dnPatternTextBox.Text.Trim();
            Settings.Store.DoSearch = (searchForDnCheckBox.CheckState == CheckState.Checked);
            Settings.Store.SearchFilter = searchFilterTextBox.Text.Trim();
            Settings.Store.SearchContexts = Regex.Split(searchContextsTextBox.Text.Trim(), @"\s*\r?\n\s*");
            Settings.Store.AuthzDefault = this.authzDefaultAllowRB.Checked;

            List<string> AttribConv = new List<string>();
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells[0].Value != null && row.Cells[1].Value != null)
                {
                    AttribConv.Add(row.Cells[0].Value.ToString() + "\t" + row.Cells[1].Value.ToString().Trim());
                }
            }
            if (AttribConv.Count > 0)
                Settings.Store.AttribConv = AttribConv.ToArray();
            else
                Settings.Store.AttribConv = new string[] { };

            // Authorization
            Settings.Store.AuthzRequireAuth = this.authzRequireAuthCB.Checked;
            Settings.Store.AuthzAllowOnError = this.authzAllowOnErrorCB.Checked;
            Settings.Store.AuthzDefault = this.authzDefaultAllowRB.Checked;
            List<GroupAuthzRule> lst = new List<GroupAuthzRule>();
            foreach (Object item in this.authzRulesListBox.Items)
            {
                lst.Add(item as GroupAuthzRule);
                m_logger.DebugFormat("Saving rule: {0}", item);
            }
            string SaveAuthzRules_ret = GroupRuleLoader.SaveAuthzRules(lst);
            if (!string.IsNullOrEmpty(SaveAuthzRules_ret))
            {
                MessageBox.Show("There was an error in saving your authorization rules.\n" + SaveAuthzRules_ret);
            }

            // Gateway
            List<GroupGatewayRule> gwList = new List<GroupGatewayRule>();
            foreach (Object item in this.gatewayRulesListBox.Items)
            {
                gwList.Add(item as GroupGatewayRule);
                m_logger.DebugFormat("Saving rule: {0}", item);
            }
            string SaveGatewayRules_ret = GroupRuleLoader.SaveGatewayRules(gwList);
            if (!string.IsNullOrEmpty(SaveGatewayRules_ret))
            {
                MessageBox.Show("There was an error in saving your gateway rules.\n" + SaveGatewayRules_ret);
            }

            // Change Password
            List<AttributeEntry> entries = new List<AttributeEntry>();
            foreach (DataGridViewRow row in this.passwordAttributesDGV.Rows)
            {
                if (row.Cells[0].Value != null && row.Cells[1].Value != null)
                {
                    string attribName = row.Cells[0].Value.ToString();
                    if (!string.IsNullOrEmpty(attribName))
                    {
                        AttributeEntry entry = new AttributeEntry
                        {
                            Name = attribName,
                            Method = (Methods)(row.Cells[1].Value)
                        };
                        entries.Add(entry);
                    }
                }

            }
            CPAttributeSettings.Save(entries);
        }

        private void showPwCB_CheckedChanged(object sender, EventArgs e)
        {
            this.searchPassTextBox.UseSystemPasswordChar = !this.showPwCB.Checked;
        }

        private void authzRuleAddButton_Click(object sender, EventArgs e)
        {
            string path = this.authzRulePathTB.Text.Trim();
            if (string.IsNullOrEmpty(path))
            {
                MessageBox.Show("Please enter a DN");
                return;
            }

            int idx = this.authzRuleMemberComboBox.SelectedIndex;
            GroupRule.Condition c;
            if (idx == 0) c = GroupRule.Condition.MEMBER_OF;
            else if (idx == 1) c = GroupRule.Condition.NOT_MEMBER_OF;
            else
                throw new Exception("Unrecognized option in authzRuleAddButton_Click");


            idx = this.authzRuleActionComboBox.SelectedIndex;
            bool allow;
            if (idx == 0) allow = true;          // allow
            else if (idx == 1) allow = false;    // deny
            else
                throw new Exception("Unrecognized action option in authzRuleAddButton_Click");

            string filter = this.authzRuleFilter.Text.Trim();
            if (string.IsNullOrEmpty(path))
            {
                MessageBox.Show("Please enter a search filter");
                return;
            }
            SearchScope search = (SearchScope)this.authzRuleScope.SelectedIndex;

            GroupAuthzRule rule = new GroupAuthzRule(path, c, allow, filter, search);
            this.authzRulesListBox.Items.Add(rule);
        }

        private void gatewayRuleAddButton_Click(object sender, EventArgs e)
        {
            string localGrp = this.gatewayRuleLocalGroupTB.Text.Trim();
            if (string.IsNullOrEmpty(localGrp))
            {
                MessageBox.Show("Please enter a group name");
                return;
            }
            int idx = this.gatewayRuleGroupMemberCB.SelectedIndex;
            GroupRule.Condition c;
            if (idx == 0) c = GroupRule.Condition.MEMBER_OF;
            else if (idx == 1) c = GroupRule.Condition.NOT_MEMBER_OF;
            else if (idx == 2) c = GroupRule.Condition.ALWAYS;
            else
                throw new Exception("Unrecognized option in addGatewayGroupRuleButton_Click");

            if (c == GroupRule.Condition.ALWAYS)
            {
                this.gatewayRulesListBox.Items.Add(new GroupGatewayRule(localGrp));
            }
            else
            {
                string path = this.gatwayRulePathTB.Text.Trim();
                if (string.IsNullOrEmpty(path))
                {
                    MessageBox.Show("Please enter a DN");
                    return;
                }
                string filter = this.gatewayRuleFilter.Text;
                if (string.IsNullOrEmpty(filter))
                {
                    MessageBox.Show("Please enter a searh filter");
                    return;
                }
                SearchScope scope = (SearchScope)this.gatewayRuleScope.SelectedIndex;

                this.gatewayRulesListBox.Items.Add(new GroupGatewayRule(path, c, localGrp, filter, scope));
            }
        }

        private void gatewayRuleGroupMemberCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.gatewayRuleGroupMemberCB.SelectedIndex == 2)
            {
                this.gatwayRulePathTB.Enabled = false;
                this.gatwayRulePathTB.Text = "";
                this.gatewayRuleFilter.Enabled = false;
                this.gatewayRuleFilter.Text = "";
                this.gatewayRuleScope.Enabled = false;
                this.gatewayRuleScope.SelectedIndex = 0;
            }
            else
            {
                this.gatwayRulePathTB.Enabled = true;
                this.gatewayRuleFilter.Enabled = true;
                this.gatewayRuleScope.Enabled = true;
            }
        }

        private void gatewayRuleDeleteBtn_Click(object sender, EventArgs e)
        {
            int idx = this.gatewayRulesListBox.SelectedIndex;
            if( idx >= 0 && idx < this.gatewayRulesListBox.Items.Count )
                this.gatewayRulesListBox.Items.RemoveAt(idx);
        }

        private void authzRuleDeleteBtn_Click(object sender, EventArgs e)
        {
            int idx = this.authzRulesListBox.SelectedIndex;
            if (idx >= 0 && idx < this.authzRulesListBox.Items.Count)
                this.authzRulesListBox.Items.RemoveAt(idx);
        }

        private void authzRuleUpBtn_Click(object sender, EventArgs e)
        {
            int idx = this.authzRulesListBox.SelectedIndex;
            if (idx > 0 && idx < this.authzRulesListBox.Items.Count)
            {
                object item = this.authzRulesListBox.Items[idx];
                this.authzRulesListBox.Items.RemoveAt(idx);
                this.authzRulesListBox.Items.Insert(idx-1,item);
                this.authzRulesListBox.SelectedIndex = idx - 1;
            }
        }

        private void authzRuleDownBtn_Click(object sender, EventArgs e)
        {
            int idx = this.authzRulesListBox.SelectedIndex;
            if (idx >= 0 && idx < this.authzRulesListBox.Items.Count - 1)
            {
                object item = this.authzRulesListBox.Items[idx];
                this.authzRulesListBox.Items.RemoveAt(idx);
                this.authzRulesListBox.Items.Insert(idx + 1, item);
                this.authzRulesListBox.SelectedIndex = idx + 1;
            }
        }

        private void gatewayRulesListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.gatewayRulesListBox.SelectedItems.Count == 0)
            {
                this.gatewayRuleDeleteBtn.Enabled = false;
            }
            else
            {
                this.gatewayRuleDeleteBtn.Enabled = true;
            }
        }

        private void authzRulesListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.authzRulesListBox.SelectedItems.Count == 0)
            {
                this.authzRuleDeleteBtn.Enabled = false;
            }
            else
            {
                this.authzRuleDeleteBtn.Enabled = true;
            }
        }

        private void useAuthBindForAuthzAndGatewayCb_CheckedChanged(object sender, EventArgs e)
        {
            if (useAuthBindForAuthzAndGatewayCb.Checked)
            {
                searchDnTextBox.Enabled = false;
                searchPassTextBox.Enabled = false;
            }
            else
            {
                searchDnTextBox.Enabled = true;
                searchPassTextBox.Enabled = true;
            }
        }

        private void Btn_help(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://mutonufoai.github.io/pgina/documentation/plugins/ldap.html");
        }
    }
}
