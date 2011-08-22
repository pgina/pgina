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
        internal Script ScriptData { get; set; }

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
                dlg.Filter = "PowerShell scripts (*.ps1)|*.ps1";
            }

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                this.fileTB.Text = dlg.FileName;
            }
        }

        private void cancelBtn_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void okBtn_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;

            string fn = this.fileTB.Text.Trim();
            if (this.batchRB.Checked)
            {
                this.ScriptData = new BatchScript(fn);
            }
            else
            {
                this.ScriptData = new PowerShellScript(fn);
            }

            this.ScriptData.UserSession = this.userSessionCB.Checked;
            this.ScriptData.SystemSession = this.systemSessionCB.Checked;
            this.Close();
        }
    }
}
