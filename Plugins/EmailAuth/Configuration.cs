using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using pGina.Shared.Settings;

namespace pGina.Plugin.Email
{
    public partial class Configuration : Form
    {
        dynamic m_settings = new pGinaDynamicSettings(EmailAuthPlugin.SimpleUuid);
        private static string popSslPort = "995";
        private static string popPort = "110";
        private static string imapPort = "143";
        private static string imapSslPort = "993";

        public Configuration()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            serverTextBox.Text = Settings.Store.Server;
            sslCheckBox.Checked = Settings.Store.UseSsl;
            if ((String)Settings.Store.Protocol == "POP3")
                popButton.Checked = true;
            else if ((String)Settings.Store.Protocol == "IMAP")
                imapButton.Checked = true;

            portTextBox.Text = Settings.Store.Port;
            domainAppendCheckBox.Checked = Settings.Store.AppendDomain;
            domainTextBox.Text = Settings.Store.Domain;
            int timeout = Settings.Store.NetworkTimeout;
            tbTimeout.Text = timeout.ToString();
            updateSettings();
        }

        private void StoreSettings()
        {
            Settings.Store.Server = serverTextBox.Text.Trim();
            Settings.Store.UseSsl = sslCheckBox.Checked;
            Settings.Store.Protocol = (popButton.Checked) ? "POP3" : "IMAP";
            Settings.Store.Port = portTextBox.Text.Trim();
            Settings.Store.AppendDomain = domainAppendCheckBox.Checked;
            Settings.Store.Domain = domainTextBox.Text.Replace('@', ' ').Trim();
            try
            {
                int timeout = Convert.ToInt32(tbTimeout.Text);
                Settings.Store.NetworkTimeout = timeout;
            }
            catch (FormatException) { }
        }

        private bool ValidateInput()
        {
            if (serverTextBox.Text.Trim().Length == 0)
            {
                MessageBox.Show("Server address can not be blank.");
                return false;
            }

            if (!popButton.Checked && !imapButton.Checked)
            {
                MessageBox.Show("A protocol must be selected.");
                return false;
            }

            try
            {
                int port = Convert.ToInt32(portTextBox.Text);
                if (port < 0) throw new FormatException();
            }
            catch (FormatException)
            {
                MessageBox.Show("Port must be an integer greater than 0.");
                return false;
            }

            if (domainAppendCheckBox.Checked && domainTextBox.Text.Trim().Length == 0)
            {
                MessageBox.Show("A domain must be entered if \"Append domain to username\" is checked.");
                return false;
            }

            return true;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (ValidateInput())
            {
                StoreSettings();
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
            }

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
            domainLabel.Enabled = domainAppendCheckBox.Checked;
            domainTextBox.Enabled = domainAppendCheckBox.Checked;
        }

        //Changes the default port value if the protocol or SSL status is changed
        private void changedProtocol(Object sender, EventArgs e)
        {
            if(popButton.Checked)
                portTextBox.Text = (sslCheckBox.Checked ? popSslPort : popPort);
            else
                portTextBox.Text = (sslCheckBox.Checked ? imapSslPort : imapPort);
        }

    }
}
