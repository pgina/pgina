/*
	Copyright (c) 2011, pGina Team
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

namespace pGina.Plugin.ScriptRunner
{
    public partial class Configuration : Form
    {
        private BindingList<Script> scriptList;

        private static readonly string SCRIPT_TYPE_COLUMN = "ScriptType";
        private static readonly string SCRIPT_FILE_COLUMN = "ScriptFileName";
        private static readonly string USER_SESSION_COLUMN = "UserSession";
        private static readonly string SYSTEM_SESSION_COLUMN = "SystemSession";
        private static readonly string GUID_COLUMN = "Guid";

        public Configuration()
        {
            InitializeComponent();
            scriptList = new BindingList<Script>(Settings.Load());
            InitUI();
        }

        private void InitUI()
        {
            timeoutTB.Text = Convert.ToString(Script.Timeout);

            DataGridView dgv = this.scriptListDGV;
            dgv.RowHeadersVisible = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
            dgv.AllowUserToAddRows = false;

            dgv.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = SCRIPT_TYPE_COLUMN,
                DataPropertyName = "Type",
                HeaderText = "Type",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                ReadOnly = true
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = SCRIPT_FILE_COLUMN,
                DataPropertyName = "File",
                HeaderText = "File Name",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            dgv.Columns.Add(new DataGridViewCheckBoxColumn()
            {
                Name = USER_SESSION_COLUMN,
                DataPropertyName = "UserSession",
                HeaderText = "User Session",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            });
            dgv.Columns.Add(new DataGridViewCheckBoxColumn()
            {
                Name = SYSTEM_SESSION_COLUMN,
                DataPropertyName = "SystemSession",
                HeaderText = "System Session",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = GUID_COLUMN,
                DataPropertyName = "Uuid",
                Visible = false
            });

            dgv.DataSource = this.scriptList;
        }

        private void addBtn_Click(object sender, EventArgs e)
        {
            AddScript dlg = new AddScript();
            System.Windows.Forms.DialogResult result = dlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                Script scr = dlg.ScriptData;
                this.scriptList.Add(scr);
            }
        }

        private void cancelBtn_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void okBtn_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Save();
            this.Close();
        }

        private void removeBtn_Click(object sender, EventArgs e)
        {
            if (this.scriptListDGV.SelectedRows.Count > 0)
            {
                this.scriptListDGV.Rows.Remove(this.scriptListDGV.SelectedRows[0]);
            }
        }

        private void Save()
        {
            Settings.Save(this.scriptList.ToList());
        }
    }
}
