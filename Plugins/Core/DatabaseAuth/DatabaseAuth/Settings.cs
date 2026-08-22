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
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace OpenCredential.Plugin.DatabaseAuth
{
    /// <summary>
    /// Settings management class for the database authentication plugin.
    /// Contains all configuration options including security settings.
    /// Version: 4.1.0 - Fixed RuntimeBinderException issues
    /// </summary>
    public class Settings
    {
        public enum DatabaseProvider
        {
            MySql = 0,
            PostgreSql = 1
        }

        /// <summary>
        /// Encoding format for password hashes in the database.
        /// </summary>
        public enum HashEncoding { HEX = 0, BASE_64 = 1 };

        private static dynamic m_settings = new OpenCredential.Shared.Settings.OpenCredentialDynamicSettings(PluginImpl.PluginUuid);
        
        /// <summary>
        /// Dynamic settings store for accessing configuration values.
        /// </summary>
        public static dynamic Store
        {
            get { return m_settings; }
        }

        static Settings()
        {
            // =============================================
            // Database Connection Settings
            // =============================================
            m_settings.SetDefault("Host", "localhost");
            m_settings.SetDefault("Port", 3306);
            m_settings.SetDefault("DatabaseProvider", (int)DatabaseProvider.MySql);
            m_settings.SetDefault("UseSsl", false);
            m_settings.SetDefault("SslMode", MySqlConnector.MySqlSslMode.None.ToString());
            m_settings.SetDefault("User", "pgina_user");
            m_settings.SetDefaultEncryptedSetting("Password", string.Empty);
            m_settings.SetDefault("Database", "pgina_access_control");

            // Connection timeout settings (for MySQL 8/MariaDB)
            m_settings.SetDefault("ConnectionTimeout", 30);
            m_settings.SetDefault("CommandTimeout", 30);

            // =============================================
            // User Table Configuration
            // =============================================
            m_settings.SetDefault("Table", "users");
            m_settings.SetDefault("HashEncoding", (int)HashEncoding.HEX);
            m_settings.SetDefault("UsernameColumn", "username");
            m_settings.SetDefault("HashMethodColumn", "hash_method");
            m_settings.SetDefault("PasswordColumn", "password_hash");
            m_settings.SetDefault("UserTablePrimaryKeyColumn", "id");
            m_settings.SetDefault("EnforceUserStatus", true);
            m_settings.SetDefault("UserStatusColumn", "status");
            m_settings.SetDefault("UserActiveValue", "1");
            m_settings.SetDefault("EnableLoginLockout", false);
            m_settings.SetDefault("FailedAttemptsColumn", "failed_attempts");
            m_settings.SetDefault("BlockedUntilColumn", "locked_until");
            m_settings.SetDefault("LastAttemptColumn", "last_attempt_at");
            m_settings.SetDefault("MaxFailedAttempts", 5);
            m_settings.SetDefault("LockoutMinutes", 15);

            // =============================================
            // Group Table Configuration
            // =============================================
            m_settings.SetDefault("GroupTableName", "groups");
            m_settings.SetDefault("GroupNameColumn", "group_name");
            m_settings.SetDefault("GroupTablePrimaryKeyColumn", "group_id");

            // User-Group relationship table
            m_settings.SetDefault("UserGroupTableName", "user_groups");
            m_settings.SetDefault("UserForeignKeyColumn", "user_id");
            m_settings.SetDefault("GroupForeignKeyColumn", "group_id");

            // =============================================
            // Authorization Settings
            // =============================================
            m_settings.SetDefault("GroupAuthzRules", new string[] { (new GroupAuthzRule(true)).ToRegString() });
            m_settings.SetDefault("AuthzRequireMySqlAuth", false);

            // Gateway settings
            m_settings.SetDefault("GroupGatewayRules", new string[] { });
            m_settings.SetDefault("PreventLogonOnServerError", false);

            // =============================================
            // BCrypt Security Settings
            // =============================================
            m_settings.SetDefault("BCryptWorkFactor", 10);
            m_settings.SetDefault("MigrateToBCrypt", false);

            // Offline resilience
            m_settings.SetDefault("LocalCacheEnabled", true);
            m_settings.SetDefault("OfflineFallbackEnabled", true);
            m_settings.SetDefault("AllowOfflineBypassForAuthorization", true);
            m_settings.SetDefault("SyncIntervalMinutes", 5);
            m_settings.SetDefault("HealthCheckSeconds", 30);
            m_settings.SetDefault("LocalCachePath", string.Empty);
        }

        // =============================================
        // Helper Methods - Use these to avoid RuntimeBinderException
        // =============================================

        private static object UnwrapSettingValue(object value)
        {
            if (value == null)
                return null;

            PropertyInfo rawValueProperty = value.GetType().GetProperty("RawValue");
            return rawValueProperty != null
                ? rawValueProperty.GetValue(value, null)
                : value;
        }

        private static string GetStringSetting(string name, string defaultValue = "")
        {
            object value = UnwrapSettingValue(m_settings.GetSetting(name, defaultValue));
            return value == null ? defaultValue : Convert.ToString(value);
        }

        private static int GetIntSetting(string name, int defaultValue = 0)
        {
            object value = UnwrapSettingValue(m_settings.GetSetting(name, defaultValue));
            if (value == null)
                return defaultValue;

            if (value is int intValue)
                return intValue;

            if (value is long longValue)
                return Convert.ToInt32(longValue);

            if (value is string stringValue && int.TryParse(stringValue, out int parsed))
                return parsed;

            return Convert.ToInt32(value);
        }

        private static bool GetBoolSetting(string name, bool defaultValue = false)
        {
            object value = UnwrapSettingValue(m_settings.GetSetting(name, defaultValue));
            if (value == null)
                return defaultValue;

            if (value is bool boolValue)
                return boolValue;

            if (value is int intValue)
                return intValue != 0;

            if (value is string stringValue)
            {
                if (bool.TryParse(stringValue, out bool parsedBool))
                    return parsedBool;

                if (int.TryParse(stringValue, out int parsedInt))
                    return parsedInt != 0;
            }

            return Convert.ToBoolean(value);
        }

        /// <summary>
        /// Gets the configured BCrypt work factor, ensuring it's within valid range (4-12).
        /// </summary>
        public static int GetBCryptWorkFactor()
        {
            int workFactor = GetIntSetting("BCryptWorkFactor", 10);
            if (workFactor < 4 || workFactor > 12)
            {
                return 10; // Default safe value
            }
            return workFactor;
        }

        /// <summary>
        /// Checks if BCrypt hash migration is enabled.
        /// </summary>
        public static bool IsMigrationEnabled()
        {
            return GetBoolSetting("MigrateToBCrypt");
        }
        
        /// <summary>
        /// Gets the connection timeout setting.
        /// </summary>
        public static int GetConnectionTimeout()
        {
            return GetIntSetting("ConnectionTimeout", 30);
        }
        
        /// <summary>
        /// Gets the command timeout setting.
        /// </summary>
        public static int GetCommandTimeout()
        {
            return GetIntSetting("CommandTimeout", 30);
        }
        
        /// <summary>
        /// Gets the database port setting.
        /// </summary>
        public static int GetPort()
        {
            return GetIntSetting("Port", 3306);
        }

        public static DatabaseProvider GetDatabaseProvider()
        {
            return (DatabaseProvider)GetIntSetting("DatabaseProvider", (int)DatabaseProvider.MySql);
        }
        
        /// <summary>
        /// Gets the hash encoding setting.
        /// </summary>
        public static HashEncoding GetHashEncoding()
        {
            return (HashEncoding)GetIntSetting("HashEncoding", (int)HashEncoding.HEX);
        }
        
        /// <summary>
        /// Checks if SSL is enabled for database connection.
        /// </summary>
        public static bool IsSslEnabled()
        {
            return GetBoolSetting("UseSsl");
        }

        /// <summary>
        /// Gets the configured TLS mode, preserving compatibility with the legacy UseSsl flag.
        /// </summary>
        public static MySqlConnector.MySqlSslMode GetSslMode()
        {
            string configuredMode = GetStringSetting("SslMode");
            MySqlConnector.MySqlSslMode parsedMode;

            if (!string.IsNullOrEmpty(configuredMode) &&
                Enum.TryParse(configuredMode, true, out parsedMode))
            {
                return parsedMode;
            }

            return IsSslEnabled()
                ? MySqlConnector.MySqlSslMode.Required
                : MySqlConnector.MySqlSslMode.None;
        }
        
        /// <summary>
        /// Checks if authorization requires this plugin to authenticate first.
        /// </summary>
        public static bool IsAuthzRequireMySqlAuth()
        {
            return GetBoolSetting("AuthzRequireMySqlAuth");
        }
        
        /// <summary>
        /// Checks if logon should be prevented on server error.
        /// </summary>
        public static bool PreventLogonOnServerError()
        {
            return GetBoolSetting("PreventLogonOnServerError");
        }

        /// <summary>
        /// Checks if user status validation is enabled.
        /// </summary>
        public static bool IsUserStatusValidationEnabled()
        {
            return GetBoolSetting("EnforceUserStatus", true);
        }

        /// <summary>
        /// Gets the configured user status column.
        /// </summary>
        public static string GetUserStatusColumn()
        {
            return GetStringSetting("UserStatusColumn");
        }

        /// <summary>
        /// Gets the configured active value for the user status column.
        /// </summary>
        public static string GetUserActiveValue()
        {
            return GetStringSetting("UserActiveValue");
        }

        public static bool IsLocalCacheEnabled()
        {
            return GetBoolSetting("LocalCacheEnabled", true);
        }

        public static bool IsLoginLockoutEnabled()
        {
            return GetBoolSetting("EnableLoginLockout", true);
        }

        public static string GetFailedAttemptsColumn()
        {
            return GetStringSetting("FailedAttemptsColumn");
        }

        public static string GetBlockedUntilColumn()
        {
            return GetStringSetting("BlockedUntilColumn");
        }

        public static string GetLastAttemptColumn()
        {
            return GetStringSetting("LastAttemptColumn");
        }

        public static int GetMaxFailedAttempts()
        {
            return Math.Max(1, GetIntSetting("MaxFailedAttempts", 5));
        }

        public static int GetLockoutMinutes()
        {
            return Math.Max(1, GetIntSetting("LockoutMinutes", 15));
        }

        public static bool IsOfflineFallbackEnabled()
        {
            return GetBoolSetting("OfflineFallbackEnabled", true);
        }

        public static bool AllowOfflineBypassForAuthorization()
        {
            return GetBoolSetting("AllowOfflineBypassForAuthorization", true);
        }

        public static int GetSyncIntervalMinutes()
        {
            return Math.Max(1, GetIntSetting("SyncIntervalMinutes", 5));
        }

        public static int GetHealthCheckSeconds()
        {
            return Math.Max(5, GetIntSetting("HealthCheckSeconds", 30));
        }

        public static string GetLocalCachePath()
        {
            string configuredPath = GetStringSetting("LocalCachePath");
            if (!string.IsNullOrWhiteSpace(configuredPath))
            {
                string normalizedConfiguredPath = NormalizeProgramDataPath(
                    configuredPath,
                    "DatabaseAuth",
                    "databaseauth-cache.sqlite");

                if (!string.Equals(configuredPath, normalizedConfiguredPath, StringComparison.OrdinalIgnoreCase))
                {
                    TryMigratePath(configuredPath, normalizedConfiguredPath);
                    m_settings.SetSetting("LocalCachePath", normalizedConfiguredPath);
                }

                return normalizedConfiguredPath;
            }

            string defaultPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "OpenCredential",
                "DatabaseAuth",
                "databaseauth-cache.sqlite");

            string legacyPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "pGina",
                "MySQLAuth",
                "mysqlauth-cache.sqlite");

            TryMigratePath(legacyPath, defaultPath);
            return defaultPath;
        }

        private static string NormalizeProgramDataPath(string path, string productFolder, string fileName)
        {
            if (string.IsNullOrWhiteSpace(path))
                return path;

            string programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string legacyRoot = Path.Combine(programData, "pGina");
            string currentRoot = Path.Combine(programData, "OpenCredential");
            string expectedLegacyPath = Path.Combine(legacyRoot, productFolder, fileName);

            if (string.Equals(path, expectedLegacyPath, StringComparison.OrdinalIgnoreCase))
                return Path.Combine(currentRoot, productFolder, fileName);

            return path;
        }

        private static void TryMigratePath(string legacyPath, string newPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(legacyPath) || string.IsNullOrWhiteSpace(newPath))
                    return;

                if (!File.Exists(legacyPath) || File.Exists(newPath))
                    return;

                string newDirectory = Path.GetDirectoryName(newPath);
                if (!string.IsNullOrWhiteSpace(newDirectory) && !Directory.Exists(newDirectory))
                    Directory.CreateDirectory(newDirectory);

                File.Move(legacyPath, newPath);
            }
            catch
            {
                // Keep running with the new default path even if migration cannot be completed.
            }
        }
    }
}

