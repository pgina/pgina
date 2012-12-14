/*
	Copyright (c) 2012, pGina Team
	All rights reserved.

	Redistribution and use in source and binary forms, with or without
	modification, are permitted provided that the following conditions are met:
		* Redistributions of source code must retain the above copyright
		  notice, this list of conditions and the following disclaimer.
		* Redistributions in binary form must reproduce the above copyright
		  notice, this list of conditions and the following disclaimer in the
		  documentation and/or other materials provided with the distribution.
		* Neither the name of the pGina Team nor the names of its contributors 
		  may be used to endorse or promote products derived from this software without 
		  specific prior written permission.

	THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
	ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
	WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
	DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY
	DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
	(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
	LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
	ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
	(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
	SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
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
            bool ScramblePasswords = Settings.Store.ScramblePasswords;
            bool RemoveProfiles = Settings.Store.RemoveProfiles;
            bool scrWhenLMFail = Settings.Store.ScramblePasswordsWhenLMAuthFails;
            string[] scrambleExceptions = Settings.Store.ScramblePasswordsExceptions;

            m_chkAlwaysAuth.Checked = AlwaysAuthenticate;
            m_chkAuthzLocalAdmin.Checked = AuthzLocalAdminsOnly;
            m_chkAuthzRequireLocal.Checked = AuthzLocalGroupsOnly;
            m_chkScramble.Checked = ScramblePasswords;
            m_chkRemoveProfile.Checked = RemoveProfiles;
            m_chkScrambleWhenLMFails.Checked = scrWhenLMFail;

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

            foreach (string user in scrambleExceptions)
            {
                m_scrambleAllExceptDGV.Rows.Add(new string[] { user });
            }

            MaskAuthzUi();
            MaskGatewayUi();
        }

        public void UiToSettings()
        {
            Settings.Store.AlwaysAuthenticate = m_chkAlwaysAuth.Checked;
            Settings.Store.AuthzLocalAdminsOnly = m_chkAuthzLocalAdmin.Checked;
            Settings.Store.AuthzLocalGroupsOnly = m_chkAuthzRequireLocal.Checked;
            Settings.Store.ScramblePasswords = m_chkScramble.Checked;
            Settings.Store.RemoveProfiles = m_chkRemoveProfile.Checked;
            Settings.Store.ScramblePasswordsWhenLMAuthFails = m_chkScrambleWhenLMFails.Checked;

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

            // Gather the exceptions list and store 
            List<string> exceptions = new List<string>();
            foreach (DataGridViewRow row in m_scrambleAllExceptDGV.Rows)
            {
                if (row.Cells[0].Value != null)
                    exceptions.Add((string)row.Cells[0].Value);
            }

            if (exceptions.Count > 0)
                Settings.Store.ScramblePasswordsExceptions = exceptions.ToArray();
            else
                Settings.Store.ScramblePasswordsExceptions = new string[] { };
        }

        public void MaskAuthzUi()
        {
            m_localGroupDgv.Enabled = m_chkAuthzRequireLocal.Checked;
        }

        public void MaskGatewayUi()
        {
            if (m_chkScramble.Checked)
            {
                m_chkScrambleWhenLMFails.Enabled = true;
                m_scrambleAllExceptDGV.Enabled = true;
            }
            else
            {
                m_chkScrambleWhenLMFails.Enabled = false;
                m_scrambleAllExceptDGV.Enabled = false;
            }
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

        private void m_chkScramble_CheckedChanged(object sender, EventArgs e)
        {
            MaskGatewayUi();
        }

        private void m_chkRemoveProfile_CheckedChanged(object sender, EventArgs e)
        {
            
        }

    }
}
