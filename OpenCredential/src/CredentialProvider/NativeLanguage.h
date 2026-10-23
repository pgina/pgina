#pragma once
#include "NativeText.h"

namespace OpenCredential { namespace Localization {
    inline const wchar_t* Text(const wchar_t* english)
    {
        // Machine policy only. No HKCU, external XML, assemblies or network access from LogonUI.
        wchar_t selected[16] = L"auto";
        HKEY key = NULL;
        // Both provider architectures read the native machine view. Use a bounded REG_SZ buffer.
        if (RegOpenKeyExW(HKEY_LOCAL_MACHINE, L"SOFTWARE\\OpenCredential3", 0,
            KEY_QUERY_VALUE | KEY_WOW64_64KEY, &key) == ERROR_SUCCESS)
        {
            wchar_t value[16] = {};
            DWORD bytes = sizeof(value), type = 0;
            if (RegQueryValueExW(key, L"SignInLanguage", NULL, &type, reinterpret_cast<BYTE*>(value), &bytes) == ERROR_SUCCESS && type == REG_SZ)
            {
                value[15] = L'\0';
                wcscpy_s(selected, value);
            }
            RegCloseKey(key);
        }
        return Text(english, IsSpanish(selected, GetSystemDefaultUILanguage()));
    }
} }
