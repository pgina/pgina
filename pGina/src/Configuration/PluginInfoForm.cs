using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using pGina.Shared.Interfaces;

namespace pGina.Configuration
{
    public partial class PluginInfoForm : Form
    {
        public IPluginBase Plugin { get; set; }

        public PluginInfoForm()
        {
            InitializeComponent();
        }

        private void PluginInfoForm_Load(object sender, EventArgs e)
        {
            this.nameTextBox.Text = Plugin.Name;
            this.uuidTextBox.Text = Plugin.Uuid.ToString();
            this.descriptionTextBox.Text = Plugin.Description;
            this.versionTextBox.Text = Plugin.Version;
        }
    }
}
