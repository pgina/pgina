using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

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
            string host = PluginImpl.Settings.Host;
            this.hostTB.Text = host;
            int port = PluginImpl.Settings.Port;
            this.portTB.Text = Convert.ToString(port);
            string db = PluginImpl.Settings.Database;
            this.dbTB.Text = db;
            string user = PluginImpl.Settings.User;
            this.userTB.Text = user;
            string pass = PluginImpl.Settings.Password;
            this.passwdTB.Text = pass;
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
                PluginImpl.Settings.Port = Convert.ToInt32(this.portTB.Text);
            }
            catch (FormatException e)
            {
                MessageBox.Show("Invalid port number.");
                return false;
            }

            PluginImpl.Settings.Host = this.hostTB.Text.Trim();
            PluginImpl.Settings.Database = this.dbTB.Text.Trim();
            PluginImpl.Settings.User = this.userTB.Text.Trim();
            PluginImpl.Settings.Password = this.passwdTB.Text;

            return true;
        }

        private void testButton_Click(object sender, EventArgs e)
        {

        }

        private void createTableBtn_Click(object sender, EventArgs e)
        {

        }

    }
}
