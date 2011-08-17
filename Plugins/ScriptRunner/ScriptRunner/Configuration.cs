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
    public partial class Configuration : Form
    {
        public Configuration()
        {
            InitializeComponent();
        }

        private void addBtn_Click(object sender, EventArgs e)
        {
            AddScript dlg = new AddScript();
            dlg.ShowDialog();
        }
    }
}
