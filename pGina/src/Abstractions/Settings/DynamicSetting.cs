/*
	Copyright (c) 2011, pGina Team
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
using System.Dynamic;
using System.Reflection;
using System.ComponentModel;
using Microsoft.Win32;

namespace Abstractions.Settings
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
