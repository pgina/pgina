using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace pGina.Plugin.MySQLAuth
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
            this.hostTB.Text = Settings.Store.Host;
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            if (Save())
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private bool Save()
        {
            Settings.Store.Host = this.hostTB.Text.Trim();
            return true;
        }
    }
}
