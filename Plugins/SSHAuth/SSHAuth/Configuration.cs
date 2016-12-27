using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace pGina.Plugin.SSHAuth
{
    public partial class Configuration : Form
    {
        public Configuration()
        {
            InitializeComponent();
            SettingsToUi();
        }

        private void SettingsToUi()
        {
            string Host_str = Settings.Store.Host;
            string Port_str = Settings.Store.Port;

            this.hostText.Text = Host_str;
            this.portText.Text = Port_str;
        }

        private void UiToSettings()
        {
            Settings.Store.Host = this.hostText.Text.Trim();
            Settings.Store.Port = this.portText.Text.Trim();
        }

        private void save_Click(object sender, EventArgs e)
        {
            this.UiToSettings();
            this.Close();
        }

        private void cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Btn_help(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://mutonufoai.github.io/pgina/documentation/plugins/sshauth.html");
        }

        private void description_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click_1(object sender, EventArgs e)
        {

        }
    }
}
