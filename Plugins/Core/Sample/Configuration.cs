using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using OpenCredential.Shared.Settings;
using OpenCredential.Plugin.Presentation;

namespace OpenCredential.Plugin.Sample
{
    public partial class Configuration : Form
    {
        dynamic m_settings;

        public Configuration() : this(true)
        {
        }

        internal Configuration(bool initializeSettings)
        {
            InitializeComponent();
            PluginPresentation.Apply(this, "Sample");
            var body = new Panel { AutoScroll = true };
            PluginPresentation.Stack(body, PluginPresentation.Fields(lblDesc, txtDescription, null, chkShowDescription));
            PluginPresentation.Shell(this, body, btnOk, btnCancel);
            MinimumSize = new Size(520, 240);
            Size = new Size(640, 320);
            if (initializeSettings)
            {
                m_settings = new OpenCredentialDynamicSettings(SimplePlugin.SimpleUuid);
                txtDescription.Text = m_settings.Description;
                chkShowDescription.Checked = m_settings.ShowDescription;
            }
            else PluginPresentation.DisablePreviewActions(this);
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            m_settings.Description = txtDescription.Text;
            m_settings.ShowDescription = chkShowDescription.Checked;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }
    }
}
