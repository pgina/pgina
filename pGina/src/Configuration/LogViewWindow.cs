using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace pGina.Configuration
{
    public partial class LogViewWindow : Form
    {
        internal TextBox LogTextBox { get { return logTextArea; } }

        public LogViewWindow()
        {
            InitializeComponent();
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Text File (.txt)|*.txt|All Files (*.*)|*.*";
            sfd.CheckPathExists = true;

            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (StreamWriter file = new StreamWriter(sfd.FileName))
                {
                    file.Write(logTextArea.Text);
                }

                MessageBox.Show(string.Format("File saved successfully: {0}", sfd.FileName), "Log Export", MessageBoxButtons.OK);
            }
        }
        
    }
}
