/*
	Copyright (c) 2014, pGina Team
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

namespace pGina.Plugin.DriveMapper
{
    public partial class ConfigurationUi : Form
    {
        private string[] m_driveOptions;
        private string[] m_credsOptions;
        private BindingList<DriveMap> m_maps;

        public ConfigurationUi()
        {
            InitializeComponent();
            InitUi();
            LoadSettings();
        }

        private void InitUi()
        {
            m_driveOptions = new string[] { 
                "E:", "F:", "G:", "H:", "I:", "J:", "K:", "L:", 
                "M:", "N:", "O:", "P:", "Q:",
                "R:", "S:", "T:", "U:", "V:", 
                "W:", "X:", "Y:", "Z:" };

            m_driveLetterCb.Items.AddRange(m_driveOptions);
            m_driveLetterCb.SelectedIndex = 0;

            m_credsOptions = new string[] { 
                DriveMap.CredentialOption.Original.ToString(), 
                DriveMap.CredentialOption.Final.ToString(), 
                DriveMap.CredentialOption.Custom.ToString() };
            m_credsCb.Items.AddRange(m_credsOptions);
            m_credsCb.SelectedIndex = 1;

            DisableAndClearAllEditFields();

            m_mapListDgv.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "Drive",
                DataPropertyName = "Drive",
                HeaderText = "Drive",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                ReadOnly = true
            });
            m_mapListDgv.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "UNC",
                DataPropertyName = "UncPath",
                HeaderText = "UNC Path",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ReadOnly = true
            });
            m_mapListDgv.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "Creds",
                DataPropertyName = "Credentials",
                HeaderText = "Credentials",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                ReadOnly = true
            });
            m_mapListDgv.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "Group",
                DataPropertyName = "Group",
                HeaderText = "Group",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                ReadOnly = true
            });
            m_mapListDgv.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "Username",
                Visible = false
            });
            m_mapListDgv.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "Password",
                Visible = false
            });
            m_mapListDgv.SelectionChanged += m_mapListDgv_SelectionChanged;
            m_uncPathTb.LostFocus += m_uncPathTb_LostFocus;
            m_passwordTb.LostFocus += m_passwordTb_LostFocus;
            m_usernameTb.LostFocus += m_usernameTb_LostFocus;
            m_groupTb.LostFocus += m_groupTb_LostFocus;
        }

        void m_groupTb_LostFocus(object sender, EventArgs e)
        {
            if (m_mapListDgv.SelectedRows.Count > 0)
            {
                DataGridViewRow row = m_mapListDgv.SelectedRows[0];
                int idx = row.Index;
                DriveMap map = (DriveMap)row.DataBoundItem;
                map.Group = m_groupTb.Text;
                m_maps.ResetItem(idx);
            }
        }

        void m_usernameTb_LostFocus(object sender, EventArgs e)
        {
            if (m_mapListDgv.SelectedRows.Count > 0)
            {
                DataGridViewRow row = m_mapListDgv.SelectedRows[0];
                int idx = row.Index;
                DriveMap map = (DriveMap)row.DataBoundItem;
                map.Username = m_usernameTb.Text;
                m_maps.ResetItem(idx);
            }
        }

        void m_passwordTb_LostFocus(object sender, EventArgs e)
        {
            if (m_mapListDgv.SelectedRows.Count > 0)
            {
                DataGridViewRow row = m_mapListDgv.SelectedRows[0];
                int idx = row.Index;
                DriveMap map = (DriveMap)row.DataBoundItem;
                map.Password = m_passwordTb.Text;
                m_maps.ResetItem(idx);
            }
        }

        void m_uncPathTb_LostFocus(object sender, EventArgs e)
        {
            if (m_mapListDgv.SelectedRows.Count > 0)
            {
                DataGridViewRow row = m_mapListDgv.SelectedRows[0];
                int idx = row.Index;
                DriveMap map = (DriveMap)row.DataBoundItem;
                map.UncPath = m_uncPathTb.Text;
                m_maps.ResetItem(idx);
            }
        }

        void m_mapListDgv_SelectionChanged(object sender, EventArgs e)
        {
            int numRowsSelected = m_mapListDgv.SelectedRows.Count;
            if( numRowsSelected == 0 ) 
                DisableAndClearAllEditFields();
            else {
                DataGridViewRow row = m_mapListDgv.SelectedRows[0];
                DriveMap map = (DriveMap)row.DataBoundItem;
                SetEditFields(map);
                EnableEditFields();
                if( map.Credentials ==  DriveMap.CredentialOption.Custom)
                {
                    EnableCredentialsFields();
                }
            }
        }

        private void EnableEditFields()
        {
            m_credsCb.Enabled = true;
            m_driveLetterCb.Enabled = true;
            m_groupTb.Enabled = true;
            m_uncPathTb.Enabled = true;
        }

        private void EnableCredentialsFields()
        {
            m_usernameTb.Enabled = true;
            m_passwordTb.Enabled = true;
            m_showPasswordCb.Enabled = true;
        }

        private void SetEditFields(DriveMap map)
        {
            m_driveLetterCb.SelectedItem = map.Drive;
            m_uncPathTb.Text = map.UncPath;
            m_credsCb.SelectedItem = map.Credentials.ToString();
            m_usernameTb.Text = map.Username;
            m_passwordTb.Text = map.Password;
            m_groupTb.Text = map.Group;
        }

        private void DisableAndClearAllEditFields()
        {
            m_credsCb.Enabled = false;
            m_driveLetterCb.Enabled = false;
            m_groupTb.Enabled = false;
            m_uncPathTb.Enabled = false;
            m_uncPathTb.Text = "";
            DisableAndClearCredentialsFields();
        }

        private void DisableAndClearCredentialsFields()
        {
            m_usernameTb.Enabled = false;
            m_usernameTb.Text = "";
            m_passwordTb.Enabled = false;
            m_passwordTb.Text = "";
            m_showPasswordCb.Enabled = false;
        }

        private void LoadSettings()
        {
            List<DriveMap> maps = Settings.GetMaps();
            m_maps = new BindingList<DriveMap>(maps);
            m_mapListDgv.DataSource = m_maps;
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void m_showPasswordCb_CheckedChanged(object sender, EventArgs e)
        {
            m_passwordTb.UseSystemPasswordChar = !m_showPasswordCb.Checked;
        }

        private void newMapButton_Click(object sender, EventArgs e)
        {
            DriveMap map = new DriveMap();
            map.Drive = m_driveOptions[0];
            map.Credentials = DriveMap.CredentialOption.Final;

            // Choose a drive letter that's available
            HashSet<string> usedLetters = new HashSet<string>();
            foreach (DriveMap m in m_maps)
            {
                usedLetters.Add(m.Drive);
            }
            for (int i = 0; i < m_driveOptions.Length; i++)
            {
                if(!usedLetters.Contains(m_driveOptions[i]))
                {
                    map.Drive = m_driveOptions[i];
                    break;
                }
            }

            m_maps.Add(map);
            m_mapListDgv.Rows[m_mapListDgv.Rows.Count - 1].Selected = true;
        }

        private void m_deleteMapButton_Click(object sender, EventArgs e)
        {
            int numRowsSelected = m_mapListDgv.SelectedRows.Count;
            if (numRowsSelected > 0)
            {
                m_maps.RemoveAt(m_mapListDgv.SelectedRows[0].Index);
            }
        }

        private void m_driveLetterCb_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(m_mapListDgv.SelectedRows.Count > 0)
            {
                DataGridViewRow row = m_mapListDgv.SelectedRows[0];
                int idx = row.Index;
                DriveMap map = (DriveMap)row.DataBoundItem;
                map.Drive = m_driveLetterCb.SelectedItem.ToString();
                m_maps.ResetItem(idx);
            }
        }

        private void m_credsCb_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (m_mapListDgv.SelectedRows.Count > 0)
            {
                DataGridViewRow row = m_mapListDgv.SelectedRows[0];
                int idx = row.Index;
                DriveMap map = (DriveMap)row.DataBoundItem;
                map.Credentials = (DriveMap.CredentialOption)Enum.Parse(typeof(DriveMap.CredentialOption), m_credsCb.SelectedItem.ToString());
                m_maps.ResetItem(idx);
                if (map.Credentials == DriveMap.CredentialOption.Custom)
                    EnableCredentialsFields();
                else
                    DisableAndClearCredentialsFields();
            }
        }

        private void m_saveCloseButton_Click(object sender, EventArgs e)
        {
            List<DriveMap> maps = m_maps.ToList<DriveMap>();
            Settings.SaveMaps(maps);
            this.Close();
        }

        private void m_cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}
