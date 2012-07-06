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
using Microsoft.Win32;

namespace pGina.Shared.Settings
{
    public class pGinaDynamicSettings : Abstractions.Settings.DynamicSettings
    {    
        public const string pGinaRoot = @"SOFTWARE\pGina3";
        public pGinaDynamicSettings() :
            base(pGinaRoot)
        {
        }

        public pGinaDynamicSettings(Guid pluginGuid) :
            base(string.Format(@"{0}\Plugins\{1}", pGinaRoot, pluginGuid.ToString()))
        {            
        }

        public pGinaDynamicSettings(Guid pluginGuid, string subKey) :
            base(string.Format(@"{0}\Plugins\{1}\{2}", pGinaRoot, pluginGuid.ToString(), subKey))
        {
        }

        /// <summary>
        /// Get a dictionary containing all sub-keys of the plugin's registry
        /// key.  The Dictionary key is the sub-key name, the value is a pGinaDynamicSettings object
        /// corresponding to the sub-key.
        /// </summary>
        /// <param name="pluginGuid">The plugin Guid.</param>
        /// <returns></returns>
        public static Dictionary<string, dynamic> GetSubSettings(Guid pluginGuid)
        {
            Dictionary<string, dynamic> result = new Dictionary<string, dynamic>();

            string subKey = string.Format(@"{0}\Plugins\{1}", pGinaRoot, pluginGuid.ToString());
            using( RegistryKey key = Registry.LocalMachine.OpenSubKey(subKey, false) )
            {
                if (key != null)
                {
                    string[] names = key.GetSubKeyNames();
                    foreach (string n in names)
                    {
                        result.Add(n, new pGinaDynamicSettings(pluginGuid, n));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Remove all sub-keys that are NOT in the provided list.
        /// </summary>
        /// <param name="pluginGuid">The plugin's Guid.</param>
        /// <param name="toKeep">The list of sub-keys to keep, all others are deleted.</param>
        public static void CleanSubSettings(Guid pluginGuid, List<string> toKeep)
        {
            string subKey = string.Format(@"{0}\Plugins\{1}", pGinaRoot, pluginGuid.ToString());
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(subKey, true))
            {
                if (key != null)
                {
                    string[] names = key.GetSubKeyNames();
                    foreach (string n in names)
                    {
                        if (! toKeep.Contains(n))
                        {
                            key.DeleteSubKey(n, false);
                        }
                    }
                }
            }

        }
    }
}
