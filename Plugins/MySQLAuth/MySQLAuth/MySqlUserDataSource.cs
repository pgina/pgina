using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using log4net;

using MySql.Data;
using MySql.Data.MySqlClient;

namespace pGina.Plugin.MySQLAuth
{
    class MySqlUserDataSource : IDisposable
    {
        private MySqlConnection m_conn = null;
        private ILog m_logger;

        public MySqlUserDataSource()
        {
            m_logger = LogManager.GetLogger("MySqlUserDataSource");

            MySqlConnectionStringBuilder bldr = new MySqlConnectionStringBuilder();
            bldr.Server = Settings.Store.Host;
            int port = Settings.Store.Port;
            bldr.Port = Convert.ToUInt32(port);
            bldr.UserID = Settings.Store.User;
            bldr.Database = Settings.Store.Database;
            bldr.Password = Settings.Store.GetEncryptedSetting("Password");
            bool useSsl = Settings.Store.UseSsl;
            if (useSsl)
                bldr.SslMode = MySqlSslMode.Required;

            m_conn = new MySqlConnection(bldr.GetConnectionString(true));
            if( m_conn != null ) m_conn.Open();
            else throw new Exception("Unable to create connection to database.");
        }

        public void Dispose()
        {
            if (m_conn != null)
            {
                m_logger.DebugFormat("Closing connection.");
                m_conn.Close();
            }
        }

        public UserEntry GetUserEntry(string userName)
        {
            string tableName = Settings.Store.Table;
            string unameCol = Settings.Store.UsernameColumn;
            string hashMethodCol = Settings.Store.HashMethodColumn;
            string passwdCol = Settings.Store.PasswordColumn;

            string query = string.Format("SELECT {1}, {2}, {3} " +
                "FROM {0} WHERE {1}=@user", tableName, unameCol, hashMethodCol, passwdCol);
            MySqlCommand cmd = new MySqlCommand(query, m_conn);
            cmd.Parameters.AddWithValue("@user", userName);
            MySqlDataReader rdr = cmd.ExecuteReader();
            if (rdr.HasRows)
            {
                rdr.Read();
                PasswordHashAlgorithm hashAlg;
                string uname = rdr[0].ToString();
                string hash = rdr[2].ToString();
                switch (rdr[1].ToString())
                {
                    case "NONE":
                        hashAlg = PasswordHashAlgorithm.NONE;
                        break;
                    case "MD5":
                        hashAlg = PasswordHashAlgorithm.MD5;
                        break;
                    case "SMD5":
                        hashAlg = PasswordHashAlgorithm.SMD5;
                        break;
                    case "SHA1":
                        hashAlg = PasswordHashAlgorithm.SHA1;
                        break;
                    case "SSHA1":
                        hashAlg = PasswordHashAlgorithm.SSHA1;
                        break;
                    case "SHA256":
                        hashAlg = PasswordHashAlgorithm.SHA256;
                        break;
                    case "SSHA256":
                        hashAlg = PasswordHashAlgorithm.SSHA256;
                        break;
                    case "SHA512":
                        hashAlg = PasswordHashAlgorithm.SHA512;
                        break;
                    case "SSHA512":
                        hashAlg = PasswordHashAlgorithm.SSHA512;
                        break;
                    case "SHA384":
                        hashAlg = PasswordHashAlgorithm.SHA384;
                        break;
                    case "SSHA384":
                        hashAlg = PasswordHashAlgorithm.SSHA384;
                        break;
                    default:
                        m_logger.ErrorFormat("Unrecognized hash algorithm: {0}", rdr[1].ToString());
                        return null;
                }
                rdr.Close();

                return new UserEntry(uname, hashAlg, hash);
            }
            return null;
        }

        public bool IsMemberOfGroup(string userName, string groupName)
        {
            string userTableName = Settings.Store.Table;
            string unameCol = Settings.Store.UsernameColumn;
            string userPK = Settings.Store.UserTablePrimaryKeyColumn;
            string user_id = null;

            // Get the user ID (the primary key for the user)
            string query = string.Format("SELECT {1}, {2} " +
               "FROM {0} WHERE {1}=@user", userTableName, unameCol, userPK);
            MySqlCommand cmd = new MySqlCommand(query, m_conn);
            cmd.Parameters.AddWithValue("@user", userName);
            MySqlDataReader rdr = cmd.ExecuteReader();
            if (rdr.HasRows)
            {
                rdr.Read();  // Read first row
                user_id = rdr[1].ToString();
            }
            rdr.Close();

            if (user_id == null)
            {
                m_logger.ErrorFormat("Unable to find entry in DB for {0}", userName);
                return false;
            }

            // Get all groups for the user, and check membership
            string userGroupTableName = Settings.Store.UserGroupTableName;
            string groupTableName = Settings.Store.GroupTableName;
            string groupFKCol = Settings.Store.GroupForeignKeyColumn;
            string userFKCol = Settings.Store.UserForeignKeyColumn;
            string groupPKCol = Settings.Store.GroupTablePrimaryKeyColumn;
            string groupNameCol = Settings.Store.GroupNameColumn;

            query = string.Format("SELECT {0}.{5} FROM {0},{1} " +
                "WHERE {0}.{4} = {1}.{3} AND {1}.{2} = @user_id",
                groupTableName, userGroupTableName, userFKCol, groupFKCol, groupPKCol, groupNameCol);
            cmd = new MySqlCommand(query, m_conn);
            cmd.Parameters.AddWithValue("@user_id", user_id);
            try
            {
                rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    string group = rdr[0].ToString();
                    if (group.Equals(groupName, StringComparison.CurrentCultureIgnoreCase))
                        return true;
                }
            }
            catch (Exception e)
            {
                m_logger.ErrorFormat("Exception: {0}", e);
                throw;
            }
            finally
            {
                if (rdr != null) rdr.Close();
            }

            return false;
        }
    }
}
