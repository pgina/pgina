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
            this.userTableTB.Text = Settings.Store.Table;
            this.dbTB.Text = Settings.Store.Database;
            bool useSsl = Settings.Store.UseSsl;
            this.useSslCB.Checked = useSsl;
            this.unameColTB.Text = Settings.Store.UsernameColumn;
            this.hashMethodColTB.Text = Settings.Store.HashMethodColumn;
            this.passwdColTB.Text = Settings.Store.PasswordColumn;

            int encodingInt = Settings.Store.HashEncoding;
            Settings.HashEncoding encoding = (Settings.HashEncoding)encodingInt;

            if (encoding == Settings.HashEncoding.HEX)
                this.encHexRB.Checked = true;
            else
                this.encBase64RB.Checked = true;
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
            Settings.Store.Table = this.userTableTB.Text.Trim();
            Settings.Store.Database = this.dbTB.Text.Trim();
            Settings.Store.UseSsl = this.useSslCB.Checked;
            Settings.Store.UsernameColumn = this.unameColTB.Text.Trim();
            Settings.Store.HashMethodColumn = this.hashMethodColTB.Text.Trim();
            Settings.Store.PasswordColumn = this.passwdColTB.Text.Trim();

            if (encHexRB.Checked)
                Settings.Store.HashEncoding = (int)Settings.HashEncoding.HEX;
            else
                Settings.Store.HashEncoding = (int)Settings.HashEncoding.BASE_64;

            return true;
        }

        private void passwdCB_CheckedChanged(object sender, EventArgs e)
        {
            this.passwordTB.UseSystemPasswordChar = !this.passwdCB.Checked;
        }

        private void testBtn_Click(object sender, EventArgs e)
        {
            m_logger.Debug("Testing connection to database...");
            MySqlConnection conn = null;
            string tableName = this.userTableTB.Text.Trim();
            try
            {
                string message = "";
                string connStr = this.BuildConnectionString();
                if (connStr == null) return;
                conn = new MySqlConnection(connStr);
                conn.Open();

                message += "Connection Status";
                message += "\n---------------------";
                message += string.Format("\n  Connection to {0} successful.", this.hostTB.Text.Trim());

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
                        message += "\n  Not using SSL.";
                    }
                    else
                    {
                        message += "\n  SSL enabled, using cipher: " + cipher;
                    }
                }
                else
                {
                    message += "\n  Not using SSL.";
                }

                message += "\n\nUser Information Table";
                message += "\n-------------------------------";

                // Check for existence of the table
                bool tableExists = this.TableExists(tableName, conn);
                if (tableExists)
                {
                    m_logger.DebugFormat("Table {0} found.", tableName);
                    message += string.Format("\n  Table {0} found.", tableName);
                }
                else
                {
                    m_logger.DebugFormat("Table {0} not found.", tableName);
                    message += string.Format("\n  WARNING: Table {0} not found.", tableName);
                }

                if (tableExists)
                {
                    // Check for appropriate columns.
                    bool userCol = false;
                    bool hashCol = false;
                    bool passCol = false;
                    string unameColName = this.unameColTB.Text.Trim();
                    string hashMethodColName = this.hashMethodColTB.Text.Trim();
                    string passwdColName = this.passwdColTB.Text.Trim();
                    query = string.Format("DESCRIBE {0}", tableName);
                    cmd = new MySqlCommand(query, conn);
                    rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        string colName = rdr[0].ToString();
                        if (colName.Equals(unameColName, StringComparison.CurrentCultureIgnoreCase))
                            userCol = true;
                        else if (colName.Equals(hashMethodColName, StringComparison.CurrentCultureIgnoreCase))
                            hashCol = true;
                        else if (colName.Equals(passwdColName, StringComparison.CurrentCultureIgnoreCase))
                            passCol = true;
                    }
                    rdr.Close();

                    if (userCol && hashCol && passCol)
                    {
                        message += "\n  Table schema looks correct.";
                    }
                    else
                    {
                        message += "\n  WARNING: Table schema looks incorrect, authentication operations may fail.";
                        if (!userCol)
                            message += string.Format("\n      Column '{0}' not found.", unameColName);
                        if (!hashCol)
                            message += string.Format("\n      Column '{0}' not found.", hashMethodColName);
                        if (!passCol)
                            message += string.Format("\n      Column '{0}' not found.", passwdColName);
                    }
                }

                MessageBox.Show(message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("A fatal error occured: \n" + ex.Message);
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
        }

        private void createTableBtn_Click(object sender, EventArgs e)
        {
            string connStr = this.BuildConnectionString();
            if (connStr == null) return;
            string tableName = this.userTableTB.Text.Trim();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();

                    if (!this.TableExists(tableName, conn))
                    {
                        string unameCol = this.unameColTB.Text.Trim();
                        string hashMethodCol = this.hashMethodColTB.Text.Trim();
                        string passwdCol = this.passwdColTB.Text.Trim();

                        string sql = string.Format(
                            "CREATE TABLE {0} (" +
                            "   {1} VARCHAR(128) PRIMARY KEY, " +
                            "   {2} TEXT, " +
                            "   {3} TEXT )", tableName, unameCol, hashMethodCol, passwdCol);
                        MySqlCommand cmd = new MySqlCommand(sql, conn);
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Table created.");
                    }
                    else
                    {
                        MessageBox.Show("Table already exists.");
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(String.Format("Error: {0}", ex.Message));
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
    }
}
