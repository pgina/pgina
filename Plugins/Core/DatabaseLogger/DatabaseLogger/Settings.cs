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
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

using OpenCredential.Shared.Settings;

namespace OpenCredential.Plugin.DatabaseLogger
{
    class Settings
    {
        public enum DatabaseProvider
        {
            MySql = 0,
            PostgreSql = 1
        }

        private static dynamic m_settings = new OpenCredentialDynamicSettings(PluginImpl.PluginUuid);
        public static dynamic Store
        {
            get { return m_settings; }
        }

        static Settings()
        {
            // Set defaults
            //m_settings.SetDefault("LoggerMode", LoggerMode.EVENT);
            m_settings.SetDefault("EventMode", true);
            m_settings.SetDefault("SessionMode", true);
            m_settings.SetDefault("Host", "localhost");
            m_settings.SetDefault("Port", 3306);
            m_settings.SetDefault("DatabaseProvider", (int)DatabaseProvider.MySql);
            m_settings.SetDefault("User", "pGina");
            m_settings.SetDefaultEncryptedSetting("Password", string.Empty, null);
            m_settings.SetDefault("Database", "pgina_access_control");
            m_settings.SetDefault("SessionTable", "login_sessions");
            m_settings.SetDefault("EventTable", "login_events");

            m_settings.SetDefault("EvtLogon", true);
            m_settings.SetDefault("EvtLogoff", true);
            m_settings.SetDefault("EvtLock", false);
            m_settings.SetDefault("EvtUnlock", false);
            m_settings.SetDefault("EvtConsoleConnect",false);
            m_settings.SetDefault("EvtConsoleDisconnect", false);
            m_settings.SetDefault("EvtRemoteControl", false);
            m_settings.SetDefault("EvtRemoteConnect", false);
            m_settings.SetDefault("EvtRemoteDisconnect", false);

            m_settings.SetDefault("UseModifiedName", false);
            m_settings.SetDefault("OfflineQueueEnabled", true);
            m_settings.SetDefault("HealthCheckSeconds", 30);
            m_settings.SetDefault("PresenceTrackingEnabled", true);
            m_settings.SetDefault("HeartbeatIntervalSeconds", 60);
            m_settings.SetDefault("PresenceLeaseTimeoutSeconds", 300);
            m_settings.SetDefault("FlushBatchSize", 100);
            m_settings.SetDefault("OfflineQueuePath", string.Empty);
            m_settings.SetDefault("PresenceStatePath", string.Empty);
        }

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

        public static uint GetPort()
        {
            return Convert.ToUInt32(GetIntSetting("Port", 3306));
        }

        public static DatabaseProvider GetDatabaseProvider()
        {
            return (DatabaseProvider)GetIntSetting("DatabaseProvider", (int)DatabaseProvider.MySql);
        }

        public static bool GetEventMode()
        {
            return GetBoolSetting("EventMode", true);
        }

        public static bool GetSessionMode()
        {
            return GetBoolSetting("SessionMode");
        }

        public static bool GetUseModifiedName()
        {
            return GetBoolSetting("UseModifiedName");
        }

        public static bool GetEvtLogon()
        {
            return GetBoolSetting("EvtLogon", true);
        }

        public static bool GetEvtLogoff()
        {
            return GetBoolSetting("EvtLogoff", true);
        }

        public static bool GetEvtLock()
        {
            return GetBoolSetting("EvtLock");
        }

        public static bool GetEvtUnlock()
        {
            return GetBoolSetting("EvtUnlock");
        }

        public static bool GetEvtConsoleConnect()
        {
            return GetBoolSetting("EvtConsoleConnect");
        }

        public static bool GetEvtConsoleDisconnect()
        {
            return GetBoolSetting("EvtConsoleDisconnect");
        }

        public static bool GetEvtRemoteControl()
        {
            return GetBoolSetting("EvtRemoteControl");
        }

        public static bool GetEvtRemoteConnect()
        {
            return GetBoolSetting("EvtRemoteConnect");
        }

        public static bool GetEvtRemoteDisconnect()
        {
            return GetBoolSetting("EvtRemoteDisconnect");
        }

        public static bool IsOfflineQueueEnabled()
        {
            return GetBoolSetting("OfflineQueueEnabled", true);
        }

        public static int GetHealthCheckSeconds()
        {
            return Math.Max(5, GetIntSetting("HealthCheckSeconds", 30));
        }

        public static bool IsPresenceTrackingEnabled()
        {
            return GetBoolSetting("PresenceTrackingEnabled", true);
        }

        public static int GetHeartbeatIntervalSeconds()
        {
            return Math.Max(15, GetIntSetting("HeartbeatIntervalSeconds", 60));
        }

        public static int GetPresenceLeaseTimeoutSeconds()
        {
            int configured = GetIntSetting("PresenceLeaseTimeoutSeconds", 300);
            return Math.Max(GetHeartbeatIntervalSeconds() * 3, Math.Max(60, configured));
        }

        public static int GetFlushBatchSize()
        {
            return Math.Max(1, GetIntSetting("FlushBatchSize", 100));
        }

        public static string GetOfflineQueuePath()
        {
            string configuredPath = GetStringSetting("OfflineQueuePath");
            if (!string.IsNullOrWhiteSpace(configuredPath))
            {
                string normalizedConfiguredPath = NormalizeProgramDataPath(
                    configuredPath,
                    "DatabaseLogger",
                    "databaselogger-queue.sqlite");

                if (!string.Equals(configuredPath, normalizedConfiguredPath, StringComparison.OrdinalIgnoreCase))
                {
                    TryMigratePath(configuredPath, normalizedConfiguredPath);
                    m_settings.SetSetting("OfflineQueuePath", normalizedConfiguredPath);
                }

                return normalizedConfiguredPath;
            }

            string defaultPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "OpenCredential",
                "DatabaseLogger",
                "databaselogger-queue.sqlite");

            string legacyPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "pGina",
                "MySQLLogger",
                "mysqllogger-queue.sqlite");

            TryMigratePath(legacyPath, defaultPath);
            return defaultPath;
        }

        public static string GetPresenceStatePath()
        {
            string configuredPath = GetStringSetting("PresenceStatePath");
            if (!string.IsNullOrWhiteSpace(configuredPath))
            {
                string normalizedConfiguredPath = NormalizeProgramDataPath(
                    configuredPath,
                    "DatabaseLogger",
                    "databaselogger-state.sqlite");

                if (!string.Equals(configuredPath, normalizedConfiguredPath, StringComparison.OrdinalIgnoreCase))
                {
                    TryMigratePath(configuredPath, normalizedConfiguredPath);
                    m_settings.SetSetting("PresenceStatePath", normalizedConfiguredPath);
                }

                return normalizedConfiguredPath;
            }

            string defaultPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "OpenCredential",
                "DatabaseLogger",
                "databaselogger-state.sqlite");

            string legacyPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "pGina",
                "MySQLLogger",
                "mysqllogger-state.sqlite");

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

