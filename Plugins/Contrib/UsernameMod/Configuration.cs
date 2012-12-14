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

using pGina.Shared.Settings;
using pGina.Plugin.UsernameMod;
using pGina.Plugin.UsernameMod.Rules;

namespace pGina.Plugin.UsernameMod
{
    public partial class Configuration : Form
    {
        ListOfRules rules;

        public Configuration()
        {
            InitializeComponent();
            
            //Setup the drop down box
            actionBox.Items.AddRange(ListOfRules.Rules);
            this.actionBox.SelectedIndex = 0;
            
            //Get list of rules
            try
            {
                rules = new ListOfRules();
                rules.Load();
                
            }
            catch (UsernameModPluginException)
            {
                MessageBox.Show("Unable to load all rules from registry.");
            }
            updateListView();

            resetForm();
        }

        /// <summary>
        /// Run when the save button is hit. If an error is encountered due to improper input,
        /// a prompt will be shown and the save/exit will be cancelled.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave_Click(object sender, EventArgs e)
        {

            try
            {
                rules.Save();
            }
            catch (UsernameModPluginException ex)
            {
                MessageBox.Show("Unable to save settings.\n{0}", ex.Message);
                return;
            }
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        /// <summary>
        /// Discards any changes without saving.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        /// <summary>
        /// Executed when the Action drop down box (rule list) is changed.
        /// Changes the forms.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dropDownActionChange(object sender, EventArgs e)
        {
            resetForm();
        }

        /// <summary>
        /// Validates input and adds the rule to the ListOfRules.
        /// Displays warning if validation fails.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAddRule_Click(object sender, EventArgs e)
        {
            Stage stage;
            if (authButton.Checked)
                stage = Stage.Authentication;
            else if (authZButton.Checked)
                stage = Stage.Authorization;
            else if (gatewayButton.Checked)
                stage = Stage.Gateway;
            else
            {
                MessageBox.Show("Authentication, Authorization or Gateway button must be checked.");
                return;
            }

            string action = (string)actionBox.SelectedItem;
            string val1 = textBox1.Text;
            string val2 = (textBox2.Enabled ? textBox2.Text : null);

            try
            {
                IUsernameRule rule = ListOfRules.CreateRule(stage, action, val1, val2);
                rules.Add(rule);
                updateListView();
            }
            catch (UsernameModPluginException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// If a rule is selected in the rule list, it is removed from the ListOfRules,
        /// as well as ListView
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRemRule_Click(object sender, EventArgs e)
        {
            ListView.SelectedListViewItemCollection items = rulesListView.SelectedItems;
            if (items.Count > 0)
            {
                ListViewItem lvi = items[0];
                int index = lvi.Index;
                rules.remove(index);
                rulesListView.Items.RemoveAt(index);
            }
            
        }

        /// <summary>
        /// Called when either the up or down arrow is pressed. Determines which and updates
        /// the ListOfRules and ListView accordingly.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMove_Click(object sender, EventArgs e)
        {
            ListView.SelectedListViewItemCollection items = rulesListView.SelectedItems;
            if (items.Count > 0)
            {
                ListViewItem lvi = items[0];
                int oldIndex = lvi.Index;
                if (sender == upButton && rules.MoveUp(lvi.Index))
                {
                    string temp = lvi.Text;
                    lvi.Text = rulesListView.Items[oldIndex - 1].Text;
                    rulesListView.Items[oldIndex - 1].Text = temp;
                    rulesListView.Focus();
                    rulesListView.Items[oldIndex - 1].Selected = true;
                }

                else if (sender == downButton && rules.MoveDown(lvi.Index))
                {
                    string temp = lvi.Text;
                    lvi.Text = rulesListView.Items[oldIndex + 1].Text;
                    rulesListView.Items[oldIndex + 1].Text = temp;
                    rulesListView.Focus();
                    rulesListView.Items[oldIndex + 1].Selected = true;
                }
            }
        }

        /// <summary>
        /// Resets the forms based on the action dropdown box (list of rules)
        /// </summary>
        private void resetForm()
        {
            //Append, Prepend, Truncate, Replace, RegEx Replace, Match
            switch ((string)actionBox.SelectedItem)
            {
                case "Append":
                    setForm("Append the following:", null);
                    break;
                case "Prepend":
                    setForm("Prepend the following:", null);
                    break;
                case "Truncate":
                    setForm("Number of characters allowed:", null);
                    break;
                case "Replace":
                    setForm("Replace the following characters:", "With this string:");
                    break;
                case "RegEx Replace":
                    setForm("Replace the regex expression:", "With this string:");
                    break;
                case "Match":
                    setForm("Require the username to match:", null);
                    break;
            } 
        }

        private void setForm(string label1, string label2){
            descLabel1.Text = label1;
            textBox1.Text = "";

            if (label2 != null)
            {
                descLabel2.Show();
                textBox2.Show();
                descLabel2.Text = label2;
                textBox2.Text = "";
            }

            else
            {
                descLabel2.Hide();
                textBox2.Hide();
            }
        }

        /// <summary>
        /// Clears and reloads the list of rules.
        /// </summary>
        private void updateListView()
        {
            rulesListView.Items.Clear();
            foreach (IUsernameRule rule in rules.list)
            {
                ListViewItem lvi = new ListViewItem(rule.ToString());
                rulesListView.Items.Add(lvi);
            }
        }
    }
}
