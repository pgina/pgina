using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Security.Permissions;

using Microsoft.Win32;

namespace pGina.Plugin.Ldap
{
    class LdapPluginConfigProperty
    {
        public string Name { get; set; }
        public Type Type { get; set; }
        public Object DefaultValue { get; set; }

        public LdapPluginConfigProperty(string name, Type type, Object defaultValue)
        {
            Name = name;
            Type = type;
            DefaultValue = defaultValue;
        }
    }

    class LdapPluginConfigElement
    {
        public LdapPluginConfigProperty ElementProperty { get; set; }
        public Object Value { get; set; }

        public LdapPluginConfigElement(LdapPluginConfigProperty prop)
        {
            ElementProperty = prop;
            Value = prop.DefaultValue;
        }
    }

    public class LdapPluginSettings
    {
        public static readonly string PGINA_LDAP_PLUGIN_SUB_KEY = @"SOFTWARE\pGina3\Plugins\LdapPlugin";

        private Dictionary<string, LdapPluginConfigElement> configMap;
 
        public Object this[string key]
        {
            get
            {
                return configMap[key].Value;
            }

            set
            {
                if (configMap.ContainsKey(key))
                {
                    configMap[key].Value = value;
                }
            }
        }

        /// <summary>
        /// LDAP server host name(s).
        /// </summary>
        public string[] LdapHost 
        { 
            get { return (string[])this["LdapHost"]; }
            set { this["LdapHost"] = value; } 
        }

        /// <summary>
        /// LDAP server port number.
        /// </summary>
        public int LdapPort 
        {
            get { return (int)this["LdapPort"]; }
            set { this["LdapPort"] = value; }
        }

        /// <summary>
        /// Whether or not to use SSL when connecting to the LDAP server.
        /// </summary>
        public bool UseSsl
        {
            get { return (bool)this["UseSsl"]; }
            set { this["UseSsl"] = value; }
        }

        /// <summary>
        /// The file name containing the server sertificate used for validation.
        /// </summary>
        public string ServerCertFile
        {
            get { return (string)this["ServerCertFile"]; }
            set { this["ServerCertFile"] = value; }
        }

        public bool DoSearch
        {
            get { return (bool)this["DoSearch"]; }
            set { this["DoSearch"] = value; }
        }

        public String[] SearchContexts
        {
            get { return (string[])this["SearchContexts"]; }
            set { this["SearchContexts"] = value; }
        }

        public string SearchFilter
        {
            get { return (string)this["SearchFilter"]; }
            set { this["SearchFilter"] = value; }
        }

        /// <summary>
        /// The pattern that is used to create the DN for use in authenticating 
        /// the login.
        /// </summary>
        public string DnPattern
        {
            get { return (string)this["DnPattern"]; }
            set { this["DnPattern"] = value; }
        }

        /// <summary>
        /// The DN that is used for binding to the LDAP server when performing searches.
        /// </summary>
        public string SearchDN
        {
            get { return (string)this["SearchDN"]; }
            set { this["SearchDN"] = value; }
        }

        /// <summary>
        /// The password used when binding to the LDAP server for performing searches.
        /// </summary>
        public string SearchPW
        {
            get { return (string)this["SearchPW"]; }
            set { this["SearchPW"] = value; }
        }

        /// <summary>
        /// Whether or not to do authorization based on group membership.
        /// </summary>
        public bool DoGroupAuthorization
        {
            get { return (bool)this["DoGroupAuthorization"]; }
            set { this["DoGroupAuthorization"] = value; }
        }

        /// <summary>
        /// The list of groups that are allowed to login.  The authenticated user must
        /// be a member of at least one of these groups in order to be granted 
        /// authorization when DoGroupAuthorization is true.
        /// </summary>
        public string[] LdapLoginGroups
        {
            get { return (string[])this["LdapLoginGroups"]; }
            set { this["LdapLoginGroups"] = value; }
        }

        /// <summary>
        /// The list of groups of which the user must be a member to be given administrative privs.
        /// </summary>
        public string LdapAdminGroup
        {
            get { return (string)this["LdapAdminGroup"]; }
            set { this["LdapAdminGroup"] = value; }
        }

        /// <summary>
        /// Private constructor, use Load method to get settings from registry.
        /// </summary>
        private LdapPluginSettings() 
        {
            this.configMap = new Dictionary<string, LdapPluginConfigElement>();

            // So far, we only support the following types:
            //   string[], string, int, bool
            //
            AddProperty(new LdapPluginConfigProperty("LdapHost", typeof(string[]), new string[] { "ldap.example.com" }));
            AddProperty(new LdapPluginConfigProperty("LdapPort", typeof(int), 389));
            AddProperty(new LdapPluginConfigProperty("UseSsl", typeof(bool), false));
            AddProperty(new LdapPluginConfigProperty("ServerCertFile", typeof(string), ""));
            AddProperty(new LdapPluginConfigProperty("DoSearch", typeof(bool), false));
            AddProperty(new LdapPluginConfigProperty("SearchContexts", typeof(string[]), new string[] {} ));
            AddProperty(new LdapPluginConfigProperty("SearchFilter", typeof(string), ""));
            AddProperty(new LdapPluginConfigProperty("DnPattern", typeof(string), "uid=%u,dc=example,dc=com"));
            AddProperty(new LdapPluginConfigProperty("SearchDN", typeof(string), ""));
            AddProperty(new LdapPluginConfigProperty("SearchPW", typeof(string), ""));
            AddProperty(new LdapPluginConfigProperty("DoGroupAuthorization", typeof(bool), false));
            AddProperty(new LdapPluginConfigProperty("LdapLoginGroups", typeof(string[]), new string[] {}));
            AddProperty(new LdapPluginConfigProperty("LdapAdminGroup", typeof(string), "wheel"));
        }

        private void AddProperty(LdapPluginConfigProperty prop)
        {
            configMap[prop.Name] = new LdapPluginConfigElement(prop);
        }

        /// <summary>
        /// Attempts to load the settings from the registry and return them as a new instance
        /// of LdapPluginSettings.
        /// </summary>
        /// <returns>A new instance of LdapPluginSettings containing the settings loaded from
        /// the registry.  If the registry entry does not exist, default values are returned.</returns>
        public static LdapPluginSettings Load()
        {
            LdapPluginSettings settings = new LdapPluginSettings();

            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(PGINA_LDAP_PLUGIN_SUB_KEY) )
            {
                if (key != null)
                {
                    foreach (KeyValuePair<string, LdapPluginConfigElement> entry in settings.configMap)
                    {
                        object o = key.GetValue(entry.Key, entry.Value.ElementProperty.DefaultValue);
                        Type t = entry.Value.ElementProperty.Type;

                        // We expect only the following types:
                        //   string[], bool, string, int
                        // More may be added later...
                        if (t.IsArray)
                        {
                            entry.Value.Value = (string[])o;
                        }
                        else
                        {
                            switch (Type.GetTypeCode(t))
                            {
                                case TypeCode.String:
                                    entry.Value.Value = Convert.ToString(o);
                                    break;
                                case TypeCode.Boolean:
                                    entry.Value.Value = Convert.ToBoolean(o);
                                    break;
                                case TypeCode.Int32:
                                    entry.Value.Value = Convert.ToInt32(o);
                                    break;
                                default:
                                    throw new Exception("Unrecognized data type found in LdapPluginSettings configMap!");

                            }
                        }
                    }
                }
            }
            
            return settings;
        }

        public void Save()
        {
            using ( RegistryKey key = Registry.LocalMachine.CreateSubKey(PGINA_LDAP_PLUGIN_SUB_KEY) )
            {          
                foreach (KeyValuePair<string, LdapPluginConfigElement> entry in configMap)
                {
                    key.SetValue(entry.Key, entry.Value.Value);
                }
            }
        }
    }
}
