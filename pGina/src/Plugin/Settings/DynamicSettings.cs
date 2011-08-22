using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using Microsoft.Win32;

namespace pGina.Shared.Settings
{
    public class DynamicSettings : DynamicObject
    {
        public static readonly string PGINA_KEY = @"SOFTWARE\pGina3";
        private string m_rootKey = PGINA_KEY;
        
        public DynamicSettings()
        {
        }

        public DynamicSettings(Guid pluginGuid)
        {
            m_rootKey = string.Format("{0}\\Plugins\\{1}", m_rootKey, pluginGuid.ToString());
        }

        public DynamicSettings(string root)
        {
            m_rootKey = root;
        }

        /// <summary>
        /// Sets the default value for a setting.  Checks to see if the setting
        /// is already defined in the registry.  If so, the method does nothing.
        /// Otherwise the setting is initialized to value.
        /// </summary>
        /// <param name="name">The name of the setting</param>
        /// <param name="value">The default value for the setting</param>
        public void SetDefault(string name, object value)
        {
            try
            {
                GetSetting(name);
            }
            catch (KeyNotFoundException)
            {
                SetSetting(name, value);
            }
        }

        public void SetSetting(string name, object value)
        {
            using (RegistryKey key = Registry.LocalMachine.CreateSubKey(m_rootKey))
            {
                key.SetValue(name, value);
            }            
        }
        
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            SetSetting(binder.Name, value);            
            return true;
        }

        public DynamicSetting GetSetting(string name)
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(m_rootKey))
            {
                if (key != null && key.GetValueNames().Contains(name))
                {
                    object value = key.GetValue(name);
                    return new DynamicSetting(name, value);                    
                }
                else
                {
                    throw new KeyNotFoundException(string.Format("Unable to find value for: {0}", name));                    
                }
            }
        }

        public DynamicSetting GetSetting(string name, object def)
        {
            try
            {
                return GetSetting(name);
            }
            catch (KeyNotFoundException)
            {
                return new DynamicSetting(name, def);
            }
        }
        
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {            
            result = GetSetting(binder.Name);
            return true;            
        }       
    }    
}
