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
using System.Text.RegularExpressions;
using System.IO;

using pGina.Shared.Settings;
using pGina.Plugin.Ldap;

namespace pGina.Plugin.Ldap
{
    public partial class Configuration : Form
    {
        public Configuration()
        {
            InitializeComponent();

            LoadSettings();
            UpdateSslElements();
            UpdateAuthenticationElements();
        }

        private void LoadSettings()
        {
            dynamic settings = new pGinaDynamicSettings(LdapPlugin.LdapUuid);

            // Set default values for settings (if not already set)
            settings.SetDefault("LdapHost", new string[] { "ldap.example.com" });
            settings.SetDefault("LdapPort", 389);
            settings.SetDefault("LdapTimeout", 10);
            settings.SetDefault("UseSsl", false);
            settings.SetDefault("RequireCert", false);
            settings.SetDefault("ServerCertFile", "");
            settings.SetDefault("DoSearch", false);
            settings.SetDefault("SearchContexts", new string[] { });
            settings.SetDefault("SearchFilter", "");
            settings.SetDefault("DnPattern", "uid=%u,dc=example,dc=com");
            settings.SetDefault("SearchDN", "");
            settings.SetDefault("SearchPW", "");
            settings.SetDefault("DoGroupAuthorization", false);
            settings.SetDefault("LdapLoginGroups", new string[] { });
            settings.SetDefault("LdapAdminGroup", "wheel");

            string[] ldapHosts = settings.LdapHost;
            string hosts = "";
            for (int i = 0; i < ldapHosts.Count(); i++)
            {
                string host = ldapHosts[i];
                if (i < ldapHosts.Count() - 1) hosts += host + " ";
                else hosts += host;
            }
            ldapHostTextBox.Text = hosts;

            int port = settings.LdapPort;
            ldapPortTextBox.Text = Convert.ToString(port);

            int timeout = settings.LdapTimeout;
            timeoutTextBox.Text = Convert.ToString(timeout);

            bool useSsl = settings.UseSsl;
            useSslCheckBox.CheckState = useSsl ? CheckState.Checked : CheckState.Unchecked;

            bool reqCert = settings.RequireCert;
            validateServerCertCheckBox.CheckState = reqCert ? CheckState.Checked : CheckState.Unchecked;

            string serverCertFile = settings.ServerCertFile;
            sslCertFileTextBox.Text = serverCertFile;

            bool doSearch = settings.DoSearch;
            searchForDnCheckBox.CheckState = doSearch ? CheckState.Checked : CheckState.Unchecked;

            string[] searchContexts = settings.SearchContexts;
            string ctxs = "";
            for (int i = 0; i < searchContexts.Count(); i++)
            {
                string ctx = searchContexts[i];
                if (i < searchContexts.Count() - 1) ctxs += ctx + "\r\n";
                else ctxs += ctx;
            }
            searchContextsTextBox.Text = ctxs;

            string filter = settings.SearchFilter;
            searchFilterTextBox.Text = filter;

            string dnPattern = settings.DnPattern;
            dnPatternTextBox.Text = dnPattern;

            string searchDn = settings.SearchDN;
            searchDnTextBox.Text = searchDn;

            string searchPw = settings.SearchPW;
            searchPassTextBox.Text = searchPw;

        }

        private void ldapServerGroupBox_Enter(object sender, EventArgs e)
        {

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
            UpdateSslElements();
        }

        private void UpdateSslElements()
        {
            if (validateServerCertCheckBox.CheckState == CheckState.Checked)
            {
                sslCertFileTextBox.Enabled = true;
                sslCertFileBrowseButton.Enabled = true;
            }
            else if (validateServerCertCheckBox.CheckState == CheckState.Unchecked)
            {
                sslCertFileTextBox.Enabled = false;
                sslCertFileBrowseButton.Enabled = false;
            }

            if (useSslCheckBox.CheckState == CheckState.Unchecked)
            {
                validateServerCertCheckBox.Enabled = false;
                sslCertFileTextBox.Enabled = false;
                sslCertFileBrowseButton.Enabled = false;
            }
            else if (useSslCheckBox.CheckState == CheckState.Checked)
            {
                validateServerCertCheckBox.Enabled = true;
            }
        }

        private void UpdateAuthenticationElements()
        {
            if (searchForDnCheckBox.CheckState == CheckState.Checked)
            {
                searchFilterTextBox.Enabled = true;
                searchContextsTextBox.Enabled = true;
                searchDnTextBox.Enabled = true;
                searchPassTextBox.Enabled = true;
                dnPatternTextBox.Enabled = false;
            }
            else if( searchForDnCheckBox.CheckState == CheckState.Unchecked )
            {
                searchFilterTextBox.Enabled = false;
                searchContextsTextBox.Enabled = false;
                searchDnTextBox.Enabled = false;
                searchPassTextBox.Enabled = false;
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

        private void okButton_Click(object sender, EventArgs e)
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

            if (searchForDnCheckBox.CheckState == CheckState.Checked &&
                searchFilterTextBox.Text.Trim().Length == 0)
            {
                MessageBox.Show("Please provide a search filter when \"Search for DN\" is enabled.");
                return false;
            }

            if (searchForDnCheckBox.CheckState == CheckState.Checked &&
                searchContextsTextBox.Text.Trim().Length == 0)
            {
                MessageBox.Show("Please provide at least one search context when \"Search for DN\" is enabled.");
                return false;
            }

            if (searchForDnCheckBox.CheckState != CheckState.Checked &&
                dnPatternTextBox.Text.Trim().Length == 0)
            {
                MessageBox.Show("Please provide a DN pattern.");
                return false;
            }

            // TODO: Make sure that other input is valid.

            return true;
        }

        private void StoreSettings()
        {
            dynamic settings = new pGinaDynamicSettings(LdapPlugin.LdapUuid);

            settings.LdapHost = Regex.Split(ldapHostTextBox.Text.Trim(), @"\s+"); 
            settings.LdapPort = Convert.ToInt32(ldapPortTextBox.Text.Trim());
            settings.Timeout = Convert.ToInt32(timeoutTextBox.Text.Trim());
            settings.UseSsl = (useSslCheckBox.CheckState == CheckState.Checked);
            settings.RequireCert = (validateServerCertCheckBox.CheckState == CheckState.Checked);
            settings.ServerCertFile = sslCertFileTextBox.Text.Trim();
            settings.DnPattern = dnPatternTextBox.Text.Trim();
            settings.DoSearch = (searchForDnCheckBox.CheckState == CheckState.Checked);
            settings.SearchFilter = searchFilterTextBox.Text.Trim();
            settings.SearchContexts = Regex.Split(searchContextsTextBox.Text.Trim(), @"\s*\r?\n\s*");
            settings.SearchDN = searchDnTextBox.Text.Trim();
            settings.SearchPW = searchPassTextBox.Text.Trim();
        }
    }
}
