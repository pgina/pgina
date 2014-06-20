/*
	Copyright (c) 2013, pGina Team
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

using MySql.Data;
using MySql.Data.MySqlClient;

namespace pGina.Plugin.MySQLAuth
{
    public partial class Configuration : Form
    {

        private log4net.ILog m_logger = log4net.LogManager.GetLogger("MySQLAuth Configuration");

        public Configuration()
        {
            InitializeComponent();
            InitUI();
        }

        private void InitUI()
        {
            this.hostTB.Text = Settings.Store.Host;
            int port = Settings.Store.Port;
            this.portTB.Text = Convert.ToString(port);
            this.userTB.Text = Settings.Store.User;
            this.passwordTB.Text = Settings.Store.GetEncryptedSetting("Password");
            this.dbTB.Text = Settings.Store.Database;
            bool useSsl = Settings.Store.UseSsl;
            this.useSslCB.Checked = useSsl;

            // User table schema settings
            this.userTableTB.Text = Settings.Store.Table;
            this.unameColTB.Text = Settings.Store.UsernameColumn;
            this.hashMethodColTB.Text = Settings.Store.HashMethodColumn;
            this.passwdColTB.Text = Settings.Store.PasswordColumn;
            this.userPrimaryKeyColTB.Text = Settings.Store.UserTablePrimaryKeyColumn;

            int encodingInt = Settings.Store.HashEncoding;
            Settings.HashEncoding encoding = (Settings.HashEncoding)encodingInt;

            if (encoding == Settings.HashEncoding.HEX)
                this.encHexRB.Checked = true;
            else
                this.encBase64RB.Checked = true;

            // Group table schema settings
            this.groupTableNameTB.Text = Settings.Store.GroupTableName;
            this.groupNameColTB.Text = Settings.Store.GroupNameColumn;
            this.groupTablePrimaryKeyColTB.Text = Settings.Store.GroupTablePrimaryKeyColumn;

            // User-Group table settings
            this.userGroupTableNameTB.Text = Settings.Store.UserGroupTableName;
            this.userGroupUserFKColTB.Text = Settings.Store.UserForeignKeyColumn;
            this.userGroupGroupFKColTB.Text = Settings.Store.GroupForeignKeyColumn;

            /////////////// Authorization tab /////////////////
            this.cbAuthzMySqlGroupMemberOrNot.SelectedIndex = 0;
            this.cbAuthzGroupRuleAllowOrDeny.SelectedIndex = 0;

            this.ckDenyWhenMySqlAuthFails.Checked = Settings.Store.AuthzRequireMySqlAuth;

            List<GroupAuthzRule> lst = GroupRuleLoader.GetAuthzRules();
            // The last one should be the default rule
            if (lst.Count > 0 &&
                lst[lst.Count - 1].RuleCondition == GroupRule.Condition.ALWAYS)
            {
                GroupAuthzRule rule = lst[lst.Count - 1];
                if (rule.AllowOnMatch)
                    this.rbDefaultAllow.Checked = true;
                else
                    this.rbDefaultDeny.Checked = true;
                lst.RemoveAt(lst.Count - 1);
            }
            else
            {
                // The list is empty or the last rule is not a default rule.
                throw new Exception("Default rule not found in rule list.");
            }
            // The rest of the rules
            foreach (GroupAuthzRule rule in lst)
                this.listBoxAuthzRules.Items.Add(rule);

            ///////////////// Gateway tab ///////////////
            List<GroupGatewayRule> gwLst = GroupRuleLoader.GetGatewayRules();
            foreach (GroupGatewayRule rule in gwLst)
                this.gtwRulesListBox.Items.Add(rule);
            this.gtwRuleConditionCB.SelectedIndex = 0;

            this.m_preventLogonWhenServerUnreachableCb.Checked = Settings.Store.PreventLogonOnServerError;
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            if (Save())
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private bool Save()
        {
            int port = 0;
            try
            {
                port = Convert.ToInt32(this.portTB.Text);
            }
            catch (Exception)
            {
                MessageBox.Show("The port must be a positive integer.");
                return false;
            }

            Settings.Store.Host = this.hostTB.Text.Trim();
            Settings.Store.Port = port;
            Settings.Store.User = this.userTB.Text.Trim();
            Settings.Store.SetEncryptedSetting("Password", this.passwordTB.Text);
            Settings.Store.Database = this.dbTB.Text.Trim();
            Settings.Store.UseSsl = this.useSslCB.Checked;

            // User table settings
            Settings.Store.Table = this.userTableTB.Text.Trim();
            Settings.Store.UsernameColumn = this.unameColTB.Text.Trim();
            Settings.Store.HashMethodColumn = this.hashMethodColTB.Text.Trim();
            Settings.Store.PasswordColumn = this.passwdColTB.Text.Trim();
            Settings.Store.UserTablePrimaryKeyColumn = this.userPrimaryKeyColTB.Text.Trim();

            if (encHexRB.Checked)
                Settings.Store.HashEncoding = (int)Settings.HashEncoding.HEX;
            else
                Settings.Store.HashEncoding = (int)Settings.HashEncoding.BASE_64;

            // Group table schema settings
            Settings.Store.GroupTableName = this.groupTableNameTB.Text.Trim();
            Settings.Store.GroupNameColumn = this.groupNameColTB.Text.Trim();
            Settings.Store.GroupTablePrimaryKeyColumn = this.groupTablePrimaryKeyColTB.Text.Trim();

            // User-Group table settings
            Settings.Store.UserGroupTableName = this.userGroupTableNameTB.Text.Trim();
            Settings.Store.UserForeignKeyColumn = this.userGroupUserFKColTB.Text.Trim();
            Settings.Store.GroupForeignKeyColumn = this.userGroupGroupFKColTB.Text.Trim();

            ////////// Authorization Tab ////////////
            Settings.Store.AuthzRequireMySqlAuth = this.ckDenyWhenMySqlAuthFails.Checked;
            List<GroupAuthzRule> lst = new List<GroupAuthzRule>();
            foreach (Object item in this.listBoxAuthzRules.Items)
            {
                lst.Add(item as GroupAuthzRule);
                m_logger.DebugFormat("Saving rule: {0}", item);
            }
            // Add the default as the last rule in the list
            lst.Add(new GroupAuthzRule(this.rbDefaultAllow.Checked));

            GroupRuleLoader.SaveAuthzRules(lst);

            // Gateway rules
            List<GroupGatewayRule> gwList = new List<GroupGatewayRule>();
            foreach (Object item in this.gtwRulesListBox.Items)
            {
                gwList.Add(item as GroupGatewayRule);
            }
            GroupRuleLoader.SaveGatewayRules(gwList);

            Settings.Store.PreventLogonOnServerError = m_preventLogonWhenServerUnreachableCb.Checked;

            return true;
        }

        private void passwdCB_CheckedChanged(object sender, EventArgs e)
        {
            this.passwordTB.UseSystemPasswordChar = !this.passwdCB.Checked;
        }

        private void testBtn_Click(object sender, EventArgs e)
        {
            TextBoxInfoDialog infoDlg = new TextBoxInfoDialog();
            infoDlg.Show();

            infoDlg.AppendLine("Beginning test of MySQL database..." + Environment.NewLine);
            MySqlConnection conn = null;
            string tableName = this.userTableTB.Text.Trim();
            try
            {
                string connStr = this.BuildConnectionString();
                if (connStr == null) return;
                
                infoDlg.AppendLine("Connection Status");
                infoDlg.AppendLine("-------------------------------------");

                conn = new MySqlConnection(connStr);
                conn.Open();

                infoDlg.AppendLine(string.Format("Connection to {0} successful.", this.hostTB.Text.Trim()));

                // Variables to be used repeatedly below
                MySqlCommand cmd = null;
                MySqlDataReader rdr = null;
                string query = "";

                // Check SSL status
                if (useSslCB.Checked)
                {
                    string cipher = "";
                    query = "SHOW STATUS LIKE 'Ssl_cipher'";
                    cmd = new MySqlCommand(query, conn);
                    rdr = cmd.ExecuteReader();
                    if (rdr.HasRows)
                    {
                        rdr.Read();
                        cipher = rdr[1].ToString();
                    }
                    rdr.Close();

                    if (string.IsNullOrEmpty(cipher))
                    {
                        infoDlg.AppendLine( "Not using SSL." );
                    }
                    else
                    {
                        infoDlg.AppendLine("SSL enabled, using cipher: " + cipher);
                    }
                }
                else
                {
                    infoDlg.AppendLine( "Not using SSL." );
                }

                infoDlg.AppendLine( Environment.NewLine + "User Table" );
                infoDlg.AppendLine( "-------------------------------");
                CheckTable(tableName,
                    new string[] { this.unameColTB.Text.Trim(), this.passwdColTB.Text.Trim(), this.hashMethodColTB.Text.Trim(), this.userPrimaryKeyColTB.Text.Trim() },
                    infoDlg, conn);

                infoDlg.AppendLine(Environment.NewLine + "Group Table");
                infoDlg.AppendLine("-------------------------------");
                CheckTable(this.groupTableNameTB.Text.Trim(),
                    new string[] { this.groupNameColTB.Text.Trim(), this.groupTablePrimaryKeyColTB.Text.Trim() },
                    infoDlg, conn);

                infoDlg.AppendLine(Environment.NewLine + "User-Group Table");
                infoDlg.AppendLine("-------------------------------");
                CheckTable(this.userGroupTableNameTB.Text.Trim(),
                    new string[] { this.userGroupUserFKColTB.Text.Trim(), this.userGroupGroupFKColTB.Text.Trim() },
                    infoDlg, conn);
            }
            catch (Exception ex)
            {
                if (ex is MySqlException)
                {
                    MySqlException mysqlEx = ex as MySqlException;
                    infoDlg.AppendLine("MySQL ERROR: " + mysqlEx.Message);
                }
                else
                {
                    infoDlg.AppendLine(string.Format("ERROR: A fatal error occured: {0}", ex));
                }
            }
            finally
            {
                infoDlg.AppendLine(Environment.NewLine + "Closing connection.");
                if (conn != null)
                    conn.Close();
                infoDlg.AppendLine("Test complete.");
            }
        }

        private void CheckTable(string tableName, string[] columnNames, TextBoxInfoDialog infoDlg, MySqlConnection conn)
        {
            // Check for existence of the table
            bool tableExists = this.TableExists(tableName, conn);
            if (tableExists)
            {
                m_logger.DebugFormat("Table \"{0}\" found.", tableName);
                infoDlg.AppendLine(string.Format("Table \"{0}\" found.", tableName));
            }
            else
            {
                m_logger.DebugFormat("Table {0} not found.", tableName);
                infoDlg.AppendLine(string.Format("ERROR: Table \"{0}\" not found.", tableName));
                return;
            }

            if (tableExists)
            {
                // Get column names from DB
                List<string> columnNamesFromDB = new List<string>();
                string query = string.Format("DESCRIBE {0}", tableName);
                MySqlCommand cmd = new MySqlCommand(query, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    string colName = rdr[0].ToString();
                    columnNamesFromDB.Add(colName);
                }
                rdr.Close();

                // Check for appropriate columns.
                bool ok = true;
                foreach (string c in columnNames)
                {
                    if (columnNamesFromDB.Contains(c, StringComparer.CurrentCultureIgnoreCase))
                        infoDlg.AppendLine(string.Format("Found column \"{0}\"", c));
                    else
                    {
                        ok = false;
                        infoDlg.AppendLine(string.Format("ERROR: Column \"{0}\" not found!", c));
                    }
                }

                if (!ok)
                    infoDlg.AppendLine(string.Format("ERROR: Table \"{0}\" schema looks incorrect.", tableName));
            }
        }

        private void createTableBtn_Click(object sender, EventArgs e)
        {
            string connStr = this.BuildConnectionString();
            if (connStr == null) return;

            TextBoxInfoDialog infoDlg = new TextBoxInfoDialog();
            infoDlg.ClearText();
            infoDlg.Show();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    infoDlg.AppendLine("Connecting...");
                    conn.Open();

                    // User table
                    string tableName = this.userTableTB.Text.Trim();
                    infoDlg.AppendLine(Environment.NewLine +
                        string.Format("Creating table \"{0}\"", tableName));

                    if (!this.TableExists(tableName, conn))
                    {
                        // Column names
                        string pk = this.userPrimaryKeyColTB.Text.Trim();
                        string unameCol = this.unameColTB.Text.Trim();
                        string hashMethodCol = this.hashMethodColTB.Text.Trim();
                        string passwdCol = this.passwdColTB.Text.Trim();

                        // Is the primary key the same as the username?
                        bool pkIsUserName =
                            unameCol.Equals(pk, StringComparison.CurrentCultureIgnoreCase);

                        StringBuilder sql = new StringBuilder();
                        sql.AppendFormat("CREATE TABLE {0} ( \r\n", tableName);
                        if (!pkIsUserName)
                            sql.AppendFormat(" {0} BIGINT auto_increment PRIMARY KEY, \r\n", pk);
                        sql.AppendFormat(" {0} VARCHAR(128) {1}, \r\n", unameCol, pkIsUserName ? "PRIMARY KEY" : "NOT NULL UNIQUE");
                        sql.AppendFormat(" {0} TEXT NOT NULL, \r\n", hashMethodCol);
                        sql.AppendFormat(" {0} TEXT \r\n", passwdCol);
                        sql.Append(")");  // End create table.

                        infoDlg.AppendLine("Executing SQL:");
                        infoDlg.AppendLine(sql.ToString());

                        using (MySqlCommand cmd = new MySqlCommand(sql.ToString(), conn))
                        {
                            cmd.ExecuteNonQuery();
                            infoDlg.AppendLine(string.Format("Table \"{0}\" created.", tableName));
                        }
                    }
                    else
                    {
                        infoDlg.AppendLine(
                            string.Format("WARNING: Table \"{0}\"already exists, skipping.", tableName));
                    }

                    // Group table
                    tableName = this.groupTableNameTB.Text.Trim();
                    infoDlg.AppendLine(Environment.NewLine +
                        string.Format("Creating table \"{0}\"", tableName));

                    if (!this.TableExists(tableName, conn))
                    {
                        // Column names
                        string pk = this.groupTablePrimaryKeyColTB.Text.Trim();
                        string groupNameCol = this.groupNameColTB.Text.Trim();

                        // Is the primary key the same as the group name?
                        bool pkIsGroupName =
                            groupNameCol.Equals(pk, StringComparison.CurrentCultureIgnoreCase);

                        StringBuilder sql = new StringBuilder();
                        sql.AppendFormat("CREATE TABLE {0} ( \r\n", tableName);
                        if (!pkIsGroupName)
                            sql.AppendFormat(" {0} BIGINT AUTO_INCREMENT PRIMARY KEY, \r\n", pk);
                        sql.AppendFormat(" {0} VARCHAR(128) {1} \r\n", groupNameCol, pkIsGroupName ? "PRIMARY KEY" : "NOT NULL UNIQUE");
                        sql.Append(")");  // End create table.

                        infoDlg.AppendLine("Executing SQL:");
                        infoDlg.AppendLine(sql.ToString());

                        using (MySqlCommand cmd = new MySqlCommand(sql.ToString(), conn))
                        {
                            cmd.ExecuteNonQuery();
                            infoDlg.AppendLine(string.Format("Table \"{0}\" created.", tableName));
                        }
                    }
                    else
                    {
                        infoDlg.AppendLine(
                            string.Format("WARNING: Table \"{0}\"already exists, skipping.", tableName));
                    }

                    // user-Group table
                    tableName = this.userGroupTableNameTB.Text.Trim();
                    infoDlg.AppendLine(Environment.NewLine +
                        string.Format("Creating table \"{0}\"", tableName));

                    if (!this.TableExists(tableName, conn))
                    {
                        // Column names
                        string userFK = this.userGroupUserFKColTB.Text.Trim();
                        string userPK = this.userPrimaryKeyColTB.Text.Trim();
                        string groupFK = this.userGroupGroupFKColTB.Text.Trim();
                        string groupPK = this.groupTablePrimaryKeyColTB.Text.Trim();

                        string groupNameCol = this.groupNameColTB.Text.Trim();
                        string unameCol = this.unameColTB.Text.Trim();

                        // Is the primary key the same as the group name?
                        bool pkIsGroupName =
                            groupNameCol.Equals(groupPK, StringComparison.CurrentCultureIgnoreCase);
                        bool pkIsUserName =
                            unameCol.Equals(userPK, StringComparison.CurrentCultureIgnoreCase);

                        StringBuilder sql = new StringBuilder();
                        sql.AppendFormat("CREATE TABLE {0} ( \r\n", tableName);
                        sql.AppendFormat(" {0} {1}, \r\n", groupFK, pkIsGroupName ? "VARCHAR(128)" : "BIGINT");
                        sql.AppendFormat(" {0} {1}, \r\n", userFK, pkIsUserName ? "VARCHAR(128)" : "BIGINT");
                        sql.AppendFormat(" PRIMARY KEY ({0}, {1}) \r\n", userFK, groupFK);
                        sql.Append(")");  // End create table.

                        infoDlg.AppendLine("Executing SQL:");
                        infoDlg.AppendLine(sql.ToString());

                        MySqlCommand cmd = new MySqlCommand(sql.ToString(), conn);
                        cmd.ExecuteNonQuery();
                        infoDlg.AppendLine(string.Format("Table \"{0}\" created.", tableName));
                    }
                    else
                    {
                        infoDlg.AppendLine(
                            string.Format("WARNING: Table \"{0}\"already exists, skipping.", tableName));
                    }

                }
            }
            catch (MySqlException ex)
            {
                infoDlg.AppendLine(String.Format("ERROR: {0}", ex.Message));
            }
            finally
            {
                infoDlg.AppendLine(Environment.NewLine + "Finished.");
            }
        }

        private bool TableExists(string tableName, MySqlConnection conn)
        {
            string query = "SHOW TABLES LIKE @table";
            MySqlCommand cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@table", tableName);
            MySqlDataReader rdr = cmd.ExecuteReader();
            bool tableExists = rdr.HasRows;
            rdr.Close();
            return tableExists;
        }

        private string BuildConnectionString()
        {
            uint port = 0;
            try
            {
                port = Convert.ToUInt32(this.portTB.Text);
            }
            catch (FormatException)
            {
                MessageBox.Show("Invalid port number.");
                return null;
            }

            MySqlConnectionStringBuilder bldr = new MySqlConnectionStringBuilder();
            bldr.Server = this.hostTB.Text.Trim();
            bldr.Port = port;
            bldr.UserID = this.userTB.Text.Trim();
            bldr.Database = this.dbTB.Text.Trim();
            bldr.Password = this.passwordTB.Text;

            if (this.useSslCB.Checked)
            {
                bldr.SslMode = MySqlSslMode.Required;
            }

            return bldr.GetConnectionString(true);
        }

        private void encHexRB_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }

        private void gtwRuleAddBtn_Click(object sender, EventArgs e)
        {
            string localGrp = this.gtwRuleLocalGroupTB.Text.Trim();
            if (string.IsNullOrEmpty(localGrp))
            {
                MessageBox.Show("Please enter a local group name");
                return;
            }
            int idx = this.gtwRuleConditionCB.SelectedIndex;
            GroupRule.Condition c;
            if (idx == 0) c = GroupRule.Condition.MEMBER_OF;
            else if (idx == 1) c = GroupRule.Condition.NOT_MEMBER_OF;
            else
                throw new Exception("Unrecognized option in gtwRuleAddBtn_Click");

            if (c == GroupRule.Condition.ALWAYS)
            {
                this.gtwRulesListBox.Items.Add(new GroupGatewayRule(localGrp));
            }
            else
            {
                string remoteGroup = this.gtwRuleMysqlGroupTB.Text.Trim();
                if (string.IsNullOrEmpty(remoteGroup))
                {
                    MessageBox.Show("Please enter a remote group name");
                    return;
                }
                this.gtwRulesListBox.Items.Add(new GroupGatewayRule(remoteGroup, c, localGrp));
            }
        }

        private void gtwRuleConditionCB_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void gtwRuleDeleteBtn_Click(object sender, EventArgs e)
        {
            int idx = this.gtwRulesListBox.SelectedIndex;
            if (idx >= 0 && idx < this.gtwRulesListBox.Items.Count)
                this.gtwRulesListBox.Items.RemoveAt(idx);
        }

        private void btnAuthzGroupRuleAdd_Click(object sender, EventArgs e)
        {
            string grp = this.tbAuthzRuleGroup.Text.Trim();
            if (string.IsNullOrEmpty(grp))
            {
                MessageBox.Show("Please enter a group name.");
                return;
            }

            int idx = this.cbAuthzMySqlGroupMemberOrNot.SelectedIndex;
            GroupRule.Condition c;
            if (idx == 0) c = GroupRule.Condition.MEMBER_OF;
            else if (idx == 1) c = GroupRule.Condition.NOT_MEMBER_OF;
            else
                throw new Exception("Unrecognized option in authzRuleAddButton_Click");


            idx = this.cbAuthzGroupRuleAllowOrDeny.SelectedIndex;
            bool allow;
            if (idx == 0) allow = true;          // allow
            else if (idx == 1) allow = false;    // deny
            else
                throw new Exception("Unrecognized action option in authzRuleAddButton_Click");

            GroupAuthzRule rule = new GroupAuthzRule(grp, c, allow);
            this.listBoxAuthzRules.Items.Add(rule);
        }

        private void btnAuthzGroupRuleUp_Click(object sender, EventArgs e)
        {
            int idx = this.listBoxAuthzRules.SelectedIndex;
            if (idx > 0 && idx < this.listBoxAuthzRules.Items.Count)
            {
                object item = this.listBoxAuthzRules.Items[idx];
                this.listBoxAuthzRules.Items.RemoveAt(idx);
                this.listBoxAuthzRules.Items.Insert(idx - 1, item);
                this.listBoxAuthzRules.SelectedIndex = idx - 1;
            }
        }

        private void btnAuthzGroupRuleDelete_Click(object sender, EventArgs e)
        {
            int idx = this.listBoxAuthzRules.SelectedIndex;
            if (idx >= 0 && idx < this.listBoxAuthzRules.Items.Count)
                this.listBoxAuthzRules.Items.RemoveAt(idx);
        }

        private void btnAuthzGroupRuleDown_Click(object sender, EventArgs e)
        {
            int idx = this.listBoxAuthzRules.SelectedIndex;
            if (idx >= 0 && idx < this.listBoxAuthzRules.Items.Count - 1)
            {
                object item = this.listBoxAuthzRules.Items[idx];
                this.listBoxAuthzRules.Items.RemoveAt(idx);
                this.listBoxAuthzRules.Items.Insert(idx + 1, item);
                this.listBoxAuthzRules.SelectedIndex = idx + 1;
            }
        }
    }
}
