/*
	Copyright (c) 2012, pGina Team
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

    public abstract class GroupRule
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

    public class GroupAuthzRule : GroupRule
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
                    str = string.Format("If member of LDAP group \"{0}\"", m_group);
                    break;
                case Condition.NOT_MEMBER_OF:
                    str = string.Format("If not member of LDAP group \"{0}\"", m_group);
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
                    str = string.Format("If member of LDAP group \"{0}\"", m_group);
                    break;
                case Condition.NOT_MEMBER_OF:
                    str = string.Format("If not member of LDAP group \"{0}\"", m_group);
                    break;
            }
            str += string.Format(" add to local group \"{0}\"", m_localGroup);
            return str;
        }
    }
}
