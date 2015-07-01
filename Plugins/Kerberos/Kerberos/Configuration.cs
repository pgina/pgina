using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace pGina.Plugin.Kerberos
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
            string Realm_str = Settings.Store.Realm;

            this.rText.Text = Realm_str;
        }

        private void UiToSettings()
        {
            Settings.Store.Realm = this.rText.Text.Trim();
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
    }
}
