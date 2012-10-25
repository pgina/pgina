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

using Microsoft.Win32;

namespace pGina.Configuration
{
    class CredProvFilterConfig
    {
        private static readonly string CRED_PROV_KEY = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Authentication\Credential Providers";

        public static List<CredProv> LoadCredProvsAndFilterSettings()
        {
            Dictionary<Guid, CredProv> result = new Dictionary<Guid, CredProv>();

            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(CRED_PROV_KEY))
            {
                if (key != null)
                {
                    string[] subKeys = key.GetSubKeyNames();
                    foreach (string sub in subKeys)
                    {
                        using (RegistryKey cpKey = key.OpenSubKey(sub))
                        {
                            if (cpKey != null)
                            {
                                CredProv credProv = new CredProv
                                {
                                    Uuid = new Guid(sub)
                                };

                                object name = cpKey.GetValue("");

                                if (name != null)
                                {
                                    credProv.Name = name.ToString();
                                }
                                else
                                {
                                    credProv.Name = credProv.Uuid.ToString();
                                }

                                if (!result.ContainsKey(credProv.Uuid))
                                {
                                    result.Add(credProv.Uuid, credProv);
                                }
                            }
                        }
                    }
                }
            }

            // Load filter settings from registry
            string[] filterSettings = pGina.Core.Settings.Get.CredentialProviderFilters;
            List<CredProv> filterSettingsList = new List<CredProv>();
            foreach (string s in filterSettings)
                filterSettingsList.Add(CredProv.FromRegString(s));

            // Merge registry settings into the dictionary
            foreach (CredProv cp in filterSettingsList)
            {
                if (result.ContainsKey(cp.Uuid))
                {
                    cp.Name = result[cp.Uuid].Name;
                    result[cp.Uuid] = cp;
                }
            }

            return result.Values.ToList();
        }

        public static void SaveFilterSettings(List<CredProv> list)
        {
            List<string> filters = new List<string>();
            foreach (CredProv cp in list)
            {
                if (cp.FilterEnabled())
                {
                    filters.Add(cp.ToRegString());
                }
            }

            string[] result = filters.ToArray();
            pGina.Core.Settings.Get.CredentialProviderFilters = result;
        }
    }


    class CredProv
    {
        public string Name { get; set; }
        public Guid Uuid { get; set; }

        public bool FilterLogon { get; set; }
        public bool FilterUnlock { get; set; }
        public bool FilterChangePass { get; set; }
        public bool FilterCredUI { get; set; }

        public bool FilterEnabled()
        {
            return FilterLogon || FilterUnlock || FilterChangePass || FilterCredUI;
        }

        public string ToRegString()
        {
            int filter = 0;
            
            if (FilterLogon) filter |= 0x1;
            if (FilterUnlock) filter |= 0x2;
            if (FilterChangePass) filter |= 0x4;
            if (FilterCredUI) filter |= 0x8;

            return string.Format("{{{0}}}\t{1}", Uuid.ToString(), filter);
        }

        public static CredProv FromRegString(string str)
        {
            string[] parts = str.Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
            CredProv result = new CredProv
            {
                Uuid = new Guid(parts[0])
            };
            int filter = Convert.ToInt32(parts[1]);
            if ((filter & 0x1) != 0) result.FilterLogon = true;
            if ((filter & 0x2) != 0) result.FilterUnlock = true;
            if ((filter & 0x4) != 0) result.FilterChangePass = true;
            if ((filter & 0x8) != 0) result.FilterCredUI = true;
            return result;
        }
    }
}
