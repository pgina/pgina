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
            this.tableTB.Text = Settings.Store.Table;
            this.dbTB.Text = Settings.Store.Database;
            bool useSsl = Settings.Store.UseSsl;
            this.useSslCB.Checked = useSsl;
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
            Settings.Store.Table = this.tableTB.Text.Trim();
            Settings.Store.Database = this.dbTB.Text.Trim();
            Settings.Store.UseSsl = this.useSslCB.Checked;

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
            string tableName = this.tableTB.Text.Trim();
            try
            {
                string message = "";
                string connStr = this.BuildConnectionString();
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
                rdr.Close();

                if (tableExists)
                {
                    // Check for appropriate columns.
                    bool userCol = false;
                    bool hashCol = false;
                    bool passCol = false;
                    query = string.Format("DESCRIBE {0}", tableName);
                    cmd = new MySqlCommand(query, conn);
                    rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        string colName = rdr[0].ToString();
                        if (colName.Equals("user", StringComparison.CurrentCultureIgnoreCase))
                            userCol = true;
                        else if (colName.Equals("hash_method", StringComparison.CurrentCultureIgnoreCase))
                            hashCol = true;
                        else if (colName.Equals("password", StringComparison.CurrentCultureIgnoreCase))
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
            string tableName = this.tableTB.Text.Trim();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();

                    if (!this.TableExists(tableName, conn))
                    {
                        string sql = string.Format(
                            "CREATE TABLE {0} (" +
                            "   user VARCHAR(128) PRIMARY KEY, " +
                            "   hash_method TEXT, " +
                            "   password TEXT )", tableName);
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
            string host = this.hostTB.Text.Trim();
            string port = this.portTB.Text.Trim();
            string user = this.userTB.Text.Trim();
            string passwd = this.passwordTB.Text;
            string db = this.dbTB.Text.Trim();

            string connStr = String.Format(
                "server={0}; port={1};user={2}; password={3}; database={4}",
                    host, port, user, passwd, db);
            if (this.useSslCB.Checked)
            {
                connStr += ";SSL Mode=Required";
            }

            return connStr;
        }
        
    }
}
