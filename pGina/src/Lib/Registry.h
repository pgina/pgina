#pragma once

#include <Windows.h>
#include <string>

namespace pGina
{
	namespace Registry 
	{
		// TBD: we should get settings from the service via named pipe, just use these for backup?
		std::wstring GetString(const wchar_t * keyName, const wchar_t * defaultValue);
		DWORD GetDword(const wchar_t * keyName, DWORD defaultValue);
		bool GetBool(const wchar_t * keyName, bool defaultValue);
	}
}