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
using System.Text.RegularExpressions;

using log4net;

using pGina.Shared.Settings;

namespace pGina.Plugin.UsernameMod.Rules
{

    public enum Stage { Authentication, Authorization, Gateway };
    
    /// <summary>
    /// Represents an action to take on the username.
    /// </summary>
    public interface IUsernameRule
    {
        Stage stage { get; set; }

        /// <summary>
        /// Generates a string representation of the rule.
        /// </summary>
        /// <returns></returns>
        string save();
    }

    /// <summary>
    /// IModifyRule represents a rule that will modify the username.
    /// </summary>
    public interface IModifyRule : IUsernameRule
    {
        string modify(string username);
    }

    /// <summary>
    /// IMatchRule represents a rule that will match against the username
    /// </summary>
    public interface IMatchRule : IUsernameRule
    {
        bool match(string username);
    }

    /// <summary>
    /// AppendUsername will append the specified value to the username.
    /// </summary>
    public class AppendUsername : IModifyRule
    {
        public Stage stage { get;  set; }
        public string toAppend { get; protected set; }


        public AppendUsername(Stage stage, string toAppend)
        {
            this.stage = stage;
            this.toAppend = toAppend;
        }

        public string modify(string username)
        {
            return string.Format("{0}{1}", username, toAppend);
        }

        public override string ToString()
        {
            return string.Format("[{0}] The username will be appended with \"{1}\"", stage, toAppend);
        }

        public string save()
        {
            return string.Format("{0}\n{1}\n{2}", stage, "Append", toAppend);
        }
    }

    /// <summary>
    /// PrependUsername will prepend the specified value to the username.
    /// </summary>
    public class PrependUsername : IModifyRule
    {
        public Stage stage { get;  set; }
        public string toPrepend { get; protected set; }

        public PrependUsername(Stage stage, string toPrepend)
        {
            this.stage = stage;
            this.toPrepend = toPrepend;
        }

        public string modify(string username)
        {
            return string.Format("{0}{1}", toPrepend, username);
        }

        public override string ToString()
        {
            return string.Format("[{0}] The username will be prepended with \"{1}\"", stage, toPrepend);
        }
        
        public string save()
        {
            return string.Format("{0}\n{1}\n{2}", stage, "Prepend", toPrepend);
        }
    }

    /// <summary>
    /// TruncateUsername will truncate the username to the specified number of characters if it is over.
    /// </summary>
    public class TruncateUsername : IModifyRule
    {
        public Stage stage { get;  set; }
        public int numChars { get; protected set; }

        public TruncateUsername(Stage stage, int numChars)
        {
            this.stage = stage;
            this.numChars = numChars;
        }

        public TruncateUsername(Stage stage, string numChars)
        {
            this.stage = stage;
            try
            {
                this.numChars = Convert.ToInt32(numChars);
            }
            catch (FormatException e)
            {
                throw new UsernameModPluginException("The number of characters must be an integer value.", e);
            }
        }

        public string modify(string username)
        {
            if(username.Length >= numChars)
                return username.Substring(0, numChars);
            return username;
        }

        public override string ToString()
        {
            return string.Format("[{0}] The username will be truncated to {1} characters.", stage, numChars);
        }

        public string save()
        {
            return string.Format("{0}\n{1}\n{2}", stage, "Truncate", numChars);
        }

    }

    /// <summary>
    /// ReplaceUsername will replace the specified characters with a specified string.
    /// </summary>
    public class ReplaceUsername : IModifyRule
    {
        public Stage stage { get;  set; }
        public string charsToReplace { get; protected set; }
        public string replaceWith { get; protected set; }

        public ReplaceUsername(Stage stage, string toReplace, string toReplaceWith)
        {
            this.stage = stage;
            charsToReplace = toReplace;
            replaceWith = toReplaceWith;
        }

        public string modify(string username)
        {
            string modifiedUsername = username;
            foreach (char c in charsToReplace)
            {
                modifiedUsername = modifiedUsername.Replace(c.ToString(), replaceWith);
            }
            return modifiedUsername;
        }

        public override string ToString()
        {
            return string.Format("[{0}] Each character in \"{1}\" will be replaced with \"{2}\"", stage, charsToReplace, replaceWith);
        }

        public string save()
        {
            return string.Format("{0}\n{1}\n{2}\n{3}", stage, "Replace", charsToReplace, replaceWith);
        }
    }

    /// <summary>
    /// RegexReplaceUsername will take a regex pattern and replace all matches with a specified string.
    /// </summary>
    public class RegexReplaceUsername : IModifyRule
    {
        public Stage stage { get;  set; }
        public string pattern { get; protected set; }
        public string replaceWith { get; protected set; }

        public RegexReplaceUsername(Stage stage, string regexPattern, string replaceWith)
        {
            this.stage = stage;
            this.pattern = regexPattern;
            this.replaceWith = replaceWith;
        }

        public string modify(string username)
        {
            return Regex.Replace(username, pattern, replaceWith); 
        }

        public override string ToString()
        {
            return string.Format("[{0}] Each regex match for \"{1}\" will be replaced with \"{2}\"", stage, pattern, replaceWith);
        }

        public string save()
        {
            return string.Format("{0}\n{1}\n{2}\n{3}", stage, "RegEx Replace", pattern, replaceWith);
        }
    }

    /// <summary>
    /// MatchRegex will match against a regular expression.
    /// </summary>
    public class MatchRegex : IMatchRule
    {
        public Stage stage { get; set; }
        public string pattern { get; protected set; }

        public MatchRegex(Stage stage, string pattern)
        {
            this.stage = stage;
            this.pattern = pattern;
        }

        public bool match(string username)
        {
            return Regex.Match(username, pattern).Success;
        }

        public override string ToString()
        {
            return string.Format("[{0}] Username must match the regex pattern \"{1}\"", stage, pattern);
        }
        public string save()
        {
            return string.Format("{0}\n{1}\n{2}", stage, "Match", pattern);
        }
    }

    class ListOfRules
    {
        private static dynamic m_settings = new pGinaDynamicSettings(UsernameModPlugin.SimpleUuid);
        public List<IUsernameRule> list { get; private set; }
        private ILog m_logger = LogManager.GetLogger("UsernameModPlugin");
        
        public static string[] Rules = { "Append", "Prepend", "Truncate", "Replace", "RegEx Replace", "Match" };

        public ListOfRules()
        {
            //Read registry key, load rules
            m_settings.SetDefault("rules", new string[]{});
            list = new List<IUsernameRule>();
        }

        /// <summary>
        /// Saves the list of rules to the registry.
        /// </summary>
        public void Save()
        {
            //Clear existing rules
            m_settings.rules = new string[] { };

            //Create new string array of rules
            List<string> rules = new List<string>();
            foreach (IUsernameRule rule in list){
                string srule = rule.save();
                m_logger.DebugFormat("Saving rule {0} as \"{1}\"", rule.ToString(), srule);
                rules.Add(rule.save());
            }
            m_settings.rules = rules.ToArray();
        }

        /// <summary>
        /// Loads the rules from the register into list. 
        /// </summary>
        public void Load()
        {  
            string[] rules = (string[])m_settings.rules;
            m_logger.DebugFormat("Loaded rules from registry. {0} lines.", rules.Length);
            try
            {
                for (int k = 0; k < rules.Length; k++)
                {   
                    m_logger.DebugFormat("Rule read: {0}", rules[k]);
                    string[] srule = rules[k].Split('\n');
                    String stage = srule[0];
                    string action = srule[1];
                    string val1 = srule[2];
                    string val2 = null;
                    if (srule.Length > 3)
                        val2 = srule[3];
                    

                    IUsernameRule rule = CreateRule(stage, action, val1, val2);
                    m_logger.DebugFormat("Rule created: {0}", rule);
                    list.Add(rule);
                }
            }
            catch (Exception e)
            {
                throw new UsernameModPluginException("Unable to load rules from registry.", e);
            }
        }

        /// <summary>
        /// Adds a rule to the list, putting it at the end of the stage
        /// </summary>
        /// <param name="rule"></param>
        public void Add(IUsernameRule rule)
        {
            //Add the rule as the last item for the gateway
            for (int k = 0; k < list.Count; k++)
            {
                if (rule.stage.CompareTo(list[k].stage) < 0)
                {
                    list.Insert(k, rule);
                    return;
                }
            }
            list.Add(rule);
        }

        /// <summary>
        /// Moves the rule at the specified index up one (e.g. index=2 becomes index=1).
        /// 
        /// Will not move an item if it's at the top, or if it will be above a rule with an earlier step.
        /// (e.g. A gateway rule can not be above a authorization rule)
        /// </summary>
        /// <param name="index"></param>
        /// <returns>True if the move was successful</returns>
        public bool MoveUp(int index)
        {
            IUsernameRule rule = list.ElementAt(index);
            if(index > 0){
                if (list.ElementAt(index - 1).stage == rule.stage)
                {
                    list.RemoveAt(index);
                    list.Insert(index - 1, rule);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Moves the rule at the specified index down one (e.g. index=2 becomes index=3).
        /// 
        /// Will not move an item if it's at the bottom, or if it will be below a rule with a later step.
        /// (e.g. An authorization rule will not move below a gateway rule)
        /// </summary>
        /// <param name="index"></param>
        /// <returns>True if the move was successful</returns>
        public bool MoveDown(int index)
        {
            IUsernameRule rule = list.ElementAt(index);
            if (index < list.Count - 1)
            {
                if (list.ElementAt(index + 1).stage == rule.stage)
                {
                    list.RemoveAt(index);
                    list.Insert(index + 1, rule);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Removes the rule at the specified index.
        /// </summary>
        /// <param name="index"></param>
        public void remove(int index)
        {
            list.RemoveAt(index);
        }

        /// <summary>
        /// Creates a IUsernameRule based on the string representation of the stage, action, and values. 
        /// </summary>
        /// <param name="stage"></param>
        /// <param name="action"></param>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns></returns>
        public static IUsernameRule CreateRule(string stage, string action, string val1, string val2)
        {
            Stage estage;
            switch (stage)
            {
                case "Authorization":
                    estage = Stage.Authorization;
                    break;
                case "Authentication":
                    estage = Stage.Authentication;
                    break;
                case "Gateway":
                    estage = Stage.Gateway;
                    break;
                default:
                    throw new UsernameModPluginException("Invalid stage.");

            }
            return CreateRule(estage, action, val1, val2);
        }

        /// <summary>
        /// Creates an IUsernameRule based on the Stage value, and string representation of the rule
        /// </summary>
        /// <param name="stage"></param>
        /// <param name="action"></param>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns></returns>
        public static IUsernameRule CreateRule(Stage stage, string action, string val1, string val2)
        {   //Rules = { "Append", "Prepend", "Truncate", "Replace", "RegEx Replace", "Match" };
            switch (action)
            {
                case "Append":
                    return new AppendUsername(stage, val1);
                case "Prepend":
                    return new PrependUsername(stage, val1);
                case "Truncate":
                    return new TruncateUsername(stage, val1);
                case "Replace":
                    return new ReplaceUsername(stage, val1, val2);
                case "RegEx Replace":
                    return new RegexReplaceUsername(stage, val1, val2);
                case "Match":
                    return new MatchRegex(stage, val1);
                default:
                    throw new UsernameModPluginException(string.Format("Unable to generate rule from \"{0}/{1}/{2}/{3}\"", stage, action, val1, val2));

            }
        }
    }
    
}
