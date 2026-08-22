// OpenCredential - Source code
// https://github.com/pedropablobm/OpenCredential
//
// Copyright (c) 2024, Pedro Bermudez
// All rights reserved.
//
// This file is part of OpenCredential, an unofficial fork of pGina.
// It is distributed under the BSD-3-Clause license terms used by the
// original pGina project unless stated otherwise.
//
// For more details, see the LICENSE and NOTICE files included with
// this distribution.

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using log4net;
using Npgsql;

namespace OpenCredential.Plugin.DatabaseAuth
{
    class PostgreSqlUserDataSource : IUserDataSource
    {
        private readonly NpgsqlConnection m_conn;
        private readonly ILog m_logger;
        private bool m_disposed;

        public PostgreSqlUserDataSource()
        {
            m_logger = LogManager.GetLogger("PostgreSqlUserDataSource");

            try
            {
                var builder = new NpgsqlConnectionStringBuilder
                {
                    Host = Settings.Store.Host,
                    Port = Settings.GetPort() > 0 ? Settings.GetPort() : 5432,
                    Username = Settings.Store.User,
                    Database = Settings.Store.Database,
                    Password = Settings.Store.GetEncryptedSetting("Password"),
                    SslMode = MapSslMode(Settings.GetSslMode()),
                    Timeout = Settings.GetConnectionTimeout(),
                    CommandTimeout = Settings.GetCommandTimeout(),
                    Pooling = true
                };

                if (builder.SslMode == SslMode.Require)
                {
                    builder.TrustServerCertificate = true;
                }

                m_conn = new NpgsqlConnection(builder.ConnectionString);
                m_conn.Open();

                m_logger.DebugFormat("Connected to PostgreSQL. Server version: {0}", m_conn.PostgreSqlVersion);
            }
            catch (NpgsqlException ex)
            {
                m_logger.ErrorFormat("PostgreSQL connection error: {0}", ex.Message);
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
            if (m_disposed)
                return;

            if (disposing && m_conn != null)
            {
                try
                {
                    m_conn.Close();
                    m_conn.Dispose();
                }
                catch (Exception ex)
                {
                    m_logger.WarnFormat("Error disposing PostgreSQL connection: {0}", ex.Message);
                }
            }

            m_disposed = true;
        }

        public UserEntry GetUserEntry(string userName)
        {
            EnsureConnection();

            bool enforceUserStatus = Settings.IsUserStatusValidationEnabled();
            bool includeLockout = Settings.IsLoginLockoutEnabled();
            string table = Convert.ToString(Settings.Store.Table);
            string usernameColumn = Convert.ToString(Settings.Store.UsernameColumn);
            string hashMethodColumn = Convert.ToString(Settings.Store.HashMethodColumn);
            string passwordColumn = Convert.ToString(Settings.Store.PasswordColumn);
            string userStatusColumn = Settings.GetUserStatusColumn();
            string failedAttemptsColumn = Settings.GetFailedAttemptsColumn();
            string blockedUntilColumn = Settings.GetBlockedUntilColumn();
            string query = enforceUserStatus && includeLockout
                ? string.Format(
                    "SELECT {1}, {2}, {3}, {4}, {5}, {6}, " +
                    "CASE WHEN {6} IS NOT NULL AND {6} > CURRENT_TIMESTAMP THEN 1 ELSE 0 END " +
                    "FROM {0} WHERE {1} = @user",
                    Q(table),
                    Q(usernameColumn),
                    Q(hashMethodColumn),
                    Q(passwordColumn),
                    Q(userStatusColumn),
                    Q(failedAttemptsColumn),
                    Q(blockedUntilColumn))
                : enforceUserStatus
                ? string.Format("SELECT {1}, {2}, {3}, {4} FROM {0} WHERE {1} = @user",
                    Q(table),
                    Q(usernameColumn),
                    Q(hashMethodColumn),
                    Q(passwordColumn),
                    Q(userStatusColumn))
                : includeLockout
                ? string.Format(
                    "SELECT {1}, {2}, {3}, {4}, {5}, " +
                    "CASE WHEN {5} IS NOT NULL AND {5} > CURRENT_TIMESTAMP THEN 1 ELSE 0 END " +
                    "FROM {0} WHERE {1} = @user",
                    Q(table),
                    Q(usernameColumn),
                    Q(hashMethodColumn),
                    Q(passwordColumn),
                    Q(failedAttemptsColumn),
                    Q(blockedUntilColumn))
                : string.Format("SELECT {1}, {2}, {3} FROM {0} WHERE {1} = @user",
                    Q(table),
                    Q(usernameColumn),
                    Q(hashMethodColumn),
                    Q(passwordColumn));

            using (var cmd = new NpgsqlCommand(query, m_conn))
            {
                cmd.Parameters.AddWithValue("@user", userName);

                using (var rdr = cmd.ExecuteReader())
                {
                    if (!rdr.Read())
                    {
                        m_logger.DebugFormat("User {0} not found in PostgreSQL database", userName);
                        return null;
                    }

                    string uname = Convert.ToString(rdr[0]);
                    string hashMethodStr = rdr[1] != DBNull.Value
                        ? Convert.ToString(rdr[1]).ToUpperInvariant().Trim()
                        : "NONE";
                    string hash = rdr[2] != DBNull.Value ? Convert.ToString(rdr[2]) : string.Empty;
                    int nextFieldIndex = 3;
                    string statusValue = enforceUserStatus && rdr.FieldCount > nextFieldIndex && rdr[nextFieldIndex] != DBNull.Value
                        ? Convert.ToString(rdr[nextFieldIndex++]).Trim()
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

                    return new UserEntry(uname, hashAlg, hash, statusValue, failedAttempts, blockedUntilUtc, isLockedByDatabase);
                }
            }
        }

        public List<UserEntry> GetAllUserEntriesForCache()
        {
            EnsureConnection();

            bool includeStatus = Settings.IsUserStatusValidationEnabled();
            bool includeLockout = Settings.IsLoginLockoutEnabled();
            string table = Convert.ToString(Settings.Store.Table);
            string usernameColumn = Convert.ToString(Settings.Store.UsernameColumn);
            string hashMethodColumn = Convert.ToString(Settings.Store.HashMethodColumn);
            string passwordColumn = Convert.ToString(Settings.Store.PasswordColumn);
            string userStatusColumn = Settings.GetUserStatusColumn();
            string failedAttemptsColumn = Settings.GetFailedAttemptsColumn();
            string blockedUntilColumn = Settings.GetBlockedUntilColumn();
            string query = includeStatus && includeLockout
                ? string.Format("SELECT {1}, {2}, {3}, {4}, {5}, {6} FROM {0}",
                    Q(table),
                    Q(usernameColumn),
                    Q(hashMethodColumn),
                    Q(passwordColumn),
                    Q(userStatusColumn),
                    Q(failedAttemptsColumn),
                    Q(blockedUntilColumn))
                : includeStatus
                ? string.Format("SELECT {1}, {2}, {3}, {4} FROM {0}",
                    Q(table),
                    Q(usernameColumn),
                    Q(hashMethodColumn),
                    Q(passwordColumn),
                    Q(userStatusColumn))
                : includeLockout
                ? string.Format("SELECT {1}, {2}, {3}, {4}, {5} FROM {0}",
                    Q(table),
                    Q(usernameColumn),
                    Q(hashMethodColumn),
                    Q(passwordColumn),
                    Q(failedAttemptsColumn),
                    Q(blockedUntilColumn))
                : string.Format("SELECT {1}, {2}, {3} FROM {0}",
                    Q(table),
                    Q(usernameColumn),
                    Q(hashMethodColumn),
                    Q(passwordColumn));

            var users = new List<UserEntry>();

            using (var cmd = new NpgsqlCommand(query, m_conn))
            using (var rdr = cmd.ExecuteReader())
            {
                while (rdr.Read())
                {
                    string uname = Convert.ToString(rdr[0]);
                    string hashMethodStr = rdr[1] != DBNull.Value ? Convert.ToString(rdr[1]).ToUpperInvariant().Trim() : "NONE";
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

        public bool IsMemberOfGroup(string userName, string groupName)
        {
            EnsureConnection();
            string table = Convert.ToString(Settings.Store.Table);
            string usernameColumn = Convert.ToString(Settings.Store.UsernameColumn);
            string userPrimaryKeyColumn = Convert.ToString(Settings.Store.UserTablePrimaryKeyColumn);
            string groupTableName = Convert.ToString(Settings.Store.GroupTableName);
            string userGroupTableName = Convert.ToString(Settings.Store.UserGroupTableName);
            string userForeignKeyColumn = Convert.ToString(Settings.Store.UserForeignKeyColumn);
            string groupForeignKeyColumn = Convert.ToString(Settings.Store.GroupForeignKeyColumn);
            string groupTablePrimaryKeyColumn = Convert.ToString(Settings.Store.GroupTablePrimaryKeyColumn);
            string groupNameColumn = Convert.ToString(Settings.Store.GroupNameColumn);

            string userIdQuery = string.Format("SELECT {2} FROM {0} WHERE {1} = @user",
                Q(table),
                Q(usernameColumn),
                Q(userPrimaryKeyColumn));

            string userId = null;

            using (var cmd = new NpgsqlCommand(userIdQuery, m_conn))
            {
                cmd.Parameters.AddWithValue("@user", userName);
                using (var rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())
                        userId = Convert.ToString(rdr[0]);
                }
            }

            if (userId == null)
            {
                m_logger.DebugFormat("User {0} not found when checking PostgreSQL group membership", userName);
                return false;
            }

            string query = string.Format(
                "SELECT g.{5} " +
                "FROM {0} g " +
                "INNER JOIN {1} ug ON g.{4} = ug.{3} " +
                "WHERE ug.{2} = @userId",
                Q(groupTableName),
                Q(userGroupTableName),
                Q(userForeignKeyColumn),
                Q(groupForeignKeyColumn),
                Q(groupTablePrimaryKeyColumn),
                Q(groupNameColumn));

            using (var cmd = new NpgsqlCommand(query, m_conn))
            {
                cmd.Parameters.AddWithValue("@userId", userId);
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        if (Convert.ToString(rdr[0]).Equals(groupName, StringComparison.OrdinalIgnoreCase))
                        {
                            m_logger.DebugFormat("User {0} IS member of PostgreSQL group {1}", userName, groupName);
                            return true;
                        }
                    }
                }
            }

            m_logger.DebugFormat("User {0} is NOT member of PostgreSQL group {1}", userName, groupName);
            return false;
        }

        public void UpdateUserHash(string userName, string newHash, string hashMethod)
        {
            EnsureConnection();
            string table = Convert.ToString(Settings.Store.Table);
            string usernameColumn = Convert.ToString(Settings.Store.UsernameColumn);
            string hashMethodColumn = Convert.ToString(Settings.Store.HashMethodColumn);
            string passwordColumn = Convert.ToString(Settings.Store.PasswordColumn);

            string query = string.Format(
                "UPDATE {0} SET {2} = @hashMethod, {3} = @hash WHERE {1} = @user",
                Q(table),
                Q(usernameColumn),
                Q(hashMethodColumn),
                Q(passwordColumn));

            using (var cmd = new NpgsqlCommand(query, m_conn))
            {
                cmd.Parameters.AddWithValue("@user", userName);
                cmd.Parameters.AddWithValue("@hashMethod", hashMethod);
                cmd.Parameters.AddWithValue("@hash", newHash);
                cmd.ExecuteNonQuery();
            }
        }

        public void ResetFailedLoginState(string userName)
        {
            if (!Settings.IsLoginLockoutEnabled())
                return;

            EnsureConnection();
            string table = Convert.ToString(Settings.Store.Table);
            string usernameColumn = Convert.ToString(Settings.Store.UsernameColumn);
            string failedAttemptsColumn = Settings.GetFailedAttemptsColumn();
            string blockedUntilColumn = Settings.GetBlockedUntilColumn();
            string lastAttemptColumn = Settings.GetLastAttemptColumn();

            string query = string.Format(
                "UPDATE {0} SET {2} = 0, {3} = NULL, {4} = CURRENT_TIMESTAMP WHERE {1} = @user",
                Q(table),
                Q(usernameColumn),
                Q(failedAttemptsColumn),
                Q(blockedUntilColumn),
                Q(lastAttemptColumn));

            using (var cmd = new NpgsqlCommand(query, m_conn))
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

            EnsureConnection();
            string table = Convert.ToString(Settings.Store.Table);
            string usernameColumn = Convert.ToString(Settings.Store.UsernameColumn);
            string failedAttemptsColumn = Settings.GetFailedAttemptsColumn();
            string blockedUntilColumn = Settings.GetBlockedUntilColumn();
            string lastAttemptColumn = Settings.GetLastAttemptColumn();

            string updateQuery = string.Format(
                "UPDATE {0} " +
                "SET {2} = COALESCE({2}, 0) + 1, " +
                "{4} = CURRENT_TIMESTAMP, " +
                "{3} = CASE " +
                "WHEN COALESCE({2}, 0) + 1 >= @maxAttempts THEN CURRENT_TIMESTAMP + (@lockoutMinutes * INTERVAL '1 minute') " +
                "ELSE NULL END " +
                "WHERE {1} = @user",
                Q(table),
                Q(usernameColumn),
                Q(failedAttemptsColumn),
                Q(blockedUntilColumn),
                Q(lastAttemptColumn));

            using (var cmd = new NpgsqlCommand(updateQuery, m_conn))
            {
                cmd.Parameters.AddWithValue("@user", userName);
                cmd.Parameters.AddWithValue("@maxAttempts", Settings.GetMaxFailedAttempts());
                cmd.Parameters.AddWithValue("@lockoutMinutes", Settings.GetLockoutMinutes());
                cmd.ExecuteNonQuery();
            }

            string selectQuery = string.Format(
                "SELECT {1}, {2}, CASE WHEN {2} IS NOT NULL AND {2} > CURRENT_TIMESTAMP THEN 1 ELSE 0 END " +
                "FROM {0} WHERE {3} = @user",
                Q(table),
                Q(failedAttemptsColumn),
                Q(blockedUntilColumn),
                Q(usernameColumn));

            using (var cmd = new NpgsqlCommand(selectQuery, m_conn))
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

        private static string Q(string identifier)
        {
            return "\"" + (identifier ?? string.Empty).Replace("\"", "\"\"") + "\"";
        }

        private void EnsureConnection()
        {
            if (m_conn == null || m_conn.State != ConnectionState.Open)
                throw new InvalidOperationException("PostgreSQL connection is not open");
        }

        private static SslMode MapSslMode(MySqlConnector.MySqlSslMode sslMode)
        {
            switch (sslMode)
            {
                case MySqlConnector.MySqlSslMode.Required:
                case MySqlConnector.MySqlSslMode.VerifyCA:
                case MySqlConnector.MySqlSslMode.VerifyFull:
                    return SslMode.Require;
                default:
                    return SslMode.Disable;
            }
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
                    return true;
                default:
                    if (hash.Length == 60 &&
                        (hash.StartsWith("$2a$") || hash.StartsWith("$2b$") || hash.StartsWith("$2y$")))
                    {
                        hashAlg = PasswordHashAlgorithm.BCRYPT;
                        return true;
                    }

                    m_logger.ErrorFormat("Unrecognized hash method: {0} for user {1}", hashMethodStr, uname);
                    hashAlg = PasswordHashAlgorithm.NONE;
                    return false;
            }
        }

        private DateTime? ReadUtcDateTime(object value)
        {
            if (value == null || value == DBNull.Value)
                return null;

            if (value is DateTime dateTime)
                return dateTime.Kind == DateTimeKind.Utc ? dateTime : dateTime.ToUniversalTime();

            if (DateTime.TryParse(Convert.ToString(value), CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out DateTime parsed))
                return parsed.ToUniversalTime();

            return null;
        }
    }
}


