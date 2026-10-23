using System.Windows.Forms;
using OpenCredential.Plugin.Presentation;

namespace OpenCredential.Plugin.Ldap
{
    public partial class Configuration
    {
        private static string T(string key, string english) { return PluginPresentation.Text("Ldap", key, english); }

        private void InitializeLocalizedPresentation()
        {
            PluginPresentation.Apply(this, "Ldap");
            foreach (var combo in new[] { authzRuleMemberComboBox, authzRuleActionComboBox, gatewayRuleGroupMemberCB, m_encryptionMethodCb, DereferenceComboBox }) PluginPresentation.Options(combo, "Ldap");
            foreach (var list in new[] { authzRulesListBox, gatewayRulesListBox })
            { list.FormattingEnabled = true; list.Format += delegate(object sender, ListControlConvertEventArgs e) { e.Value = RuleText(e.ListItem); }; }
            PluginPresentation.Stack(ldapServerGroupBox, PluginPresentation.Fields(ldapHostDescriptionLabel, ldapHostTextBox,
                ldapPortLabel, ldapPortTextBox, timeoutLabel, timeoutTextBox, label10, m_encryptionMethodCb,
                null, validateServerCertCheckBox, label1, sslCertFileTextBox, null, PluginPresentation.Actions(sslCertFileBrowseButton)));
            PluginPresentation.Stack(groupBox1, PluginPresentation.Fields(null, label12, label2, searchDnTextBox,
                label3, searchPassTextBox, null, showPwCB, null, m_useAuthBindForAuthzAndGatewayCb));
            PluginPresentation.Stack(groupBox3, PluginPresentation.Fields(label8, DereferenceComboBox));
            PluginPresentation.Stack(groupBox4, PluginPresentation.Fields(label4, groupDNPattern, label5, groupMemberAttrTB,
                label11, groupGidAttrIU, label13, groupGidAttr, null, label14));
            PluginPresentation.Stack(tabPage1, ldapServerGroupBox, groupBox1, groupBox3, groupBox4);
            PluginPresentation.Stack(authTabPage, PluginPresentation.Fields(null, allowEmptyPwCB, dnPatternLabel, dnPatternTextBox,
                null, searchForDnCheckBox, searchFilterLabel, searchFilterTextBox, searchContextsLabel, searchContextsTextBox));
            authzRulesListBox.Height = gatewayRulesListBox.Height = passwordAttributesDGV.Height = 180;
            authzRuleUpBtn.Text = T("Action.Up", "Move up");
            authzRuleDownBtn.Text = T("Action.Down", "Move down");
            authzRuleDeleteBtn.Text = gatewayRuleDeleteBtn.Text = T("Action.Delete", "Delete rule");
            foreach (var button in new[] { authzRuleUpBtn, authzRuleDownBtn, authzRuleDeleteBtn, gatewayRuleDeleteBtn })
            { button.Image = null; button.AccessibleName = button.Text; }
            PluginPresentation.Stack(authzTabPage, PluginPresentation.Fields(null, authzApplyToAllUsersCB, null, authzRequireAuthCB,
                null, authzAllowOnErrorCB, null, authzRuleMemberComboBox, null, authzRuleGroupTB, null, authzRuleActionComboBox,
                null, PluginPresentation.Actions(authzRuleAddButton), label7, PluginPresentation.Actions(authzDefaultAllowRB, authzDefaultDenyRB),
                null, authzRulesListBox, null, PluginPresentation.Actions(authzRuleUpBtn, authzRuleDownBtn, authzRuleDeleteBtn)));
            PluginPresentation.Stack(gatewayTabPage, PluginPresentation.Fields(null, gatewayRuleGroupMemberCB, null, gatwayRemoteGroupTB,
                label6, gatewayLocalGroupTB, null, PluginPresentation.Actions(addGatewayGroupRuleButton), null, gatewayRulesListBox,
                null, PluginPresentation.Actions(gatewayRuleDeleteBtn)));
            passwordAttributesDGV.StandardTab = true;
            passwordAttributesDGV.AccessibleName = T("Control.label9", "Password Attributes:");
            PluginPresentation.Stack(tabPageChangePassword, label9, passwordAttributesDGV, PluginPresentation.Actions(changePasswordDeleteAttribBtn));
            tabControl1.Multiline = true;
            PluginPresentation.Shell(this, tabControl1, saveButton, cancelButton);
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
