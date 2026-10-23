using System.Windows.Forms;
using OpenCredential.Plugin.Presentation;

namespace OpenCredential.Plugin.DatabaseLogger
{
    public partial class Configuration
    {
        private static string T(string key, string english) { return PluginPresentation.Text("DatabaseLogger", key, english); }

        private void InitializeLocalizedPresentation()
        {
            PluginPresentation.Apply(this, "DatabaseLogger");
            PluginPresentation.Stack(groupBox2, PluginPresentation.Fields(label1, hostTB, portLabel, portTB,
                m_providerLabel, m_providerCB, dbLabel, dbTB, label2, eventTableTB, label3, sessionTableTB,
                userLabel, userTB, passwdLabel, passwdTB, null, showPassCB));
            PluginPresentation.Stack(groupBox3, PluginPresentation.Actions(eventModeCB, sessionModeCB));
            PluginPresentation.Stack(eventsBox, PluginPresentation.Actions(logonEvtCB, logoffEvtCB, lockEvtCB, unlockEvtCB,
                consoleConnectEvtCB, consoleDisconnectEvtCB, remoteControlEvtCB, remoteConnectEvtCB, remoteDisconnectEvtCB));
            PluginPresentation.Stack(optionsBox, PluginPresentation.Fields(null, useModNameCB, null, offlineQueueEnabledCB,
                label4, healthCheckTB, label5, flushBatchTB, label6, offlineQueuePathTB, null, m_presenceTrackingEnabledCB,
                m_heartbeatIntervalLabel, m_heartbeatIntervalTB, m_presenceStatePathLabel, m_presenceStatePathTB,
                m_tlsLabel, m_sslModeCB));
            var body = new Panel { AutoScroll = true };
            PluginPresentation.Stack(body, groupBox2, groupBox3, eventsBox, optionsBox);
            PluginPresentation.Shell(this, body, testButton, createTableBtn, okBtn, cancelBtn);
        }

        private static string DiagnosticResult(string value)
        {
            if (value == null) return null;
            if (value.StartsWith("Error:", System.StringComparison.Ordinal) || value.StartsWith("Connection failed:", System.StringComparison.Ordinal))
                return T("Error.Database", "The database operation failed. Check connection settings, TLS and permissions.");
            string key;
            switch (value)
            {
                case "Connection successful, but table does not exist. Click 'Create Table'.": key = "Report.Missing"; break;
                case "Table exists but has invalid columns.": key = "Report.Invalid"; break;
                case "Table has incorrect columns.": key = "Report.Incorrect"; break;
                case "Table exists and is correct.": key = "Report.Correct"; break;
                case "Table already exists.": key = "Report.Exists"; break;
                case "Table created.": key = "Report.Created"; break;
                case "Table already exists and was updated.": key = "Report.Updated"; break;
                default:
                    const string prefix = "Table exists but is missing OpenCredential presence columns: ";
                    const string suffix = ". Click 'Create Table' to update it.";
                    if (value.StartsWith(prefix, System.StringComparison.Ordinal) && value.EndsWith(suffix, System.StringComparison.Ordinal))
                        return string.Format(T("Report.PresenceColumns", "Missing presence columns: {0}. Use Create Table to update the table."), value.Substring(prefix.Length, value.Length - prefix.Length - suffix.Length));
                    return value;
            }
            return T(key, value);
        }
    }
}
