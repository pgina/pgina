using System;
using System.Globalization;
using System.Windows.Forms;
using OpenCredential.Plugin.Presentation;

namespace OpenCredential.Plugin.DatabaseAuth
{
    public partial class Configuration
    {
        private static string T(string key, string english) { return PluginPresentation.Text("DatabaseAuth", key, english); }

        private void InitializeLocalizedPresentation()
        {
            PluginPresentation.Apply(this, "DatabaseAuth");
            foreach (var combo in new[] { cbAuthzMySqlGroupMemberOrNot, cbAuthzGroupRuleAllowOrDeny, gtwRuleConditionCB }) PluginPresentation.Options(combo, "DatabaseAuth");
            foreach (var list in new[] { listBoxAuthzRules, gtwRulesListBox })
            { list.FormattingEnabled = true; list.Format += delegate(object sender, ListControlConvertEventArgs e) { e.Value = RuleText(e.ListItem); }; }
            PluginPresentation.Stack(groupBox1, PluginPresentation.Fields(hostLabel, hostTB, label1, portTB,
                m_providerLabel, m_providerCB, label21, sslModeCB, label2, userTB, label3, passwordTB, label4, dbTB, null, passwdCB));
            PluginPresentation.Stack(groupBoxOffline, PluginPresentation.Fields(null, localCacheEnabledCB,
                null, offlineFallbackEnabledCB, null, offlineBypassCB, label27, syncIntervalTB, label28, healthCheckTB, label29, cachePathTB));
            PluginPresentation.Stack(tabPage6, groupBox1, groupBoxOffline);
            PluginPresentation.Stack(groupBox3, PluginPresentation.Fields(label7, unameColTB, label8, hashMethodColTB,
                label9, passwdColTB, label10, userPrimaryKeyColTB, null, enforceStatusCB, label22, statusColTB, label23, activeValueTB));
            PluginPresentation.Stack(groupBoxLockout, PluginPresentation.Fields(null, lockoutEnabledCB, label30, maxFailedAttemptsTB,
                label31, lockoutMinutesTB, label32, failedAttemptsColTB, label33, blockedUntilColTB, label34, lastAttemptColTB));
            PluginPresentation.Stack(tabPage1, PluginPresentation.Fields(label5, userTableTB, label6,
                PluginPresentation.Actions(encHexRB, encBase64RB), null, label35), groupBox3, groupBoxLockout);
            PluginPresentation.Stack(groupBox4, PluginPresentation.Fields(label12, groupNameColTB, label13, groupTablePrimaryKeyColTB));
            PluginPresentation.Stack(tabPage2, PluginPresentation.Fields(label11, groupTableNameTB), groupBox4);
            PluginPresentation.Stack(groupBox5, PluginPresentation.Fields(label15, userGroupUserFKColTB, label16, userGroupGroupFKColTB));
            PluginPresentation.Stack(tabPage3, PluginPresentation.Fields(label14, userGroupTableNameTB), groupBox5);
            tabControlDBSchema.Dock = DockStyle.Fill;
            tabControlDBSchema.Multiline = true;
            tabControl1.Multiline = true;
            listBoxAuthzRules.Height = 160;
            gtwRulesListBox.Height = 160;
            btnAuthzGroupRuleUp.Text = T("Action.Up", "Move up");
            btnAuthzGroupRuleDown.Text = T("Action.Down", "Move down");
            btnAuthzGroupRuleDelete.Text = gtwRuleDeleteBtn.Text = T("Action.Delete", "Delete rule");
            foreach (var button in new[] { btnAuthzGroupRuleUp, btnAuthzGroupRuleDown, btnAuthzGroupRuleDelete, gtwRuleDeleteBtn })
            { button.Image = null; button.AccessibleName = button.Text; }
            PluginPresentation.Stack(tabAuthz, PluginPresentation.Fields(null, ckDenyWhenMySqlAuthFails,
                null, cbAuthzMySqlGroupMemberOrNot, null, tbAuthzRuleGroup, null, cbAuthzGroupRuleAllowOrDeny,
                null, PluginPresentation.Actions(btnAuthzGroupRuleAdd), label19, PluginPresentation.Actions(rbDefaultAllow, rbDefaultDeny),
                null, label20, null, listBoxAuthzRules, null, PluginPresentation.Actions(btnAuthzGroupRuleUp, btnAuthzGroupRuleDown, btnAuthzGroupRuleDelete)));
            PluginPresentation.Stack(tabPage5, PluginPresentation.Fields(null, m_preventLogonWhenServerUnreachableCb,
                null, gtwRuleConditionCB, null, gtwRuleMysqlGroupTB, label17, gtwRuleLocalGroupTB,
                null, PluginPresentation.Actions(gtwRuleAddBtn), null, label18, null, gtwRulesListBox, null, PluginPresentation.Actions(gtwRuleDeleteBtn)));
            PluginPresentation.Shell(this, tabControl1, testBtn, createTableBtn, saveButton, cancelButton);
        }

        private static string OfflineDisplay(OfflineCacheStatus status)
        {
            return string.Format(CultureInfo.InvariantCulture, T("Offline.Summary",
                "Local copy: {0}\r\nCached users: {1}; records within the age limit: {2}\r\nLast complete synchronization: {3}\r\nLast attempt: {4} ({5})\r\nRecord freshness does not override account status, lockout, password or authorization rules."),
                status.Exists ? T("Offline.Available", "available") : T("Offline.NotCreated", "not created"),
                status.UserCount, status.FreshUserCount, OfflineDate(status.LastFullSyncUtc), OfflineDate(status.LastAttemptUtc),
                T("Offline.Result." + (status.LastAttemptResult ?? "none"), status.LastAttemptResult ?? "no recorded attempt"));
        }

        private static string OfflineDate(DateTime? value)
        {
            return value.HasValue ? value.Value.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss 'UTC'", CultureInfo.InvariantCulture) : T("Offline.NotRecorded", "not recorded");
        }

        private static string DatabaseErrorMessage(Exception error)
        {
            // Do not render provider exception messages or connection strings in a translated dialog.
            string code = error is MySqlConnector.MySqlException ? ((MySqlConnector.MySqlException)error).Number.ToString(CultureInfo.InvariantCulture) :
                error is Npgsql.PostgresException ? ((Npgsql.PostgresException)error).SqlState : error.GetType().Name;
            return string.Format(CultureInfo.InvariantCulture, T("Error.Database", "Database operation failed ({0}). Check connection settings, TLS, schema and permissions."), code);
        }

        private static string RuleText(object item)
        {
            var rule = item as GroupRule;
            if (rule == null) return System.Convert.ToString(item);
            string condition = rule.RuleCondition == GroupRule.Condition.ALWAYS ? T("Rule.Always", "Always") :
                string.Format(T(rule.RuleCondition == GroupRule.Condition.MEMBER_OF ? "Rule.Member" : "Rule.NotMember",
                    rule.RuleCondition == GroupRule.Condition.MEMBER_OF ? "If member of group \"{0}\"" : "If not member of group \"{0}\""), rule.Group);
            var auth = rule as GroupAuthzRule;
            if (auth != null) return condition + " " + T(auth.AllowOnMatch ? "Rule.Allow" : "Rule.Deny", auth.AllowOnMatch ? "allow." : "deny.");
            return condition + string.Format(T("Rule.AddGroup", " add to local group \"{0}\""), ((GroupGatewayRule)rule).LocalGroup);
        }
    }
}
