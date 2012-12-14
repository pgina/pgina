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
    public partial class TextBoxInfoDialog : Form
    {
        public TextBoxInfoDialog()
        {
            InitializeComponent();
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
