/*
	Copyright (c) 2017, pGina Team
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
using System.Collections;
using System.Text.RegularExpressions;
using System.DirectoryServices.Protocols;

using pGina.Shared.Types;
using log4net;

namespace pGina.Plugin.Ldap
{

    class GroupRuleLoader
    {
        private static ILog m_logger = LogManager.GetLogger("LDAP GroupRuleLoader");

        public static string SaveAuthzRules(List<GroupAuthzRule> rules)
        {
            string ret = "";
            List<string> strList = new List<string>();
            foreach (GroupRule rule in rules)
            {
                if (GroupAuthzRule.FromRegString(rule.ToRegString()) == null)
                {
                    ret += rule + "\n";
                    m_logger.ErrorFormat("Rule doesn't comply:{0}", rule);
                }
                else
                {
                    strList.Add(rule.ToRegString());
                }
            }
            Settings.Store.GroupAuthzRules = strList.ToArray();
            return ret;
        }

        public static string SaveGatewayRules(List<GroupGatewayRule> rules)
        {
            string ret = "";
            List<string> strList = new List<string>();
            foreach (GroupRule rule in rules)
            {
                if (GroupGatewayRule.FromRegString(rule.ToRegString()) == null)
                {
                    ret += rule + "\n";
                    m_logger.ErrorFormat("Rule doesn't comply:{0}", rule);
                }
                else
                {
                    strList.Add(rule.ToRegString());
                }
            }
            Settings.Store.GroupGatewayRules = strList.ToArray();
            return ret;
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

    public abstract class GroupRule
    {
        public Condition RuleCondition { get { return m_condition; } }
        protected Condition m_condition;

        public string path { get { return m_path; } }
        protected string m_path;

        public string filter { get { return m_filter; } }
        protected string m_filter;

        public SearchScope SearchScope { get { return m_searchscope; } }
        protected SearchScope m_searchscope;

        public GroupRule( string p, Condition c, string f, SearchScope s )
        {
            m_condition = c;
            m_searchscope = s;
            m_path = p;
            m_filter = f;
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

    public class GroupAuthzRule : GroupRule
    {
        public bool AllowOnMatch { get { return m_allowOnMatch; } }
        private bool m_allowOnMatch;

        public GroupAuthzRule(bool allow) : base("", Condition.ALWAYS, "", SearchScope.Base)
        {
            m_allowOnMatch = allow;
        }

        public GroupAuthzRule(string path, Condition c, bool allow, string filter, SearchScope scope) : base(path, c, filter, scope)
        {
            m_allowOnMatch = allow;
        }

        override public string ToRegString()
        {
            return string.Format("{0}\t{1}\t{2}\t{3}\t{4}", (int)m_condition, (int)m_searchscope, m_path, m_filter, m_allowOnMatch ? "1" : "0");
        }

        public static GroupAuthzRule FromRegString(string str)
        {
            str = str.Trim();
            if (Regex.IsMatch(Regex.Replace(str, @"(\\.)+", ""), @"[0-1]\t[0-2]\t([\w%]+=[^,\t]+,?)+\t\(.*\)\t[0|1]"))
            {
                string[] parts = Regex.Split(str, @"\t");
                if (parts.Length == 5)
                {
                    Condition c = (Condition)(Convert.ToInt32(parts[0]));
                    SearchScope scope = (SearchScope)Convert.ToInt32(parts[1]);
                    string path = parts[2];
                    string filter = parts[3];
                    bool allow = Convert.ToInt32(parts[4]) != 0;
                    return new GroupAuthzRule(path, c, allow, filter, scope);
                }
            }
            return null;
        }

        override public string ToString()
        {
            string sScope = "";
            switch ( SearchScope )
            {
                case System.DirectoryServices.Protocols.SearchScope.Base:
                    sScope = "Base";
                    break;
                case System.DirectoryServices.Protocols.SearchScope.OneLevel:
                    sScope = "OneLevel";
                    break;
                case System.DirectoryServices.Protocols.SearchScope.Subtree:
                    sScope = "Subtree";
                    break;
            }
            string str = "";
            switch( RuleCondition )
            {
                case Condition.ALWAYS:
                    str = "Always";
                    break;
                case Condition.MEMBER_OF:
                    str = string.Format("If \"{0}\" level {1} in \"{2}\"", m_filter, sScope, m_path);
                    break;
                case Condition.NOT_MEMBER_OF:
                    str = string.Format("!If \"{0}\" level {1} in \"{2}\"", m_filter, sScope, m_path);
                    break;
            }
            if (m_allowOnMatch) str += " allow.";
            else str += " deny.";

            return str;
        }
    }

    public class GroupGatewayRule : GroupRule
    {
        public string LocalGroup { get { return m_localGroup; } }
        private string m_localGroup;

        public GroupGatewayRule(string localGroup) : base("", Condition.ALWAYS, "", SearchScope.Base)
        {
            m_localGroup = localGroup;
        }
        public GroupGatewayRule(string path, Condition c, string addTo, string filter, SearchScope scope) : base(path, c, filter, scope)
        {
            m_localGroup = addTo;
        }

        public static GroupGatewayRule FromRegString(string str)
        {
            str = str.Trim();
            if (Regex.IsMatch(Regex.Replace(str, @"(\\.)+", ""), @"[0-1]\t[0-2]\t([\w%]+=[^,\t]+,?)+\t\(.*\)\t[\w-]+") || Regex.IsMatch(str, @"2\t0\t\t\t[\w-]+"))
            {
                string[] parts = Regex.Split(str, @"\t");
                if (parts.Length == 5)
                {
                    Condition c = (Condition)(Convert.ToInt32(parts[0]));
                    SearchScope scope = (SearchScope)Convert.ToInt32(parts[1]);
                    string path = parts[2];
                    string filter = parts[3];
                    string addTo = parts[4];
                    return new GroupGatewayRule(path, c, addTo, filter, scope);
                }
            }
            return null;
        }

        override public string ToRegString()
        {
            return string.Format("{0}\t{1}\t{2}\t{3}\t{4}", (int)m_condition, (int)m_searchscope, m_path, m_filter, m_localGroup);
        }

        override public string ToString()
        {
            string sScope = "";
            switch (SearchScope)
            {
                case System.DirectoryServices.Protocols.SearchScope.Base:
                    sScope = "Base";
                    break;
                case System.DirectoryServices.Protocols.SearchScope.OneLevel:
                    sScope = "OneLevel";
                    break;
                case System.DirectoryServices.Protocols.SearchScope.Subtree:
                    sScope = "Subtree";
                    break;
            }
            string str = "";
            switch (RuleCondition)
            {
                case Condition.ALWAYS:
                    str = "Always";
                    break;
                case Condition.MEMBER_OF:
                    str = string.Format("If \"{0}\" level {1} in \"{2}\"", m_filter, sScope, m_path);
                    break;
                case Condition.NOT_MEMBER_OF:
                    str = string.Format("!If \"{0}\" level {1} in \"{2}\"", m_filter, sScope, m_path);
                    break;
            }
            str += string.Format(" add to \"{0}\"", m_localGroup);

            return str;
        }
    }
}
