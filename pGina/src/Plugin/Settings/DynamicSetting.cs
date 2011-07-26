using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Reflection;
using System.ComponentModel;
using Microsoft.Win32;

namespace pGina.Shared.Settings
{
    public class DynamicSetting : DynamicObject
    {
        private object m_value = null;
        private string m_name = null;        

        public DynamicSetting(string name, object value)
        {
            m_name = name;
            m_value = value;
        }

        public object RawValue
        {
            get { return m_value; }
            set { m_value = value; }
        }

        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var descr = TypeDescriptor.GetProperties(m_value);
            if (descr[binder.Name] != null)
            {
                result = descr[binder.Name].GetValue(m_value);
                return true;
            }

            result = null;
            return false;
        }

        public override string ToString()
        {
            if (m_value != null)
            {
                return m_value.ToString();
            }

            return base.ToString();
        }
        
        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            result = null;

            if (m_value == null)
                return true;

            // 1:1 conversion 
            Type ourType = m_value.GetType();
            if (binder.Type == ourType)
            {
                result = m_value;
                return true;
            }

            // If caller wants a bool we have to do a little 
            //  conversion, as there is no native Reg bool type,
            //  instead it saves them as strings.
            if (binder.Type == typeof(bool))
            {
                if(ourType == typeof(string))
                {
                    result = bool.Parse((string)m_value);
                    return true;
                }

                if (ourType == typeof(Int32))
                {
                    result = (((int)m_value) != 0);
                    return true;
                }
            }
            
            // We could potentially offer some standard conversions here? For now,
            // we just fail.
            return false;
        }
        
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            Type ourType = m_value.GetType();
            try
            {
                result = ourType.InvokeMember(
                          binder.Name,
                          BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance,
                          null, m_value, args);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }
    }
}
