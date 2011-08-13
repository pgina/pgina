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