using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Text.RegularExpressions;

using pGina.Shared.Types;

using log4net;

namespace pGina.Plugin.Ldap
{
    
    class GroupRuleLoader 
    {
        private static ILog m_logger = LogManager.GetLogger("LDAP GroupRuleLoader");

        public static void SaveAuthzRules(List<GroupAuthzRule> rules)
        {
            List<string> strList = new List<string>();
            foreach (GroupRule rule in rules)
            {
                strList.Add(rule.ToRegString());
            }
            Settings.Store.GroupAuthzRules = strList.ToArray();
        }

        public static void SaveGatewayRules(List<GroupGatewayRule> rules)
        {
            List<string> strList = new List<string>();
            foreach (GroupRule rule in rules)
            {
                strList.Add(rule.ToRegString());
            }
            Settings.Store.GroupGatewayRules = strList.ToArray();
        }

        public static List<GroupAuthzRule> GetAuthzRules()
        {
            List<GroupAuthzRule> rules = new List<GroupAuthzRule>();
            string[] strRules = Settings.Store.GroupAuthzRules;

            foreach (string str in strRules)
            {
                GroupAuthzRule rule = GroupAuthzRule.FromRegString(str);
                if (rule != null)
                    rules.Add(rule);
                else
                    // Log error
                    m_logger.ErrorFormat("Unrecognized registry entry when loading authorization rule, ignoring: {0}", str);
            }
            return rules;
        }

        public static List<GroupGatewayRule> GetGatewayRules()
        {
            List<GroupGatewayRule> rules = new List<GroupGatewayRule>();
            string[] strRules = Settings.Store.GroupGatewayRules;
            foreach (string str in strRules)
            {
                GroupGatewayRule rule = GroupGatewayRule.FromRegString(str);
                if( rule != null )
                    rules.Add(rule);
                else
                    // Log error
                    m_logger.ErrorFormat("Unrecognized registry entry when loading gateway rule, ignoring: {0}", str);
            }
            return rules;
        }
    }

    abstract class GroupRule
    {
        public string Group { get { return m_group; } }
        protected string m_group;

        public Condition RuleCondition { get { return m_condition; } }
        protected Condition m_condition;

        public GroupRule( string grp, Condition c )
        {
            m_group = grp;
            m_condition = c;
        }

        public bool RuleMatch(bool userIsMember)
        {
            switch( RuleCondition )
            {
                case Condition.ALWAYS: return true;
                case Condition.MEMBER_OF: return userIsMember;
                case Condition.NOT_MEMBER_OF: return !userIsMember;
                default: return false;
            }
        }

        public enum Condition { MEMBER_OF, NOT_MEMBER_OF, ALWAYS }        
        public abstract string ToRegString();
    }

    class GroupAuthzRule : GroupRule
    {
        public bool AllowOnMatch { get { return m_allowOnMatch; } }
        private bool m_allowOnMatch;

        public GroupAuthzRule(bool allow) : base("", Condition.ALWAYS)
        {
            m_allowOnMatch = allow;
        }

        public GroupAuthzRule(string grp, Condition c, bool allow) : base(grp, c)
        {
            m_allowOnMatch = allow;
        }

        override public string ToRegString()
        {
            return string.Format("{0}\n{1}\n{2}", m_group, ((int)RuleCondition),(m_allowOnMatch ? "1" : "0") );
        }

        public static GroupAuthzRule FromRegString(string str)
        {
            string[] parts = Regex.Split(str, @"\n");
            if (parts.Length == 3)
            {
                string grp = parts[0];
                Condition c = (Condition)(Convert.ToInt32(parts[1]));
                bool allow = Convert.ToInt32(parts[2]) != 0;
                return new GroupAuthzRule(grp, c, allow);
            }
            return null;
        }

        override public string ToString()
        {
            string str = "";
            switch( RuleCondition )
            {
                case Condition.ALWAYS:
                    str = "Always";
                    break;
                case Condition.MEMBER_OF:
                    str = "If member of LDAP group " + m_group;
                    break;
                case Condition.NOT_MEMBER_OF:
                    str = "If not member of LDAP group " + m_group;
                    break;
            }
            if (m_allowOnMatch) str += " allow.";
            else str += " deny.";

            return str;
        }
    }

    class GroupGatewayRule : GroupRule
    {
        public string LocalGroup { get { return m_localGroup; } }
        private string m_localGroup;

        public GroupGatewayRule(string localGroup) : base("", Condition.ALWAYS)
        {
            m_localGroup = localGroup;
        }
        public GroupGatewayRule(string grp, Condition c, string addTo) : base(grp,c)
        {
            m_localGroup = addTo;
        }

        public static GroupGatewayRule FromRegString(string str)
        {
            string[] parts = Regex.Split(str, @"\n");
            if (parts.Length == 3)
            {
                string grp = parts[0];
                Condition c = (Condition)(Convert.ToInt32(parts[1]));
                string addTo = parts[2];
                return new GroupGatewayRule(grp, c, addTo);
            }
            return null;
        }

        override public string ToRegString()
        {
            return m_group + "\n" + ((int)RuleCondition) + "\n" + m_localGroup;
        }

        override public string ToString()
        {
            string str = "";
            switch (RuleCondition)
            {
                case Condition.ALWAYS:
                    str = "Always";
                    break;
                case Condition.MEMBER_OF:
                    str = "If member of LDAP group " + m_group;
                    break;
                case Condition.NOT_MEMBER_OF:
                    str = "If not member of LDAP group " + m_group;
                    break;
            }
            str += " add to local group " + m_localGroup;
            return str;
        }
    }
}
