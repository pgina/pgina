#pragma once

#include <Windows.h>
#include <string>

namespace pGina
{
	namespace Registry 
	{
		std::wstring GetString(const wchar_t * keyName, const wchar_t * defaultValue);
	}
}