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
            int port = 0;
            int timeout = 0;
            try
            {
                port = Convert.ToInt32(portTB.Text.Trim());
                timeout = (int)(1000 * Convert.ToDouble(timeoutTB.Text.Trim()));
                if (port <= 0 || timeout <= 0)
                    throw new FormatException("Port and Timeout must be greater than 0");
            }
            catch (FormatException)
            {
                MessageBox.Show("Port and Timeout values must be numbers greater than 0.");
                return false;
            }

            Settings.Store.Server = serverTB.Text.Trim();
            Settings.Store.Port = port;
            Settings.Store.SetEncryptedSetting("SharedSecret", secretTB.Text);
            Settings.Store.Timeout = timeout;
            return true;
        }

        private void load()
        {
            serverTB.Text = Settings.Store.Server;
            portTB.Text = String.Format("{0}", (int)Settings.Store.Port);
            secretTB.Text = Settings.Store.GetEncryptedSetting("SharedSecret") ;

           
            timeoutTB.Text = String.Format("{0:0.00}", (int)Settings.Store.Timeout / 1000.0);
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
