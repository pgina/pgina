using System;
using System.IO;
using Microsoft.Win32;
using OpenCredential.Shared.Settings;

namespace OpenCredential.Configuration
{
    internal static class ConfigurationAccessPolicy
    {
        internal const string SettingName = "RequireConfigurationReauthentication";

        // Read-only: startup must not initialize plugins or write default settings before access is checked.
        internal static object ReadValue()
        {
            using (var key = Registry.LocalMachine.OpenSubKey(OpenCredentialDynamicSettings.OpenCredentialRoot, false))
                return key == null ? null : key.GetValue(SettingName, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
        }

        internal static bool Parse(object value)
        {
            if (value == null) return false;
            if (value is bool) return (bool)value;
            if (value is int && ((int)value == 0 || (int)value == 1)) return (int)value == 1;
            bool parsed;
            if (value is string && bool.TryParse((string)value, out parsed)) return parsed;
            // Corrupt/unreadable policy must not silently disable an enabled security option.
            throw new InvalidDataException("Invalid configuration access policy.");
        }

        internal static bool Authorize(bool isAdministrator, Func<object> readPolicy, Func<bool> reauthenticate)
        {
            if (!isAdministrator) return false;
            return !Parse(readPolicy()) || reauthenticate();
        }
    }
}
