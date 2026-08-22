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
/*
	OpenCredential fork note:
	Based on original pGina source code.
	Further modifications Copyright (c) 2024, Pedro Bermudez.

	This file is part of OpenCredential, an unofficial fork of pGina.
	It remains subject to the BSD-3-Clause license terms of the original
	pGina project. See LICENSE and NOTICE for details.
*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using MySqlConnector;
using Npgsql;

namespace OpenCredential.Plugin.DatabaseAuth
{
    public partial class Configuration : Form
    {
        private ComboBox m_providerCB;
        private Label m_providerLabel;
        private bool m_hasStoredPassword;

        private log4net.ILog m_logger = log4net.LogManager.GetLogger("DatabaseAuth Configuration");

        public Configuration()
        {
            InitializeComponent();
            InitializeProviderControls();
            InitUI();
        }

        private void InitializeProviderControls()
        {
            m_providerLabel = new Label();
            m_providerLabel.AutoSize = true;
            m_providerLabel.Location = new Point(284, 49);
            m_providerLabel.Name = "providerLabel";
            m_providerLabel.Size = new Size(49, 13);
            m_providerLabel.Text = "Provider:";

            m_providerCB = new ComboBox();
            m_providerCB.DropDownStyle = ComboBoxStyle.DropDownList;
            m_providerCB.FormattingEnabled = true;
            m_providerCB.Location = new Point(339, 46);
            m_providerCB.Name = "providerCB";
            m_providerCB.Size = new Size(159, 21);
            m_providerCB.Items.Add(Settings.DatabaseProvider.MySql.ToString());
            m_providerCB.Items.Add(Settings.DatabaseProvider.PostgreSql.ToString());
            m_providerCB.SelectedIndexChanged += providerCB_SelectedIndexChanged;

            this.groupBox1.Controls.Add(m_providerLabel);
            this.groupBox1.Controls.Add(m_providerCB);
        }

        private void InitUI()
        {
            this.hostTB.Text = Convert.ToString(Settings.Store.Host);
            this.portTB.Text = Convert.ToString(Settings.GetPort());
            this.userTB.Text = Convert.ToString(Settings.Store.User);
            string storedPassword = Settings.Store.GetEncryptedSetting("Password");
            m_hasStoredPassword = !string.IsNullOrEmpty(storedPassword);
            this.passwordTB.Text = string.Empty;
            this.passwordTB.UseSystemPasswordChar = true;
            this.passwdCB.Checked = false;
            this.passwdCB.Visible = false;
            this.dbTB.Text = Convert.ToString(Settings.Store.Database);
            this.m_providerCB.SelectedItem = Settings.GetDatabaseProvider().ToString();
            this.sslModeCB.SelectedItem = Settings.GetSslMode().ToString();
            this.localCacheEnabledCB.Checked = Settings.IsLocalCacheEnabled();
            this.offlineFallbackEnabledCB.Checked = Settings.IsOfflineFallbackEnabled();
            this.offlineBypassCB.Checked = Settings.AllowOfflineBypassForAuthorization();
            this.syncIntervalTB.Text = Convert.ToString(Settings.GetSyncIntervalMinutes());
            this.healthCheckTB.Text = Convert.ToString(Settings.GetHealthCheckSeconds());
            this.cachePathTB.Text = Settings.GetLocalCachePath();
            this.enforceStatusCB.Checked = Settings.IsUserStatusValidationEnabled();

            // User table schema settings
            this.userTableTB.Text = Convert.ToString(Settings.Store.Table);
            this.unameColTB.Text = Convert.ToString(Settings.Store.UsernameColumn);
            this.hashMethodColTB.Text = Convert.ToString(Settings.Store.HashMethodColumn);
            this.passwdColTB.Text = Convert.ToString(Settings.Store.PasswordColumn);
            this.userPrimaryKeyColTB.Text = Convert.ToString(Settings.Store.UserTablePrimaryKeyColumn);
            this.statusColTB.Text = Settings.GetUserStatusColumn();
            this.activeValueTB.Text = Settings.GetUserActiveValue();
            this.lockoutEnabledCB.Checked = Settings.IsLoginLockoutEnabled();
            this.failedAttemptsColTB.Text = Settings.GetFailedAttemptsColumn();
            this.blockedUntilColTB.Text = Settings.GetBlockedUntilColumn();
            this.lastAttemptColTB.Text = Settings.GetLastAttemptColumn();
            this.maxFailedAttemptsTB.Text = Convert.ToString(Settings.GetMaxFailedAttempts());
            this.lockoutMinutesTB.Text = Convert.ToString(Settings.GetLockoutMinutes());

            Settings.HashEncoding encoding = Settings.GetHashEncoding();

            if (encoding == Settings.HashEncoding.HEX)
                this.encHexRB.Checked = true;
            else
                this.encBase64RB.Checked = true;

            // Group table schema settings
            this.groupTableNameTB.Text = Convert.ToString(Settings.Store.GroupTableName);
            this.groupNameColTB.Text = Convert.ToString(Settings.Store.GroupNameColumn);
            this.groupTablePrimaryKeyColTB.Text = Convert.ToString(Settings.Store.GroupTablePrimaryKeyColumn);

            // User-Group table settings
            this.userGroupTableNameTB.Text = Convert.ToString(Settings.Store.UserGroupTableName);
            this.userGroupUserFKColTB.Text = Convert.ToString(Settings.Store.UserForeignKeyColumn);
            this.userGroupGroupFKColTB.Text = Convert.ToString(Settings.Store.GroupForeignKeyColumn);

            /////////////// Authorization tab /////////////////
            this.cbAuthzMySqlGroupMemberOrNot.SelectedIndex = 0;
            this.cbAuthzGroupRuleAllowOrDeny.SelectedIndex = 0;

            this.ckDenyWhenMySqlAuthFails.Checked = Settings.IsAuthzRequireMySqlAuth();

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

            this.m_preventLogonWhenServerUnreachableCb.Checked = Settings.PreventLogonOnServerError();
            UpdateProviderUi();
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
            int syncIntervalMinutes = 0;
            int healthCheckSeconds = 0;
            int maxFailedAttempts = Settings.GetMaxFailedAttempts();
            int lockoutMinutes = Settings.GetLockoutMinutes();
            try
            {
                port = Convert.ToInt32(this.portTB.Text);
            }
            catch (Exception)
            {
                MessageBox.Show("The port must be a positive integer.");
                return false;
            }

            try
            {
                syncIntervalMinutes = Convert.ToInt32(this.syncIntervalTB.Text);
                healthCheckSeconds = Convert.ToInt32(this.healthCheckTB.Text);
            }
            catch (Exception)
            {
                MessageBox.Show("Sync interval and health check must be positive integers.");
                return false;
            }

            if (syncIntervalMinutes < 1)
            {
                MessageBox.Show("Sync interval must be at least 1 minute.");
                return false;
            }

            if (healthCheckSeconds < 5)
            {
                MessageBox.Show("Health check must be at least 5 seconds.");
                return false;
            }

            if (this.lockoutEnabledCB.Checked)
            {
                try
                {
                    maxFailedAttempts = Convert.ToInt32(this.maxFailedAttemptsTB.Text);
                    lockoutMinutes = Convert.ToInt32(this.lockoutMinutesTB.Text);
                }
                catch (Exception)
                {
                    MessageBox.Show("Login lockout values must be positive integers.");
                    return false;
                }

                if (maxFailedAttempts < 1 || lockoutMinutes < 1)
                {
                    MessageBox.Show("Login lockout values must be at least 1.");
                    return false;
                }
            }

            if (this.enforceStatusCB.Checked &&
                (string.IsNullOrWhiteSpace(this.statusColTB.Text) || string.IsNullOrWhiteSpace(this.activeValueTB.Text)))
            {
                MessageBox.Show("User status validation requires both status column and active value.");
                return false;
            }

            if (this.lockoutEnabledCB.Checked &&
                (string.IsNullOrWhiteSpace(this.failedAttemptsColTB.Text) ||
                 string.IsNullOrWhiteSpace(this.blockedUntilColTB.Text) ||
                 string.IsNullOrWhiteSpace(this.lastAttemptColTB.Text)))
            {
                MessageBox.Show("Login lockout requires attempts, blocked-until and last-attempt columns.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(this.passwordTB.Text) && !m_hasStoredPassword)
            {
                MessageBox.Show("Please enter a database password.");
                return false;
            }

            MySqlSslMode sslMode;
            if (!Enum.TryParse(this.sslModeCB.Text, true, out sslMode))
            {
                MessageBox.Show("Please select a valid TLS mode.");
                return false;
            }

            Settings.DatabaseProvider provider;
            if (!Enum.TryParse(Convert.ToString(this.m_providerCB.SelectedItem), out provider))
            {
                MessageBox.Show("Please select a valid database provider.");
                return false;
            }

            Settings.Store.Host = this.hostTB.Text.Trim();
            Settings.Store.Port = port;
            Settings.Store.DatabaseProvider = (int)provider;
            Settings.Store.User = this.userTB.Text.Trim();
            string newPassword = this.passwordTB.Text;
            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                Settings.Store.SetEncryptedSetting("Password", newPassword);
                m_hasStoredPassword = true;
                this.passwordTB.Text = string.Empty;
            }
            Settings.Store.Database = this.dbTB.Text.Trim();
            Settings.Store.LocalCacheEnabled = this.localCacheEnabledCB.Checked;
            Settings.Store.OfflineFallbackEnabled = this.offlineFallbackEnabledCB.Checked;
            Settings.Store.AllowOfflineBypassForAuthorization = this.offlineBypassCB.Checked;
            Settings.Store.SyncIntervalMinutes = syncIntervalMinutes;
            Settings.Store.HealthCheckSeconds = healthCheckSeconds;
            Settings.Store.LocalCachePath = this.cachePathTB.Text.Trim();
            Settings.Store.SslMode = sslMode.ToString();
            Settings.Store.UseSsl = sslMode != MySqlSslMode.None;

            // User table settings
            Settings.Store.Table = this.userTableTB.Text.Trim();
            Settings.Store.UsernameColumn = this.unameColTB.Text.Trim();
            Settings.Store.HashMethodColumn = this.hashMethodColTB.Text.Trim();
            Settings.Store.PasswordColumn = this.passwdColTB.Text.Trim();
            Settings.Store.UserTablePrimaryKeyColumn = this.userPrimaryKeyColTB.Text.Trim();
            Settings.Store.EnforceUserStatus = this.enforceStatusCB.Checked;
            Settings.Store.UserStatusColumn = this.statusColTB.Text.Trim();
            Settings.Store.UserActiveValue = this.activeValueTB.Text.Trim();
            Settings.Store.EnableLoginLockout = this.lockoutEnabledCB.Checked;
            Settings.Store.FailedAttemptsColumn = this.failedAttemptsColTB.Text.Trim();
            Settings.Store.BlockedUntilColumn = this.blockedUntilColTB.Text.Trim();
            Settings.Store.LastAttemptColumn = this.lastAttemptColTB.Text.Trim();
            Settings.Store.MaxFailedAttempts = maxFailedAttempts;
            Settings.Store.LockoutMinutes = lockoutMinutes;

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

            if (GetSelectedDatabaseProvider() == Settings.DatabaseProvider.PostgreSql)
            {
                TestPostgreSql(infoDlg);
                return;
            }

            infoDlg.AppendLine(string.Format("Beginning test of {0} database...", GetSelectedProviderDisplayName()) + Environment.NewLine);
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
                if (Settings.GetSslMode() != MySqlSslMode.None)
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
                        infoDlg.AppendLine("TLS mode configured, but no cipher is active.");
                    }
                    else
                    {
                        infoDlg.AppendLine(string.Format("TLS mode: {0}, cipher: {1}", Settings.GetSslMode(), cipher));
                    }
                }
                else
                {
                    infoDlg.AppendLine("TLS disabled.");
                }

                infoDlg.AppendLine( Environment.NewLine + "User Table" );
                infoDlg.AppendLine( "-------------------------------");
                CheckTable(tableName,
                    BuildUserTableColumns(),
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

                infoDlg.AppendLine(Environment.NewLine + LocalUserCache.TestConfiguration());
            }
            catch (Exception ex)
            {
                if (ex is MySqlException)
                {
                    MySqlException mysqlEx = ex as MySqlException;
                    infoDlg.AppendLine("DATABASE ERROR: " + mysqlEx.Message);
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
            if (GetSelectedDatabaseProvider() == Settings.DatabaseProvider.PostgreSql)
            {
                CreatePostgreSqlTables();
                return;
            }

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
                        string statusCol = this.statusColTB.Text.Trim();
                        string failedAttemptsCol = this.failedAttemptsColTB.Text.Trim();
                        string blockedUntilCol = this.blockedUntilColTB.Text.Trim();
                        string lastAttemptCol = this.lastAttemptColTB.Text.Trim();

                        // Is the primary key the same as the username?
                        bool pkIsUserName =
                            unameCol.Equals(pk, StringComparison.CurrentCultureIgnoreCase);

                        StringBuilder sql = new StringBuilder();
                        sql.AppendFormat("CREATE TABLE `{0}` ( \r\n", tableName);
                        if (!pkIsUserName)
                            sql.AppendFormat(" `{0}` BIGINT AUTO_INCREMENT PRIMARY KEY, \r\n", pk);
                        sql.AppendFormat(" `{0}` VARCHAR(128) {1}, \r\n", unameCol, pkIsUserName ? "PRIMARY KEY" : "NOT NULL UNIQUE");
                        sql.AppendFormat(" `{0}` TEXT NOT NULL, \r\n", hashMethodCol);
                        if (this.enforceStatusCB.Checked)
                            sql.AppendFormat(" `{0}` VARCHAR(32) NOT NULL DEFAULT '{1}', \r\n", statusCol, this.activeValueTB.Text.Trim().Replace("'", "''"));
                        bool includeLockoutColumns = this.lockoutEnabledCB.Checked || this.IsStandardEnglishFullSchemaRequested();
                        if (includeLockoutColumns)
                        {
                            sql.AppendFormat(" `{0}` INT NOT NULL DEFAULT 0, \r\n", failedAttemptsCol);
                            sql.AppendFormat(" `{0}` DATETIME NULL, \r\n", blockedUntilCol);
                            sql.AppendFormat(" `{0}` DATETIME NULL, \r\n", lastAttemptCol);
                        }
                        sql.AppendFormat(" `{0}` TEXT \r\n", passwdCol);
                        sql.Append(") ENGINE=InnoDB DEFAULT CHARSET=utf8mb4");  // End create table.

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
                        sql.AppendFormat("CREATE TABLE `{0}` ( \r\n", tableName);
                        if (!pkIsGroupName)
                            sql.AppendFormat(" `{0}` BIGINT AUTO_INCREMENT PRIMARY KEY, \r\n", pk);
                        sql.AppendFormat(" `{0}` VARCHAR(128) {1} \r\n", groupNameCol, pkIsGroupName ? "PRIMARY KEY" : "NOT NULL UNIQUE");
                        sql.Append(") ENGINE=InnoDB DEFAULT CHARSET=utf8mb4");  // End create table.

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
                        sql.AppendFormat("CREATE TABLE `{0}` ( \r\n", tableName);
                        sql.AppendFormat(" `{0}` {1}, \r\n", groupFK, pkIsGroupName ? "VARCHAR(128)" : "BIGINT");
                        sql.AppendFormat(" `{0}` {1}, \r\n", userFK, pkIsUserName ? "VARCHAR(128)" : "BIGINT");
                        sql.AppendFormat(" PRIMARY KEY (`{0}`, `{1}`) \r\n", userFK, groupFK);
                        sql.Append(") ENGINE=InnoDB DEFAULT CHARSET=utf8mb4");  // End create table.

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

                    if (this.IsStandardEnglishFullSchemaRequested())
                    {
                        infoDlg.AppendLine(Environment.NewLine + "Extending standard English schema...");
                        this.EnsureEnglishReferenceTables(infoDlg, conn);
                        this.EnsureEnglishUserColumns(infoDlg, conn);
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

        private bool ColumnExists(string tableName, string columnName, MySqlConnection conn)
        {
            string query =
                "SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS " +
                "WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = @table AND COLUMN_NAME = @column";
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@table", tableName);
                cmd.Parameters.AddWithValue("@column", columnName);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        private bool IndexExists(string tableName, string indexName, MySqlConnection conn)
        {
            string query =
                "SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS " +
                "WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = @table AND INDEX_NAME = @index";
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@table", tableName);
                cmd.Parameters.AddWithValue("@index", indexName);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        private bool ForeignKeyExists(string tableName, string constraintName, MySqlConnection conn)
        {
            string query =
                "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS " +
                "WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = @table AND CONSTRAINT_NAME = @constraint";
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@table", tableName);
                cmd.Parameters.AddWithValue("@constraint", constraintName);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        private bool IsStandardEnglishFullSchemaRequested()
        {
            return
                this.userTableTB.Text.Trim().Equals("users", StringComparison.OrdinalIgnoreCase) &&
                this.groupTableNameTB.Text.Trim().Equals("groups", StringComparison.OrdinalIgnoreCase) &&
                this.userGroupTableNameTB.Text.Trim().Equals("user_groups", StringComparison.OrdinalIgnoreCase);
        }

        private void EnsureEnglishReferenceTables(TextBoxInfoDialog infoDlg, MySqlConnection conn)
        {
            if (!this.TableExists("careers", conn))
            {
                string sql =
                    "CREATE TABLE `careers` (" +
                    "`id` INT NOT NULL AUTO_INCREMENT, " +
                    "`name` VARCHAR(255) NOT NULL, " +
                    "`status` INT NOT NULL DEFAULT 1, " +
                    "PRIMARY KEY (`id`)) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4";
                infoDlg.AppendLine("Creating table \"careers\"");
                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            else
            {
                infoDlg.AppendLine("Table \"careers\" already exists.");
            }

            if (!this.TableExists("levels", conn))
            {
                string sql =
                    "CREATE TABLE `levels` (" +
                    "`id` INT NOT NULL AUTO_INCREMENT, " +
                    "`name` VARCHAR(100) NOT NULL, " +
                    "`status` INT NOT NULL DEFAULT 1, " +
                    "PRIMARY KEY (`id`)) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4";
                infoDlg.AppendLine("Creating table \"levels\"");
                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            else
            {
                infoDlg.AppendLine("Table \"levels\" already exists.");
            }
        }

        private void EnsureEnglishUserColumns(TextBoxInfoDialog infoDlg, MySqlConnection conn)
        {
            this.AddColumnIfMissing("users", "failed_attempts", "ALTER TABLE `users` ADD COLUMN `failed_attempts` INT NOT NULL DEFAULT 0", infoDlg, conn);
            this.AddColumnIfMissing("users", "locked_until", "ALTER TABLE `users` ADD COLUMN `locked_until` DATETIME NULL", infoDlg, conn);
            this.AddColumnIfMissing("users", "last_attempt_at", "ALTER TABLE `users` ADD COLUMN `last_attempt_at` DATETIME NULL", infoDlg, conn);
            this.AddColumnIfMissing("users", "first_name", "ALTER TABLE `users` ADD COLUMN `first_name` VARCHAR(100) NULL", infoDlg, conn);
            this.AddColumnIfMissing("users", "last_name", "ALTER TABLE `users` ADD COLUMN `last_name` VARCHAR(100) NULL", infoDlg, conn);
            this.AddColumnIfMissing("users", "document_id", "ALTER TABLE `users` ADD COLUMN `document_id` VARCHAR(15) NULL", infoDlg, conn);
            this.AddColumnIfMissing("users", "email", "ALTER TABLE `users` ADD COLUMN `email` VARCHAR(200) NULL", infoDlg, conn);
            this.AddColumnIfMissing("users", "career_id", "ALTER TABLE `users` ADD COLUMN `career_id` INT NULL", infoDlg, conn);
            this.AddColumnIfMissing("users", "level_id", "ALTER TABLE `users` ADD COLUMN `level_id` INT NULL", infoDlg, conn);

            this.AddIndexIfMissing("users", "idx_users_career_id", "ALTER TABLE `users` ADD INDEX `idx_users_career_id` (`career_id`)", infoDlg, conn);
            this.AddIndexIfMissing("users", "idx_users_level_id", "ALTER TABLE `users` ADD INDEX `idx_users_level_id` (`level_id`)", infoDlg, conn);

            this.AddForeignKeyIfMissing(
                "users",
                "fk_users_career",
                "ALTER TABLE `users` ADD CONSTRAINT `fk_users_career` FOREIGN KEY (`career_id`) REFERENCES `careers` (`id`) ON DELETE CASCADE ON UPDATE CASCADE",
                infoDlg,
                conn);

            this.AddForeignKeyIfMissing(
                "users",
                "fk_users_level",
                "ALTER TABLE `users` ADD CONSTRAINT `fk_users_level` FOREIGN KEY (`level_id`) REFERENCES `levels` (`id`) ON DELETE CASCADE ON UPDATE CASCADE",
                infoDlg,
                conn);
        }

        private void AddColumnIfMissing(string tableName, string columnName, string sql, TextBoxInfoDialog infoDlg, MySqlConnection conn)
        {
            if (this.ColumnExists(tableName, columnName, conn))
            {
                infoDlg.AppendLine(string.Format("Column \"{0}.{1}\" already exists.", tableName, columnName));
                return;
            }

            infoDlg.AppendLine(string.Format("Adding column \"{0}.{1}\"", tableName, columnName));
            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
            {
                cmd.ExecuteNonQuery();
            }
        }

        private void AddIndexIfMissing(string tableName, string indexName, string sql, TextBoxInfoDialog infoDlg, MySqlConnection conn)
        {
            if (this.IndexExists(tableName, indexName, conn))
            {
                infoDlg.AppendLine(string.Format("Index \"{0}\" already exists on \"{1}\".", indexName, tableName));
                return;
            }

            infoDlg.AppendLine(string.Format("Adding index \"{0}\" on \"{1}\"", indexName, tableName));
            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
            {
                cmd.ExecuteNonQuery();
            }
        }

        private void AddForeignKeyIfMissing(string tableName, string constraintName, string sql, TextBoxInfoDialog infoDlg, MySqlConnection conn)
        {
            if (this.ForeignKeyExists(tableName, constraintName, conn))
            {
                infoDlg.AppendLine(string.Format("Foreign key \"{0}\" already exists on \"{1}\".", constraintName, tableName));
                return;
            }

            infoDlg.AppendLine(string.Format("Adding foreign key \"{0}\" on \"{1}\"", constraintName, tableName));
            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
            {
                cmd.ExecuteNonQuery();
            }
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

            Settings.DatabaseProvider provider = Settings.GetDatabaseProvider();
            if (m_providerCB != null && m_providerCB.SelectedItem != null)
                Enum.TryParse(Convert.ToString(m_providerCB.SelectedItem), out provider);

            MySqlSslMode sslMode;
            if (!Enum.TryParse(this.sslModeCB.Text, true, out sslMode))
                sslMode = MySqlSslMode.None;

            if (provider == Settings.DatabaseProvider.PostgreSql)
            {
                string password = GetConfiguredPassword();
                var builder = new NpgsqlConnectionStringBuilder
                {
                    Host = this.hostTB.Text.Trim(),
                    Port = Convert.ToInt32(port),
                    Username = this.userTB.Text.Trim(),
                    Database = this.dbTB.Text.Trim(),
                    Password = password,
                    SslMode = MapPostgreSqlSslMode(sslMode),
                    Pooling = true
                };

                if (builder.SslMode == SslMode.Require)
                    builder.TrustServerCertificate = true;

                return builder.ConnectionString;
            }

            string mySqlPassword = GetConfiguredPassword();
            MySqlConnectionStringBuilder bldr = new MySqlConnectionStringBuilder
            {
                Server = this.hostTB.Text.Trim(),
                Port = port,
                UserID = this.userTB.Text.Trim(),
                Database = this.dbTB.Text.Trim(),
                Password = mySqlPassword,
                SslMode = sslMode
            };

            return bldr.ConnectionString;
        }

        private string GetConfiguredPassword()
        {
            string password = this.passwordTB.Text;
            if (!string.IsNullOrWhiteSpace(password))
                return password;

            return Settings.Store.GetEncryptedSetting("Password");
        }

        private Settings.DatabaseProvider GetSelectedDatabaseProvider()
        {
            Settings.DatabaseProvider provider = Settings.GetDatabaseProvider();
            if (m_providerCB != null && m_providerCB.SelectedItem != null)
                Enum.TryParse(Convert.ToString(m_providerCB.SelectedItem), out provider);
            return provider;
        }

        private void TestPostgreSql(TextBoxInfoDialog infoDlg)
        {
            infoDlg.AppendLine(string.Format("Beginning test of {0} database...", GetSelectedProviderDisplayName()) + Environment.NewLine);
            NpgsqlConnection conn = null;
            try
            {
                string connStr = this.BuildConnectionString();
                if (connStr == null)
                    return;

                infoDlg.AppendLine("Connection Status");
                infoDlg.AppendLine("-------------------------------------");

                conn = new NpgsqlConnection(connStr);
                conn.Open();

                infoDlg.AppendLine(string.Format("Connection to {0} successful.", this.hostTB.Text.Trim()));
                infoDlg.AppendLine(string.Format("SSL mode: {0}", MapPostgreSqlSslMode(Settings.GetSslMode())));

                infoDlg.AppendLine(Environment.NewLine + "User Table");
                infoDlg.AppendLine("-------------------------------");
                CheckTable(this.userTableTB.Text.Trim(), BuildUserTableColumns(), infoDlg, conn);

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

                infoDlg.AppendLine(Environment.NewLine + LocalUserCache.TestConfiguration());
            }
            catch (Exception ex)
            {
                infoDlg.AppendLine(string.Format("ERROR: A fatal error occurred: {0}", ex));
            }
            finally
            {
                infoDlg.AppendLine(Environment.NewLine + "Closing connection.");
                if (conn != null)
                    conn.Close();
                infoDlg.AppendLine("Test complete.");
            }
        }

        private void CheckTable(string tableName, string[] columnNames, TextBoxInfoDialog infoDlg, NpgsqlConnection conn)
        {
            bool tableExists = this.TableExists(tableName, conn);
            if (!tableExists)
            {
                infoDlg.AppendLine(string.Format("ERROR: Table \"{0}\" not found.", tableName));
                return;
            }

            infoDlg.AppendLine(string.Format("Table \"{0}\" found.", tableName));
            List<string> columnNamesFromDb = new List<string>();

            using (NpgsqlCommand cmd = new NpgsqlCommand(
                "SELECT column_name FROM information_schema.columns WHERE table_schema = current_schema() AND table_name = @table ORDER BY ordinal_position",
                conn))
            {
                cmd.Parameters.AddWithValue("@table", tableName);
                using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                        columnNamesFromDb.Add(Convert.ToString(rdr[0]));
                }
            }

            bool ok = true;
            foreach (string columnName in columnNames)
            {
                if (columnNamesFromDb.Contains(columnName, StringComparer.CurrentCultureIgnoreCase))
                    infoDlg.AppendLine(string.Format("Found column \"{0}\"", columnName));
                else
                {
                    ok = false;
                    infoDlg.AppendLine(string.Format("ERROR: Column \"{0}\" not found!", columnName));
                }
            }

            if (!ok)
                infoDlg.AppendLine(string.Format("ERROR: Table \"{0}\" schema looks incorrect.", tableName));
        }

        private void CreatePostgreSqlTables()
        {
            string connStr = this.BuildConnectionString();
            if (connStr == null)
                return;

            TextBoxInfoDialog infoDlg = new TextBoxInfoDialog();
            infoDlg.ClearText();
            infoDlg.Show();

            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(connStr))
                {
                    infoDlg.AppendLine("Connecting...");
                    conn.Open();

                    CreatePostgreSqlUserTable(infoDlg, conn);
                    CreatePostgreSqlGroupTable(infoDlg, conn);
                    CreatePostgreSqlUserGroupTable(infoDlg, conn);

                    if (this.IsStandardEnglishFullSchemaRequested())
                    {
                        infoDlg.AppendLine(Environment.NewLine + "Extending standard English schema...");
                        this.EnsureEnglishReferenceTables(infoDlg, conn);
                        this.EnsureEnglishUserColumns(infoDlg, conn);
                    }
                }
            }
            catch (Exception ex)
            {
                infoDlg.AppendLine(string.Format("ERROR: {0}", ex.Message));
            }
            finally
            {
                infoDlg.AppendLine(Environment.NewLine + "Finished.");
            }
        }

        private void CreatePostgreSqlUserTable(TextBoxInfoDialog infoDlg, NpgsqlConnection conn)
        {
            string tableName = this.userTableTB.Text.Trim();
            infoDlg.AppendLine(Environment.NewLine + string.Format("Creating table \"{0}\"", tableName));

            if (this.TableExists(tableName, conn))
            {
                infoDlg.AppendLine(string.Format("WARNING: Table \"{0}\" already exists, skipping.", tableName));
                return;
            }

            string pk = this.userPrimaryKeyColTB.Text.Trim();
            string unameCol = this.unameColTB.Text.Trim();
            string hashMethodCol = this.hashMethodColTB.Text.Trim();
            string passwdCol = this.passwdColTB.Text.Trim();
            string statusCol = this.statusColTB.Text.Trim();
            string failedAttemptsCol = this.failedAttemptsColTB.Text.Trim();
            string blockedUntilCol = this.blockedUntilColTB.Text.Trim();
            string lastAttemptCol = this.lastAttemptColTB.Text.Trim();
            bool pkIsUserName = unameCol.Equals(pk, StringComparison.CurrentCultureIgnoreCase);

            StringBuilder sql = new StringBuilder();
            sql.AppendFormat("CREATE TABLE {0} ( \r\n", QuotePg(tableName));
            if (!pkIsUserName)
                sql.AppendFormat(" {0} BIGSERIAL PRIMARY KEY, \r\n", QuotePg(pk));
            sql.AppendFormat(" {0} VARCHAR(128) {1}, \r\n", QuotePg(unameCol), pkIsUserName ? "PRIMARY KEY" : "NOT NULL UNIQUE");
            sql.AppendFormat(" {0} TEXT NOT NULL, \r\n", QuotePg(hashMethodCol));
            if (this.enforceStatusCB.Checked)
                sql.AppendFormat(" {0} VARCHAR(32) NOT NULL DEFAULT '{1}', \r\n", QuotePg(statusCol), this.activeValueTB.Text.Trim().Replace("'", "''"));
            bool includeLockoutColumns = this.lockoutEnabledCB.Checked || this.IsStandardEnglishFullSchemaRequested();
            if (includeLockoutColumns)
            {
                sql.AppendFormat(" {0} INT NOT NULL DEFAULT 0, \r\n", QuotePg(failedAttemptsCol));
                sql.AppendFormat(" {0} TIMESTAMP NULL, \r\n", QuotePg(blockedUntilCol));
                sql.AppendFormat(" {0} TIMESTAMP NULL, \r\n", QuotePg(lastAttemptCol));
            }
            sql.AppendFormat(" {0} TEXT \r\n", QuotePg(passwdCol));
            sql.Append(")");

            infoDlg.AppendLine("Executing SQL:");
            infoDlg.AppendLine(sql.ToString());

            using (NpgsqlCommand cmd = new NpgsqlCommand(sql.ToString(), conn))
            {
                cmd.ExecuteNonQuery();
                infoDlg.AppendLine(string.Format("Table \"{0}\" created.", tableName));
            }
        }

        private void CreatePostgreSqlGroupTable(TextBoxInfoDialog infoDlg, NpgsqlConnection conn)
        {
            string tableName = this.groupTableNameTB.Text.Trim();
            infoDlg.AppendLine(Environment.NewLine + string.Format("Creating table \"{0}\"", tableName));

            if (this.TableExists(tableName, conn))
            {
                infoDlg.AppendLine(string.Format("WARNING: Table \"{0}\" already exists, skipping.", tableName));
                return;
            }

            string pk = this.groupTablePrimaryKeyColTB.Text.Trim();
            string groupNameCol = this.groupNameColTB.Text.Trim();
            bool pkIsGroupName = groupNameCol.Equals(pk, StringComparison.CurrentCultureIgnoreCase);

            StringBuilder sql = new StringBuilder();
            sql.AppendFormat("CREATE TABLE {0} ( \r\n", QuotePg(tableName));
            if (!pkIsGroupName)
                sql.AppendFormat(" {0} BIGSERIAL PRIMARY KEY, \r\n", QuotePg(pk));
            sql.AppendFormat(" {0} VARCHAR(128) {1} \r\n", QuotePg(groupNameCol), pkIsGroupName ? "PRIMARY KEY" : "NOT NULL UNIQUE");
            sql.Append(")");

            infoDlg.AppendLine("Executing SQL:");
            infoDlg.AppendLine(sql.ToString());

            using (NpgsqlCommand cmd = new NpgsqlCommand(sql.ToString(), conn))
            {
                cmd.ExecuteNonQuery();
                infoDlg.AppendLine(string.Format("Table \"{0}\" created.", tableName));
            }
        }

        private void CreatePostgreSqlUserGroupTable(TextBoxInfoDialog infoDlg, NpgsqlConnection conn)
        {
            string tableName = this.userGroupTableNameTB.Text.Trim();
            infoDlg.AppendLine(Environment.NewLine + string.Format("Creating table \"{0}\"", tableName));

            if (this.TableExists(tableName, conn))
            {
                infoDlg.AppendLine(string.Format("WARNING: Table \"{0}\" already exists, skipping.", tableName));
                return;
            }

            string userFK = this.userGroupUserFKColTB.Text.Trim();
            string userPK = this.userPrimaryKeyColTB.Text.Trim();
            string groupFK = this.userGroupGroupFKColTB.Text.Trim();
            string groupPK = this.groupTablePrimaryKeyColTB.Text.Trim();
            string groupNameCol = this.groupNameColTB.Text.Trim();
            string unameCol = this.unameColTB.Text.Trim();

            bool pkIsGroupName = groupNameCol.Equals(groupPK, StringComparison.CurrentCultureIgnoreCase);
            bool pkIsUserName = unameCol.Equals(userPK, StringComparison.CurrentCultureIgnoreCase);

            StringBuilder sql = new StringBuilder();
            sql.AppendFormat("CREATE TABLE {0} ( \r\n", QuotePg(tableName));
            sql.AppendFormat(" {0} {1}, \r\n", QuotePg(groupFK), pkIsGroupName ? "VARCHAR(128)" : "BIGINT");
            sql.AppendFormat(" {0} {1}, \r\n", QuotePg(userFK), pkIsUserName ? "VARCHAR(128)" : "BIGINT");
            sql.AppendFormat(" PRIMARY KEY ({0}, {1}) \r\n", QuotePg(userFK), QuotePg(groupFK));
            sql.Append(")");

            infoDlg.AppendLine("Executing SQL:");
            infoDlg.AppendLine(sql.ToString());

            using (NpgsqlCommand cmd = new NpgsqlCommand(sql.ToString(), conn))
            {
                cmd.ExecuteNonQuery();
                infoDlg.AppendLine(string.Format("Table \"{0}\" created.", tableName));
            }
        }

        private bool TableExists(string tableName, NpgsqlConnection conn)
        {
            string query = "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = current_schema() AND table_name = @table";
            using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@table", tableName);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        private bool ColumnExists(string tableName, string columnName, NpgsqlConnection conn)
        {
            string query =
                "SELECT COUNT(*) FROM information_schema.columns " +
                "WHERE table_schema = current_schema() AND table_name = @table AND column_name = @column";
            using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@table", tableName);
                cmd.Parameters.AddWithValue("@column", columnName);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        private bool IndexExists(string tableName, string indexName, NpgsqlConnection conn)
        {
            string query =
                "SELECT COUNT(*) FROM pg_indexes " +
                "WHERE schemaname = current_schema() AND tablename = @table AND indexname = @index";
            using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@table", tableName);
                cmd.Parameters.AddWithValue("@index", indexName);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        private bool ForeignKeyExists(string tableName, string constraintName, NpgsqlConnection conn)
        {
            string query =
                "SELECT COUNT(*) FROM information_schema.table_constraints " +
                "WHERE table_schema = current_schema() AND table_name = @table AND constraint_name = @constraint";
            using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@table", tableName);
                cmd.Parameters.AddWithValue("@constraint", constraintName);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        private void EnsureEnglishReferenceTables(TextBoxInfoDialog infoDlg, NpgsqlConnection conn)
        {
            if (!this.TableExists("careers", conn))
            {
                string sql = "CREATE TABLE \"careers\" (\"id\" SERIAL PRIMARY KEY, \"name\" VARCHAR(255) NOT NULL, \"status\" INT NOT NULL DEFAULT 1)";
                infoDlg.AppendLine("Creating table \"careers\"");
                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                    cmd.ExecuteNonQuery();
            }
            else
            {
                infoDlg.AppendLine("Table \"careers\" already exists.");
            }

            if (!this.TableExists("levels", conn))
            {
                string sql = "CREATE TABLE \"levels\" (\"id\" SERIAL PRIMARY KEY, \"name\" VARCHAR(100) NOT NULL, \"status\" INT NOT NULL DEFAULT 1)";
                infoDlg.AppendLine("Creating table \"levels\"");
                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                    cmd.ExecuteNonQuery();
            }
            else
            {
                infoDlg.AppendLine("Table \"levels\" already exists.");
            }
        }

        private void EnsureEnglishUserColumns(TextBoxInfoDialog infoDlg, NpgsqlConnection conn)
        {
            this.AddColumnIfMissing("users", "failed_attempts", "ALTER TABLE \"users\" ADD COLUMN \"failed_attempts\" INT NOT NULL DEFAULT 0", infoDlg, conn);
            this.AddColumnIfMissing("users", "locked_until", "ALTER TABLE \"users\" ADD COLUMN \"locked_until\" TIMESTAMP NULL", infoDlg, conn);
            this.AddColumnIfMissing("users", "last_attempt_at", "ALTER TABLE \"users\" ADD COLUMN \"last_attempt_at\" TIMESTAMP NULL", infoDlg, conn);
            this.AddColumnIfMissing("users", "first_name", "ALTER TABLE \"users\" ADD COLUMN \"first_name\" VARCHAR(100) NULL", infoDlg, conn);
            this.AddColumnIfMissing("users", "last_name", "ALTER TABLE \"users\" ADD COLUMN \"last_name\" VARCHAR(100) NULL", infoDlg, conn);
            this.AddColumnIfMissing("users", "document_id", "ALTER TABLE \"users\" ADD COLUMN \"document_id\" VARCHAR(15) NULL", infoDlg, conn);
            this.AddColumnIfMissing("users", "email", "ALTER TABLE \"users\" ADD COLUMN \"email\" VARCHAR(200) NULL", infoDlg, conn);
            this.AddColumnIfMissing("users", "career_id", "ALTER TABLE \"users\" ADD COLUMN \"career_id\" INT NULL", infoDlg, conn);
            this.AddColumnIfMissing("users", "level_id", "ALTER TABLE \"users\" ADD COLUMN \"level_id\" INT NULL", infoDlg, conn);

            this.AddIndexIfMissing("users", "idx_users_career_id", "CREATE INDEX \"idx_users_career_id\" ON \"users\" (\"career_id\")", infoDlg, conn);
            this.AddIndexIfMissing("users", "idx_users_level_id", "CREATE INDEX \"idx_users_level_id\" ON \"users\" (\"level_id\")", infoDlg, conn);

            this.AddForeignKeyIfMissing("users", "fk_users_career", "ALTER TABLE \"users\" ADD CONSTRAINT \"fk_users_career\" FOREIGN KEY (\"career_id\") REFERENCES \"careers\" (\"id\") ON DELETE CASCADE ON UPDATE CASCADE", infoDlg, conn);
            this.AddForeignKeyIfMissing("users", "fk_users_level", "ALTER TABLE \"users\" ADD CONSTRAINT \"fk_users_level\" FOREIGN KEY (\"level_id\") REFERENCES \"levels\" (\"id\") ON DELETE CASCADE ON UPDATE CASCADE", infoDlg, conn);
        }

        private void AddColumnIfMissing(string tableName, string columnName, string sql, TextBoxInfoDialog infoDlg, NpgsqlConnection conn)
        {
            if (this.ColumnExists(tableName, columnName, conn))
            {
                infoDlg.AppendLine(string.Format("Column \"{0}.{1}\" already exists.", tableName, columnName));
                return;
            }

            infoDlg.AppendLine(string.Format("Adding column \"{0}.{1}\"", tableName, columnName));
            using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                cmd.ExecuteNonQuery();
        }

        private void AddIndexIfMissing(string tableName, string indexName, string sql, TextBoxInfoDialog infoDlg, NpgsqlConnection conn)
        {
            if (this.IndexExists(tableName, indexName, conn))
            {
                infoDlg.AppendLine(string.Format("Index \"{0}\" already exists on \"{1}\".", indexName, tableName));
                return;
            }

            infoDlg.AppendLine(string.Format("Adding index \"{0}\" on \"{1}\"", indexName, tableName));
            using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                cmd.ExecuteNonQuery();
        }

        private void AddForeignKeyIfMissing(string tableName, string constraintName, string sql, TextBoxInfoDialog infoDlg, NpgsqlConnection conn)
        {
            if (this.ForeignKeyExists(tableName, constraintName, conn))
            {
                infoDlg.AppendLine(string.Format("Foreign key \"{0}\" already exists on \"{1}\".", constraintName, tableName));
                return;
            }

            infoDlg.AppendLine(string.Format("Adding foreign key \"{0}\" on \"{1}\"", constraintName, tableName));
            using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                cmd.ExecuteNonQuery();
        }

        private static string QuotePg(string identifier)
        {
            return "\"" + identifier.Replace("\"", "\"\"") + "\"";
        }

        private void providerCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateProviderUi();
        }

        private void UpdateProviderUi()
        {
            Settings.DatabaseProvider provider = Settings.DatabaseProvider.MySql;
            if (m_providerCB != null && m_providerCB.SelectedItem != null)
                Enum.TryParse(Convert.ToString(m_providerCB.SelectedItem), out provider);

            bool isMySql = provider == Settings.DatabaseProvider.MySql;
            this.label21.Enabled = isMySql;
            this.sslModeCB.Enabled = true;
            this.groupBox1.Text = "Database Server";
            this.label4.Text = "Database:";
        }

        private string GetSelectedProviderDisplayName()
        {
            return GetSelectedDatabaseProvider() == Settings.DatabaseProvider.PostgreSql
                ? "PostgreSQL"
                : "MySQL/MariaDB";
        }

        private static SslMode MapPostgreSqlSslMode(MySqlSslMode sslMode)
        {
            switch (sslMode)
            {
                case MySqlSslMode.Required:
                case MySqlSslMode.VerifyCA:
                case MySqlSslMode.VerifyFull:
                    return SslMode.Require;
                default:
                    return SslMode.Disable;
            }
        }

        private string[] BuildUserTableColumns()
        {
            List<string> columns = new List<string>
            {
                this.unameColTB.Text.Trim(),
                this.passwdColTB.Text.Trim(),
                this.hashMethodColTB.Text.Trim(),
                this.userPrimaryKeyColTB.Text.Trim()
            };

            if (this.enforceStatusCB.Checked)
                columns.Add(this.statusColTB.Text.Trim());

            if (this.lockoutEnabledCB.Checked)
            {
                columns.Add(this.failedAttemptsColTB.Text.Trim());
                columns.Add(this.blockedUntilColTB.Text.Trim());
                columns.Add(this.lastAttemptColTB.Text.Trim());
            }

            return columns.ToArray();
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


