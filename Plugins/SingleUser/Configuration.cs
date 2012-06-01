using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using pGina.Shared.Settings;

namespace pGina.Plugin.SingleUser
{
    public partial class Configuration : Form
    {       
        public Configuration()
        {
            InitializeComponent();
            SettingsToUi();
        }

        public void SettingsToUi()
        {
            m_txtUser.Text = Settings.Store.Username;
            m_txtDomain.Text = Settings.Store.Domain;
            m_txtPass.Text = Settings.Store.GetEncryptedSetting("Password", null);
            m_chkRequirePlugin.Checked = Settings.Store.RequirePlugins;
            m_chkRequireAnyPlugin.Checked = Settings.Store.RequireAnyPlugins;
            string[] plugins = Settings.Store.RequiredPluginList;

            foreach (string uuid in plugins)
            {
                m_dgv.Rows.Add(new string[] { uuid });
            }

            maskUI();
        }

        public bool SaveSettings()
        {
            if (m_chkRequireAnyPlugin.Checked && m_chkRequirePlugin.Checked)
            {
                MessageBox.Show("You may not have both \"Require All Plugins\" and \"Require Any Plugins\" checked.");
                return false;
            }

            Settings.Store.Username = m_txtUser.Text;
            Settings.Store.Domain = m_txtDomain.Text;
            Settings.Store.SetEncryptedSetting("Password", m_txtPass.Text, null);
            Settings.Store.RequirePlugins = m_chkRequirePlugin.Checked;
            Settings.Store.RequireAnyPlugins = m_chkRequireAnyPlugin.Checked;

            List<string> uuids = new List<string>();
            foreach (DataGridViewRow row in m_dgv.Rows)
            {
                if (row.Cells[0].Value != null)
                    uuids.Add((string) row.Cells[0].Value);
            }

            Settings.Store.RequiredPluginList = uuids.ToArray();

            return true;
        }



        private void btnOk_Click(object sender, EventArgs e)
        {
            if (SaveSettings())
            {
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void requirePluginCheckChange(object sender, EventArgs e)
        {
            if (sender == m_chkRequireAnyPlugin && m_chkRequireAnyPlugin.Checked)
                m_chkRequirePlugin.Checked = false;
            else if (sender == m_chkRequirePlugin && m_chkRequirePlugin.Checked)
                m_chkRequireAnyPlugin.Checked = false;
            maskUI();
        }

        private void maskUI()
        {
            m_dgv.Enabled = m_chkRequireAnyPlugin.Checked || m_chkRequirePlugin.Checked;
        }
    }
}
