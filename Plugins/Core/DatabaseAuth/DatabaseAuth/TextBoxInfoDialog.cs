using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OpenCredential.Plugin.DatabaseAuth
{
    public partial class TextBoxInfoDialog : Form
    {
        public TextBoxInfoDialog()
        {
            InitializeComponent();
            Text = OpenCredential.Shared.Localization.UiLanguage.Text("DatabaseAuth", "Diagnostics.Title", "Information");
            closeButton.Text = OpenCredential.Shared.Localization.UiLanguage.Text("DatabaseAuth", "Control.cancelButton", "Close");
            closeButton.AccessibleName = closeButton.Text;
        }

        public void AppendText(string text)
        {
            this.textBox.AppendText(text);
        }

        public void AppendLine(string line)
        {
            this.textBox.AppendText(line + Environment.NewLine);
        }

        public void ClearText()
        {
            this.textBox.Text = "";
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

