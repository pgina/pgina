using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using pGina.Shared.Settings;

namespace pGina.Plugin.RADIUS
{
    public partial class Configuration : Form
    {
        dynamic m_settings = new pGinaDynamicSettings(RADIUSPlugin.SimpleUuid);

        public Configuration()
        {
            InitializeComponent();
            secretTB.UseSystemPasswordChar = true;
            sendNasIdentifierCB.CheckedChanged += cbDisableTBInput;
            sendCalledStationCB.CheckedChanged += cbDisableTBInput;

            load();
        }



        private bool save()
        {
            int authport = 0;
            int acctport = 0;
            int timeout = 0;
            int retry = 0;
            
            try
            {
                authport = Convert.ToInt32(authPortTB.Text.Trim());
                acctport = Convert.ToInt32(acctPortTB.Text.Trim());
                timeout = (int)(1000 * Convert.ToDouble(timeoutTB.Text.Trim()));
                retry = Convert.ToInt32(retryTB.Text.Trim());
                if (authport <= 0 || acctport <= 0 || timeout <= 0 || retry <= 0)
                    throw new FormatException("Ports, Retry and Timeout values must be values greater than 0");
            }
            catch (FormatException)
            {
                MessageBox.Show("Port and Timeout values must be numbers greater than 0.");
                return false;
            }

            if (!sendNasIpAddrCB.Checked && !sendNasIdentifierCB.Checked)
            {
                MessageBox.Show("Send NAS IP Address or Send NAS Identifier must be checked under Authentication Options");
                return false;
            }

            if (sendNasIdentifierCB.Checked && String.IsNullOrEmpty(sendNasIdentifierTB.Text.Trim()))
            {
                MessageBox.Show("NAS Identifier can not be blank if the option is enabled.");
                return false;
            }

            if (sendCalledStationCB.Checked && String.IsNullOrEmpty(sendCalledStationTB.Text.Trim()))
            {
                MessageBox.Show("Called-Station-ID can not be blank if the option is enabled.");
                return false;
            }


            Settings.Store.Server = serverTB.Text.Trim();
            Settings.Store.AuthPort = authport;
            Settings.Store.AcctPort = acctport;
            Settings.Store.SetEncryptedSetting("SharedSecret", secretTB.Text);
            Settings.Store.Timeout = timeout;
            Settings.Store.Retry = retry;

            Settings.Store.SendNASIPAddress = sendNasIpAddrCB.Checked;
            Settings.Store.SendNASIdentifier = sendNasIdentifierCB.Checked;
            Settings.Store.NASIdentifier = sendNasIdentifierTB.Text.Trim();
            Settings.Store.SendCalledStationID = sendCalledStationCB.Checked;
            Settings.Store.CalledStationID = sendCalledStationTB.Text.Trim();

            Settings.Store.SendInterimUpdates = sendInterimUpdatesCB.Checked;

            Settings.Store.AllowSessionTimeout = sessionTimeoutCB.Checked;
            Settings.Store.WisprSessionTerminate = wisprTimeoutCB.Checked;
            
            Settings.Store.UseModifiedName = useModifiedNameCB.Checked;
            Settings.Store.IPSuggestion = ipAddrSuggestionTB.Text.Trim();

            return true;
        }

        private void load()
        {
            serverTB.Text = Settings.Store.Server;
            authPortTB.Text = String.Format("{0}", (int)Settings.Store.AuthPort);
            acctPortTB.Text = String.Format("{0}", (int)Settings.Store.AcctPort);
            secretTB.Text = Settings.Store.GetEncryptedSetting("SharedSecret") ;
            timeoutTB.Text = String.Format("{0:0.00}", (int)Settings.Store.Timeout / 1000.0);
            retryTB.Text = String.Format("{0}", (int)Settings.Store.Retry);

            sendNasIdentifierCB.Checked = Settings.Store.SendNASIPAddress;
            sendNasIdentifierCB.Checked = Settings.Store.SendNASIdentifier;
            sendNasIdentifierTB.Text = Settings.Store.NASIdentifier;
            sendCalledStationCB.Checked = Settings.Store.SendCalledStationID;
            sendCalledStationTB.Text = Settings.Store.CalledStationID;

            sendInterimUpdatesCB.Checked = Settings.Store.SendInterimUpdates;

            sessionTimeoutCB.Checked = Settings.Store.AllowSessionTimeout;
            wisprTimeoutCB.Checked = Settings.Store.WisprSessionTerminate;

            ipAddrSuggestionTB.Text = Settings.Store.IPSuggestion;
            useModifiedNameCB.Checked = Settings.Store.UseModifiedName;
        }

        private void cbDisableTBInput(object sender, EventArgs e)
        {
            sendNasIdentifierTB.Enabled = sendNasIdentifierCB.Checked;
            sendCalledStationTB.Enabled = sendCalledStationCB.Checked;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;

            if(save())
                this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void showSecretChanged(object sender, EventArgs e)
        {
            secretTB.UseSystemPasswordChar = !showSecretCB.Checked;
        }


        private void Configuration_Load(object sender, EventArgs e)
        {

        }
    }
}
