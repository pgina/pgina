using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace OpenCredential.Configuration
{
    public partial class LogViewWindow : Form
    {
        internal TextBox LogTextBox { get { return logTextArea; } }

        public LogViewWindow()
        {
            InitializeComponent();
            Text = T("Log.Title", "OpenCredential Log");
            saveBtn.Text = T("Log.Save", "Save");
            saveBtn.AccessibleName = saveBtn.Text;
            logTextArea.AccessibleName = T("Log.Content", "Technical log contents");
        }

        private static string T(string key, string english)
        {
            return OpenCredential.Shared.Localization.UiLanguage.Text("Configuration", key, english);
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = T("Log.TextFile", "Text File (.txt)").Replace("|", "") + "|*.txt|" +
                T("Log.AllFiles", "All Files (*.*)").Replace("|", "") + "|*.*";
            sfd.CheckPathExists = true;

            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (StreamWriter file = new StreamWriter(sfd.FileName))
                {
                    file.Write(logTextArea.Text);
                }

                MessageBox.Show(string.Format(T("Log.Saved", "File saved successfully: {0}"), sfd.FileName), T("Log.Export", "Log Export"), MessageBoxButtons.OK);
            }
        }
        
    }
}
