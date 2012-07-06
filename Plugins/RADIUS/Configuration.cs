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

            MachineIdentifier machineId = MachineIdentifier.IP_Address;
            if (machineNameButton.Checked) 
                machineId = MachineIdentifier.Machine_Name;
            else if (bothButton.Checked) 
                machineId = MachineIdentifier.Both;

            Settings.Store.Server = serverTB.Text.Trim();
            Settings.Store.AuthPort = authport;
            Settings.Store.AcctPort = acctport;
            Settings.Store.SetEncryptedSetting("SharedSecret", secretTB.Text);
            Settings.Store.Timeout = timeout;
            Settings.Store.Retry = retry;
            Settings.Store.UseModifiedName = useModifiedNameCB.Checked;
            Settings.Store.IPSuggestion = ipAddrSuggestionTB.Text.Trim();
            Settings.Store.MachineIdentifier = (int)machineId;
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
            ipAddrSuggestionTB.Text = Settings.Store.IPSuggestion;
            useModifiedNameCB.Checked = Settings.Store.UseModifiedName;

            MachineIdentifier mid = (MachineIdentifier)((int)Settings.Store.MachineIdentifier);
            if (mid == MachineIdentifier.IP_Address)
                ipAddressButton.Checked = true;
            else if (mid == MachineIdentifier.Machine_Name)
                machineNameButton.Checked = true;
            else
                bothButton.Checked = true;
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
    }
}
