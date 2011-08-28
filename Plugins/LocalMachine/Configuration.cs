using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace pGina.Plugin.LocalMachine
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
            bool AlwaysAuthenticate = Settings.Store.AlwaysAuthenticate;
            bool AuthzLocalAdminsOnly = Settings.Store.AuthzLocalAdminsOnly;
            bool AuthzLocalGroupsOnly = Settings.Store.AuthzLocalGroupsOnly;
            string[] AuthzLocalGroups = Settings.Store.AuthzLocalGroups;
            bool AuthzApplyToAllUsers = Settings.Store.AuthzApplyToAllUsers;
            bool MirrorGroupsForAuthdUsers = Settings.Store.MirrorGroupsForAuthdUsers;
            bool GroupCreateFailIsFail = Settings.Store.GroupCreateFailIsFail;
            string[] MandatoryGroups = Settings.Store.MandatoryGroups;

            m_chkAlwaysAuth.Checked = AlwaysAuthenticate;
            m_chkAuthzLocalAdmin.Checked = AuthzLocalAdminsOnly;
            m_chkAuthzRequireLocal.Checked = AuthzLocalGroupsOnly;

            foreach (string group in AuthzLocalGroups)
            {
                m_localGroupDgv.Rows.Add(new string[] { group });
            }

            m_chkAuthzAll.Checked = AuthzApplyToAllUsers;
            m_chkMirror.Checked = MirrorGroupsForAuthdUsers;
            m_chkGroupFailIsFAIL.Checked = GroupCreateFailIsFail;

            foreach (string group in MandatoryGroups)
            {
                m_groupsDgv.Rows.Add(new string[] { group });
            }

            MaskAuthzUi();
        }

        public void UiToSettings()
        {
            Settings.Store.AlwaysAuthenticate = m_chkAlwaysAuth.Checked;
            Settings.Store.AuthzLocalAdminsOnly = m_chkAuthzLocalAdmin.Checked;
            Settings.Store.AuthzLocalGroupsOnly = m_chkAuthzRequireLocal.Checked;

            List<string> localGroups = new List<string>();
            foreach (DataGridViewRow row in m_localGroupDgv.Rows)
            {
                if(row.Cells[0].Value != null)
                    localGroups.Add((string) row.Cells[0].Value);
            }

            if (localGroups.Count > 0)
                Settings.Store.AuthzLocalGroups = localGroups.ToArray();
            else
                Settings.Store.AuthzLocalGroups = new string[] { };

            Settings.Store.AuthzApplyToAllUsers = m_chkAuthzAll.Checked;
            Settings.Store.MirrorGroupsForAuthdUsers = m_chkMirror.Checked;
            Settings.Store.GroupCreateFailIsFail = m_chkGroupFailIsFAIL.Checked;

            List<string> mandatory = new List<string>();
            foreach (DataGridViewRow row in m_groupsDgv.Rows)
            {
                if (row.Cells[0].Value != null)
                    mandatory.Add((string)row.Cells[0].Value);
            }

            if (mandatory.Count > 0)
                Settings.Store.MandatoryGroups = mandatory.ToArray();
            else
                Settings.Store.MandatoryGroups = new string[] { };
        }

        public void MaskAuthzUi()
        {
            m_localGroupDgv.Enabled = m_chkAuthzRequireLocal.Checked;            
        }

        private void m_chkAuthzRequireLocal_CheckedChanged(object sender, EventArgs e)
        {
            MaskAuthzUi();
        }

        private void m_btnSave_Click(object sender, EventArgs e)
        {
            UiToSettings();
            this.Close();
        }

        private void m_btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
