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
using System.Data;
using System.Linq;
using System.Text;
using System.Globalization;

using log4net;

// Updated to use MySqlConnector for MySQL 8.x and MariaDB compatibility
using MySqlConnector;

namespace OpenCredential.Plugin.DatabaseAuth
{
    /// <summary>
    /// Provides access to user data stored in MySQL/MariaDB database.
    /// Supports MySQL 8.x and MariaDB 10.x/11.x.
    /// </summary>
    class MySqlUserDataSource : IUserDataSource
    {
        private MySqlConnection m_conn = null;
        private ILog m_logger;
        private bool m_disposed = false;

        public MySqlUserDataSource()
        {
            m_logger = LogManager.GetLogger("DatabaseAuth.MySqlUserDataSource");

            try
            {
                var builder = new MySqlConnectionStringBuilder();
                builder.Server = Settings.Store.Host;
                
                // FIX: Use helper method with proper casting
                int port = Settings.GetPort();
                builder.Port = Convert.ToUInt32(port > 0 ? port : 3306);
                
                builder.UserID = Settings.Store.User;
                builder.Database = Settings.Store.Database;
                builder.Password = Settings.Store.GetEncryptedSetting("Password");

                // SSL/TLS Configuration - FIX: Use helper method
                builder.SslMode = Settings.GetSslMode();

                // MySQL 8.x and MariaDB compatibility settings
                builder.AllowUserVariables = true;
                builder.AllowZeroDateTime = true;
                builder.ConvertZeroDateTime = true;
                
                // Timeout settings - FIX: Use helper methods
                builder.ConnectionTimeout = (uint)Settings.GetConnectionTimeout();
                builder.DefaultCommandTimeout = (uint)Settings.GetCommandTimeout();
                
                // Character set and pooling
                builder.CharacterSet = "utf8mb4";
                builder.Pooling = true;

                m_conn = new MySqlConnection(builder.ConnectionString);
                m_conn.Open();
                
                m_logger.DebugFormat("Connected to MySQL. Server version: {0}", m_conn.ServerVersion);
            }
            catch (MySqlException ex)
            {
                m_logger.ErrorFormat("MySQL connection error: {0}", ex.Message);
                throw;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                if (disposing && m_conn != null)
                {
                    try
                    {
                        m_conn.Close();
                        m_conn.Dispose();
                    }
                    catch (Exception ex)
                    {
                        m_logger.WarnFormat("Error disposing connection: {0}", ex.Message);
                    }
                    m_conn = null;
                }
                m_disposed = true;
            }
        }

        /// <summary>
        /// Gets user entry from the database including password hash information.
        /// Supports BCrypt and legacy hash algorithms.
        /// </summary>
        public UserEntry GetUserEntry(string userName)
        {
            if (m_conn == null || m_conn.State != ConnectionState.Open)
            {
                m_logger.Error("Database connection is not open");
                return null;
            }

            bool enforceUserStatus = Settings.IsUserStatusValidationEnabled();
            bool includeLockout = Settings.IsLoginLockoutEnabled();
            string query = enforceUserStatus && includeLockout
                ? string.Format(
                    "SELECT `{1}`, `{2}`, `{3}`, `{4}`, `{5}`, `{6}`, " +
                    "CASE WHEN `{6}` IS NOT NULL AND `{6}` > NOW() THEN 1 ELSE 0 END " +
                    "FROM `{0}` WHERE `{1}` = @user",
                    Settings.Store.Table,
                    Settings.Store.UsernameColumn,
                    Settings.Store.HashMethodColumn,
                    Settings.Store.PasswordColumn,
                    Settings.GetUserStatusColumn(),
                    Settings.GetFailedAttemptsColumn(),
                    Settings.GetBlockedUntilColumn())
                : enforceUserStatus
                ? string.Format("SELECT `{1}`, `{2}`, `{3}`, `{4}` FROM `{0}` WHERE `{1}` = @user",
                    Settings.Store.Table,
                    Settings.Store.UsernameColumn,
                    Settings.Store.HashMethodColumn,
                    Settings.Store.PasswordColumn,
                    Settings.GetUserStatusColumn())
                : includeLockout
                ? string.Format(
                    "SELECT `{1}`, `{2}`, `{3}`, `{4}`, `{5}`, " +
                    "CASE WHEN `{5}` IS NOT NULL AND `{5}` > NOW() THEN 1 ELSE 0 END " +
                    "FROM `{0}` WHERE `{1}` = @user",
                    Settings.Store.Table,
                    Settings.Store.UsernameColumn,
                    Settings.Store.HashMethodColumn,
                    Settings.Store.PasswordColumn,
                    Settings.GetFailedAttemptsColumn(),
                    Settings.GetBlockedUntilColumn())
                : string.Format("SELECT `{1}`, `{2}`, `{3}` FROM `{0}` WHERE `{1}` = @user",
                    Settings.Store.Table,
                    Settings.Store.UsernameColumn,
                    Settings.Store.HashMethodColumn,
                    Settings.Store.PasswordColumn);

            m_logger.DebugFormat("Executing query to find user: {0}", userName);

            using (var cmd = new MySqlCommand(query, m_conn))
            {
                cmd.Parameters.AddWithValue("@user", userName);

                using (var rdr = cmd.ExecuteReader())
                {
                    if (rdr.HasRows)
                    {
                        rdr.Read();

                        string uname = rdr[0].ToString();
                        string hashMethodStr = rdr[1] != null && rdr[1] != DBNull.Value
                            ? rdr[1].ToString().ToUpper().Trim()
                            : "NONE";
                        string hash = rdr[2] != null && rdr[2] != DBNull.Value
                            ? rdr[2].ToString()
                            : "";
                        int nextFieldIndex = 3;
                        string statusValue = enforceUserStatus && rdr.FieldCount > nextFieldIndex && rdr[nextFieldIndex] != DBNull.Value
                            ? rdr[nextFieldIndex++].ToString().Trim()
                            : string.Empty;
                        int failedAttempts = includeLockout && rdr.FieldCount > nextFieldIndex && rdr[nextFieldIndex] != DBNull.Value
                            ? Convert.ToInt32(rdr[nextFieldIndex++])
                            : 0;
                        DateTime? blockedUntilUtc = includeLockout && rdr.FieldCount > nextFieldIndex
                            ? ReadUtcDateTime(rdr[nextFieldIndex++])
                            : null;
                        bool isLockedByDatabase = includeLockout && rdr.FieldCount > nextFieldIndex && rdr[nextFieldIndex] != DBNull.Value
                            ? Convert.ToInt32(rdr[nextFieldIndex]) == 1
                            : false;

                        m_logger.DebugFormat("User {0} found, hash method from DB: {1}", uname, hashMethodStr);

                        if (enforceUserStatus &&
                            !string.Equals(statusValue, Settings.GetUserActiveValue(), StringComparison.OrdinalIgnoreCase))
                        {
                            m_logger.WarnFormat(
                                "User {0} rejected because status '{1}' does not match active value '{2}'",
                                uname,
                                statusValue,
                                Settings.GetUserActiveValue());
                            return null;
                        }

                        PasswordHashAlgorithm hashAlg;
                        if (!TryParseHashAlgorithm(hashMethodStr, hash, uname, out hashAlg))
                            return null;

                        m_logger.DebugFormat("Retrieved user entry for {0}, hash algorithm: {1}", uname, hashAlg);
                        return new UserEntry(uname, hashAlg, hash, statusValue, failedAttempts, blockedUntilUtc, isLockedByDatabase);
                    }

                    m_logger.DebugFormat("User {0} not found in database", userName);
                    return null;
                }
            }
        }

        public List<UserEntry> GetAllUserEntriesForCache()
        {
            if (m_conn == null || m_conn.State != ConnectionState.Open)
            {
                m_logger.Error("Database connection is not open");
                return new List<UserEntry>();
            }

            bool includeStatus = Settings.IsUserStatusValidationEnabled();
            bool includeLockout = Settings.IsLoginLockoutEnabled();
            string query = includeStatus && includeLockout
                ? string.Format("SELECT `{1}`, `{2}`, `{3}`, `{4}`, `{5}`, `{6}` FROM `{0}`",
                    Settings.Store.Table,
                    Settings.Store.UsernameColumn,
                    Settings.Store.HashMethodColumn,
                    Settings.Store.PasswordColumn,
                    Settings.GetUserStatusColumn(),
                    Settings.GetFailedAttemptsColumn(),
                    Settings.GetBlockedUntilColumn())
                : includeStatus
                ? string.Format("SELECT `{1}`, `{2}`, `{3}`, `{4}` FROM `{0}`",
                    Settings.Store.Table,
                    Settings.Store.UsernameColumn,
                    Settings.Store.HashMethodColumn,
                    Settings.Store.PasswordColumn,
                    Settings.GetUserStatusColumn())
                : includeLockout
                ? string.Format("SELECT `{1}`, `{2}`, `{3}`, `{4}`, `{5}` FROM `{0}`",
                    Settings.Store.Table,
                    Settings.Store.UsernameColumn,
                    Settings.Store.HashMethodColumn,
                    Settings.Store.PasswordColumn,
                    Settings.GetFailedAttemptsColumn(),
                    Settings.GetBlockedUntilColumn())
                : string.Format("SELECT `{1}`, `{2}`, `{3}` FROM `{0}`",
                    Settings.Store.Table,
                    Settings.Store.UsernameColumn,
                    Settings.Store.HashMethodColumn,
                    Settings.Store.PasswordColumn);

            var users = new List<UserEntry>();

            using (var cmd = new MySqlCommand(query, m_conn))
            using (var rdr = cmd.ExecuteReader())
            {
                while (rdr.Read())
                {
                    string uname = Convert.ToString(rdr[0]);
                    string hashMethodStr = rdr[1] != DBNull.Value ? Convert.ToString(rdr[1]).ToUpper().Trim() : "NONE";
                    string hash = rdr[2] != DBNull.Value ? Convert.ToString(rdr[2]) : string.Empty;
                    int nextFieldIndex = 3;
                    string statusValue = includeStatus && rdr.FieldCount > nextFieldIndex && rdr[nextFieldIndex] != DBNull.Value
                        ? Convert.ToString(rdr[nextFieldIndex++]).Trim()
                        : null;
                    int failedAttempts = includeLockout && rdr.FieldCount > nextFieldIndex && rdr[nextFieldIndex] != DBNull.Value
                        ? Convert.ToInt32(rdr[nextFieldIndex++])
                        : 0;
                    DateTime? blockedUntilUtc = includeLockout && rdr.FieldCount > nextFieldIndex
                        ? ReadUtcDateTime(rdr[nextFieldIndex++])
                        : null;
                    PasswordHashAlgorithm hashAlg;

                    if (!TryParseHashAlgorithm(hashMethodStr, hash, uname, out hashAlg))
                        continue;

                    users.Add(new UserEntry(uname, hashAlg, hash, statusValue, failedAttempts, blockedUntilUtc));
                }
            }

            return users;
        }

        /// <summary>
        /// Checks if a user is a member of a specific group.
        /// </summary>
        public bool IsMemberOfGroup(string userName, string groupName)
        {
            if (m_conn == null || m_conn.State != ConnectionState.Open)
            {
                m_logger.Error("Database connection is not open");
                return false;
            }

            // Get user ID first
            string query = string.Format("SELECT `{2}` FROM `{0}` WHERE `{1}` = @user",
                Settings.Store.Table,
                Settings.Store.UsernameColumn,
                Settings.Store.UserTablePrimaryKeyColumn);

            string userId = null;

            using (var cmd = new MySqlCommand(query, m_conn))
            {
                cmd.Parameters.AddWithValue("@user", userName);

                using (var rdr = cmd.ExecuteReader())
                {
                    if (rdr.HasRows)
                    {
                        rdr.Read();
                        userId = rdr[0].ToString();
                    }
                }
            }

            if (userId == null)
            {
                m_logger.DebugFormat("User {0} not found when checking group membership", userName);
                return false;
            }

            // Check group membership
            query = string.Format(
                "SELECT `{0}`.`{5}` FROM `{0}`, `{1}` " +
                "WHERE `{0}`.`{4}` = `{1}`.`{3}` AND `{1}`.`{2}` = @userId",
                Settings.Store.GroupTableName,
                Settings.Store.UserGroupTableName,
                Settings.Store.UserForeignKeyColumn,
                Settings.Store.GroupForeignKeyColumn,
                Settings.Store.GroupTablePrimaryKeyColumn,
                Settings.Store.GroupNameColumn);

            using (var cmd = new MySqlCommand(query, m_conn))
            {
                cmd.Parameters.AddWithValue("@userId", userId);

                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        if (rdr[0].ToString().Equals(groupName, StringComparison.OrdinalIgnoreCase))
                        {
                            m_logger.DebugFormat("User {0} IS member of group {1}", userName, groupName);
                            return true;
                        }
                    }
                }
            }

            m_logger.DebugFormat("User {0} is NOT member of group {1}", userName, groupName);
            return false;
        }

        private bool TryParseHashAlgorithm(string hashMethodStr, string hash, string uname, out PasswordHashAlgorithm hashAlg)
        {
            switch (hashMethodStr)
            {
                case "NONE":
                    hashAlg = PasswordHashAlgorithm.NONE;
                    return true;
                case "MD5":
                    hashAlg = PasswordHashAlgorithm.MD5;
                    return true;
                case "SMD5":
                    hashAlg = PasswordHashAlgorithm.SMD5;
                    return true;
                case "SHA1":
                    hashAlg = PasswordHashAlgorithm.SHA1;
                    return true;
                case "SSHA1":
                    hashAlg = PasswordHashAlgorithm.SSHA1;
                    return true;
                case "SHA256":
                    hashAlg = PasswordHashAlgorithm.SHA256;
                    return true;
                case "SSHA256":
                    hashAlg = PasswordHashAlgorithm.SSHA256;
                    return true;
                case "SHA384":
                    hashAlg = PasswordHashAlgorithm.SHA384;
                    return true;
                case "SSHA384":
                    hashAlg = PasswordHashAlgorithm.SSHA384;
                    return true;
                case "SHA512":
                    hashAlg = PasswordHashAlgorithm.SHA512;
                    return true;
                case "SSHA512":
                    hashAlg = PasswordHashAlgorithm.SSHA512;
                    return true;
                case "BCRYPT":
                case "BCRYPT_SHA256":
                    hashAlg = PasswordHashAlgorithm.BCRYPT;
                    m_logger.DebugFormat("User {0} using BCrypt hash", uname);
                    return true;
                default:
                    if (hash.Length == 60 &&
                        (hash.StartsWith("$2a$") || hash.StartsWith("$2b$") || hash.StartsWith("$2y$")))
                    {
                        m_logger.WarnFormat(
                            "Hash method column says '{0}' but hash appears to be BCrypt for user {1}",
                            hashMethodStr,
                            uname);
                        hashAlg = PasswordHashAlgorithm.BCRYPT;
                        return true;
                    }

                    m_logger.ErrorFormat("Unrecognized hash method: {0} for user {1}", hashMethodStr, uname);
                    hashAlg = PasswordHashAlgorithm.NONE;
                    return false;
            }
        }

        /// <summary>
        /// Updates a user's password hash in the database.
        /// Used for BCrypt migration feature.
        /// </summary>
        /// <param name="userName">Username to update</param>
        /// <param name="newHash">New password hash</param>
        /// <param name="hashMethod">Hash method name (e.g., "BCRYPT")</param>
        public void UpdateUserHash(string userName, string newHash, string hashMethod)
        {
            if (m_conn == null || m_conn.State != ConnectionState.Open)
            {
                m_logger.Error("Database connection is not open");
                throw new InvalidOperationException("Database connection is not open");
            }

            string query = string.Format("UPDATE `{0}` SET `{2}` = @hashMethod, `{3}` = @hash WHERE `{1}` = @user",
                Settings.Store.Table,
                Settings.Store.UsernameColumn,
                Settings.Store.HashMethodColumn,
                Settings.Store.PasswordColumn);

            m_logger.DebugFormat("Updating hash for user: {0} to {1}", userName, hashMethod);

            using (var cmd = new MySqlCommand(query, m_conn))
            {
                cmd.Parameters.AddWithValue("@user", userName);
                cmd.Parameters.AddWithValue("@hashMethod", hashMethod);
                cmd.Parameters.AddWithValue("@hash", newHash);

                int rowsAffected = cmd.ExecuteNonQuery();
                
                if (rowsAffected > 0)
                {
                    m_logger.InfoFormat("Successfully updated hash for user {0}", userName);
                }
                else
                {
                    m_logger.WarnFormat("No rows affected when updating hash for user {0}", userName);
                }
            }
        }

        public void ResetFailedLoginState(string userName)
        {
            if (!Settings.IsLoginLockoutEnabled())
                return;

            string query = string.Format(
                "UPDATE `{0}` SET `{2}` = 0, `{3}` = NULL, `{4}` = NOW() WHERE `{1}` = @user",
                Settings.Store.Table,
                Settings.Store.UsernameColumn,
                Settings.GetFailedAttemptsColumn(),
                Settings.GetBlockedUntilColumn(),
                Settings.GetLastAttemptColumn());

            using (var cmd = new MySqlCommand(query, m_conn))
            {
                cmd.Parameters.AddWithValue("@user", userName);
                cmd.ExecuteNonQuery();
            }
        }

        public FailedAttemptResult RegisterFailedLoginAttempt(string userName)
        {
            var result = new FailedAttemptResult();

            if (!Settings.IsLoginLockoutEnabled())
                return result;

            string updateQuery = string.Format(
                "UPDATE `{0}` " +
                "SET `{2}` = COALESCE(`{2}`, 0) + 1, " +
                "`{4}` = NOW(), " +
                "`{3}` = CASE " +
                "WHEN COALESCE(`{2}`, 0) + 1 >= @maxAttempts THEN DATE_ADD(NOW(), INTERVAL @lockoutMinutes MINUTE) " +
                "ELSE NULL END " +
                "WHERE `{1}` = @user",
                Settings.Store.Table,
                Settings.Store.UsernameColumn,
                Settings.GetFailedAttemptsColumn(),
                Settings.GetBlockedUntilColumn(),
                Settings.GetLastAttemptColumn());

            using (var cmd = new MySqlCommand(updateQuery, m_conn))
            {
                cmd.Parameters.AddWithValue("@user", userName);
                cmd.Parameters.AddWithValue("@maxAttempts", Settings.GetMaxFailedAttempts());
                cmd.Parameters.AddWithValue("@lockoutMinutes", Settings.GetLockoutMinutes());
                cmd.ExecuteNonQuery();
            }

            string selectQuery = string.Format(
                "SELECT `{1}`, `{2}`, CASE WHEN `{2}` IS NOT NULL AND `{2}` > NOW() THEN 1 ELSE 0 END " +
                "FROM `{0}` WHERE `{3}` = @user",
                Settings.Store.Table,
                Settings.GetFailedAttemptsColumn(),
                Settings.GetBlockedUntilColumn(),
                Settings.Store.UsernameColumn);

            using (var cmd = new MySqlCommand(selectQuery, m_conn))
            {
                cmd.Parameters.AddWithValue("@user", userName);
                using (var rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        result.FailedAttempts = rdr[0] == DBNull.Value ? 0 : Convert.ToInt32(rdr[0]);
                        result.BlockedUntilUtc = ReadUtcDateTime(rdr[1]);
                        result.IsLockedByDatabase = rdr[2] != DBNull.Value && Convert.ToInt32(rdr[2]) == 1;
                    }
                }
            }

            return result;
        }

        private DateTime? ReadUtcDateTime(object value)
        {
            if (value == null || value == DBNull.Value)
                return null;

            if (value is DateTime dateTime)
                return dateTime.Kind == DateTimeKind.Utc ? dateTime : dateTime.ToUniversalTime();

            if (value is MySqlDateTime mySqlDateTime)
            {
                if (!mySqlDateTime.IsValidDateTime)
                    return null;

                DateTime parsedDateTime = mySqlDateTime.GetDateTime();
                return parsedDateTime.Kind == DateTimeKind.Utc
                    ? parsedDateTime
                    : parsedDateTime.ToUniversalTime();
            }

            if (DateTime.TryParse(Convert.ToString(value), CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out DateTime parsed))
                return parsed.ToUniversalTime();

            return null;
        }
    }
}

