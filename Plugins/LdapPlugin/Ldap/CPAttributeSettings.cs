/*
	Copyright (c) 2013, pGina Team
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
using System.Security.Cryptography;

namespace pGina.Plugin.Ldap
{
    class PasswordAttributeEntry
    {
        public string Name { get; set; }
        public HashMethod Method { get; set; }

        public string ToRegistryString()
        {
            return Name + "\n" + (int)Method;
        }

        public void FromRegistryString(string str)
        {
            string[] parts = Regex.Split(str, @"\n");
            if (parts.Length == 2)
            {
                Name = parts[0];
                Method = (HashMethod)Enum.Parse(typeof(HashMethod), parts[1]);
            }
        }
    }

    class CPAttributeSettings
    {
        public static List<PasswordAttributeEntry> Load()
        {
            string[] values = Settings.Store.ChangePasswordAttributes;
            List<PasswordAttributeEntry> result = new List<PasswordAttributeEntry>();
            foreach (string entry in values)
            {
                PasswordAttributeEntry pae = new PasswordAttributeEntry();
                pae.FromRegistryString(entry);
                result.Add(pae);
            }
            return result;
        }

        public static void Save(List<PasswordAttributeEntry> values)
        {
            List<string> regList = new List<string>();
            foreach (PasswordAttributeEntry entry in values)
            {
                regList.Add(entry.ToRegistryString());
            }
            Settings.Store.ChangePasswordAttributes = regList.ToArray();
        }
    }
}
