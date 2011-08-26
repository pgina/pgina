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
#include "Registry.h"
#include <WinReg.h>

namespace pGina
{
	namespace Registry
	{
		std::wstring GetString(const wchar_t * keyName, const wchar_t * defaultValue)
		{
			std::wstring result = (defaultValue ? defaultValue : L"");	// Do not assign NULL!
			HKEY hKey = NULL;

			if(RegOpenKeyEx(HKEY_LOCAL_MACHINE, L"Software\\pGina3", 0, KEY_READ, &hKey) == ERROR_SUCCESS)
			{
				DWORD dataLength = 0;
				if(RegGetValue(hKey, NULL, keyName, RRF_RT_REG_SZ, NULL, NULL, &dataLength) == ERROR_SUCCESS)
				{
					wchar_t * buffer = (wchar_t *) malloc(dataLength);
					memset(buffer, 0, dataLength);

					if(RegGetValue(hKey, NULL, keyName, RRF_RT_REG_SZ, NULL, buffer, &dataLength) == ERROR_SUCCESS)
					{
						result = buffer;
					}

					free(buffer);
				}

				RegCloseKey(hKey);
			}

			return result;
		}	

		DWORD GetDword(const wchar_t * keyName, DWORD defaultValue)
		{
			DWORD result = defaultValue;
			HKEY hKey = NULL;

			if(RegOpenKeyEx(HKEY_LOCAL_MACHINE, L"Software\\pGina3", 0, KEY_READ, &hKey) == ERROR_SUCCESS)
			{
				DWORD dataLength = 0;				
				RegGetValue(hKey, NULL, keyName, RRF_RT_REG_DWORD, NULL, &result, &dataLength);				
				RegCloseKey(hKey);
			}

			return result;
		}

		bool GetBool(const wchar_t * keyName, bool defaultValue)
		{
			return (GetDword(keyName, defaultValue ? (DWORD) 0x01 : (DWORD) 0x00) != (DWORD) 0x00);
		}
	}
}