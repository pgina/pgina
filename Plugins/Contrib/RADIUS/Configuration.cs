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
            sendNasIdentifierCB.CheckedChanged += checkboxModifyInputs;
            sendCalledStationCB.CheckedChanged += checkboxModifyInputs;
            enableAuthCB.CheckedChanged += checkboxModifyInputs;
            enableAcctCB.CheckedChanged += checkboxModifyInputs;
            sendInterimUpdatesCB.CheckedChanged += checkboxModifyInputs;
            load();
        }

        private bool save()
        {
            int authport = 0;
            int acctport = 0;
            int timeout = 0;
            int retry = 0;
            int interim_time = 0;
            
            try
            {
                authport = Convert.ToInt32(authPortTB.Text.Trim());
                acctport = Convert.ToInt32(acctPortTB.Text.Trim());
                timeout = (int)(1000 * Convert.ToDouble(timeoutTB.Text.Trim()));
                retry = Convert.ToInt32(retryTB.Text.Trim());
                interim_time = Convert.ToInt32(forceInterimUpdTB.Text.Trim());
                
                if (authport <= 0 || acctport <= 0 || timeout <= 0 || retry <= 0 || interim_time <= 0)
                    throw new FormatException("Ports, Retry, Timeout and interval values must be values greater than 0");
            }
            catch (FormatException)
            {
                MessageBox.Show("Port and Timeout values must be numbers greater than 0.");
                return false;
            }


            if (enableAuthCB.Checked) //Settings only relevent to users with auth checked
            {
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
            }

            Settings.Store.EnableAuth = enableAuthCB.Checked;
            Settings.Store.EnableAcct = enableAcctCB.Checked;

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

            Settings.Store.AcctingForAllUsers = acctingForAllUsersCB.Checked;
            Settings.Store.SendInterimUpdates = sendInterimUpdatesCB.Checked;
            Settings.Store.ForceInterimUpdates = forceInterimUpdCB.Checked;
            Settings.Store.InterimUpdateTime = interim_time;
               

            Settings.Store.AllowSessionTimeout = sessionTimeoutCB.Checked;
            Settings.Store.WisprSessionTerminate = wisprTimeoutCB.Checked;
            
            Settings.Store.UseModifiedName = useModifiedNameCB.Checked;
            Settings.Store.IPSuggestion = ipAddrSuggestionTB.Text.Trim();

            return true;
        }

        private void load()
        {
            enableAuthCB.Checked = (bool)Settings.Store.EnableAuth;
            enableAcctCB.Checked = (bool)Settings.Store.EnableAcct;

            serverTB.Text = Settings.Store.Server;
            authPortTB.Text = String.Format("{0}", (int)Settings.Store.AuthPort);
            acctPortTB.Text = String.Format("{0}", (int)Settings.Store.AcctPort);
            secretTB.Text = Settings.Store.GetEncryptedSetting("SharedSecret") ;
            timeoutTB.Text = String.Format("{0:0.00}", ((int)Settings.Store.Timeout) / 1000.0 ); //2500ms
            retryTB.Text = String.Format("{0}", (int)Settings.Store.Retry);

            sendNasIpAddrCB.Checked = (bool)Settings.Store.SendNASIPAddress;
            sendNasIdentifierCB.Checked = (bool)Settings.Store.SendNASIdentifier;
            sendNasIdentifierTB.Text = Settings.Store.NASIdentifier;
            sendCalledStationCB.Checked = (bool)Settings.Store.SendCalledStationID;
            sendCalledStationTB.Text = Settings.Store.CalledStationID;

            acctingForAllUsersCB.Checked = (bool)Settings.Store.AcctingForAllUsers;
            sendInterimUpdatesCB.Checked = (bool)Settings.Store.SendInterimUpdates;
            forceInterimUpdCB.Checked = (bool)Settings.Store.ForceInterimUpdates;
            forceInterimUpdTB.Text = String.Format("{0}", (int)Settings.Store.InterimUpdateTime);

            sessionTimeoutCB.Checked = (bool)Settings.Store.AllowSessionTimeout;
            wisprTimeoutCB.Checked = (bool)Settings.Store.WisprSessionTerminate;

            ipAddrSuggestionTB.Text = Settings.Store.IPSuggestion;
            useModifiedNameCB.Checked = (bool)Settings.Store.UseModifiedName;
        }

        private void checkboxModifyInputs(object sender, EventArgs e)
        {
            //Server Settings
            authPortTB.Enabled = enableAuthCB.Checked;
            acctPortTB.Enabled = enableAcctCB.Checked;
            
            //Authentication options:
            authGB.Enabled = enableAuthCB.Checked;
            sendNasIdentifierTB.Enabled = sendNasIdentifierCB.Checked;
            sendCalledStationTB.Enabled = sendCalledStationCB.Checked;

            //Accounting options
            acctGB.Enabled = enableAcctCB.Checked;
            forceInterimUpdCB.Enabled = sendInterimUpdatesCB.Checked;
            forceInterimUpdTB.Enabled = forceInterimUpdCB.Enabled;
            forceInterimUpdLbl.Enabled = forceInterimUpdCB.Enabled;
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

        //Converts value to int, or returns default value. 
        /*private int stoi(Object o, int def = 0)
        {
            try{ return (int)o; }
            catch (InvalidCastException) { 
               
                return def; }
        }*/
    }
}
