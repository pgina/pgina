using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace pGina.Plugin.ScriptRunner
{
    public partial class AddScript : Form
    {
        public AddScript()
        {
            InitializeComponent();
            InitUI();
        }

        private void InitUI()
        {
            this.batchRB.Checked = true;
            this.userSessionCB.Checked = true;
        }

        private void browseFileBtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = @"C:\";

            if (this.batchRB.Checked)
            {
                dlg.Filter = "Batch scripts (*.bat)|*.bat";
            }
            else
            {
                dlg.Filter = "PowerShell scripts (*.ps)|*.ps";
            }

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                this.fileTB.Text = dlg.FileName;
            }
        }
    }
}
