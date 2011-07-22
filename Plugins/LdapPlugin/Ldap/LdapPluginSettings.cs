using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Security.Permissions;

using Microsoft.Win32;

namespace pGina.Plugin.Ldap
{
    /// <summary>
    /// This class contains each setting as a property.  The values for the properties are stored in a 
    /// dictionary that contains information about each property including the default value.
    /// </summary>
    public class LdapPluginSettings
    {
        /// <summary>
        /// The registry key where the settings for this plugin are stored.
        /// </summary>
        public static readonly string PGINA_LDAP_PLUGIN_SUB_KEY = @"SOFTWARE\pGina3\Plugins\LdapPlugin";

        /// <summary>
        /// This map stores one key value pair for each of the properties in this
        /// class.  The actual values for each property are kept in this map including
        /// other information such as type and default value.
        /// </summary>
        private Dictionary<string, LdapPluginConfigElement> configMap;
 
        /// <summary>
        /// Indexer for accessing each configuration element by name.
        /// </summary>
        /// <param name="key">The name of the configuration property.</param>
        /// <returns>The value of the configuration property (as Object).</returns>
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

        /////////////////////////////////////////////////////////////////
        ///////////////////  Settings ///////////////////////////////////
        /////////////////////////////////////////////////////////////////

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

        /// <summary>
        /// Whether or not to do a search in order to find the DN for the user.
        /// </summary>
        public bool DoSearch
        {
            get { return (bool)this["DoSearch"]; }
            set { this["DoSearch"] = value; }
        }

        /// <summary>
        /// The contexts to search (if DoSearch is true).
        /// </summary>
        public String[] SearchContexts
        {
            get { return (string[])this["SearchContexts"]; }
            set { this["SearchContexts"] = value; }
        }

        /// <summary>
        /// The search filter to use (if DoSearch is true).
        /// </summary>
        public string SearchFilter
        {
            get { return (string)this["SearchFilter"]; }
            set { this["SearchFilter"] = value; }
        }

        /// <summary>
        /// The DN that is used for binding to the LDAP server when performing searches
        /// (when DoSearch is true).
        /// </summary>
        public string SearchDN
        {
            get { return (string)this["SearchDN"]; }
            set { this["SearchDN"] = value; }
        }

        /// <summary>
        /// The password used when binding to the LDAP server for performing searches
        /// (when DoSearch is true).
        /// </summary>
        public string SearchPW
        {
            get { return (string)this["SearchPW"]; }
            set { this["SearchPW"] = value; }
        }

        /// <summary>
        /// The pattern that is used to create the DN for use in authenticating 
        /// the login (used when DoSearch is false).
        /// </summary>
        public string DnPattern
        {
            get { return (string)this["DnPattern"]; }
            set { this["DnPattern"] = value; }
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
        /// Private constructor, use Load method to get settings from registry.  This sets each
        /// property to its default value.
        /// </summary>
        private LdapPluginSettings() 
        {
            this.configMap = new Dictionary<string, LdapPluginConfigElement>();

            // Set default values

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

        /// <summary>
        /// Saves these settings to the registry.
        /// </summary>
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

    /// <summary>
    /// Instances of this class store name, type, and default value for each of the settings
    /// in LdapPluginSettings.
    /// </summary>
    class LdapPluginConfigProperty
    {
        /// <summary>
        /// The name of the setting (same as the property name in LdapPluginSettings)
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The data type for the setting
        /// </summary>
        public Type Type { get; set; }
        /// <summary>
        /// The default value for the setting
        /// </summary>
        public Object DefaultValue { get; set; }

        public LdapPluginConfigProperty(string name, Type type, Object defaultValue)
        {
            Name = name;
            Type = type;
            DefaultValue = defaultValue;
        }
    }

    /// <summary>
    /// Instances of this are used as the values in the configuration map.  Stores
    /// the LdapPluginConfigProperty object for each setting as well as the actual
    /// value.
    /// </summary>
    class LdapPluginConfigElement
    {
        /// <summary>
        /// The LdapPluginConfigProperty object that describes this setting.
        /// </summary>
        public LdapPluginConfigProperty ElementProperty { get; set; }
        /// <summary>
        /// The value of this setting.
        /// </summary>
        public Object Value { get; set; }

        public LdapPluginConfigElement(LdapPluginConfigProperty prop)
        {
            ElementProperty = prop;
            Value = prop.DefaultValue;
        }
    }
}
