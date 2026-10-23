#pragma once
#include <windows.h>
#include <string>
#include <cwchar>

namespace OpenCredential { namespace Localization {
    inline bool IsSpanish(const std::wstring& selected, LANGID systemLanguage)
    {
        if (selected == L"es" || selected.compare(0, 3, L"es-") == 0) return true;
        if (selected == L"auto" || selected.empty()) return PRIMARYLANGID(systemLanguage) == LANG_SPANISH;
        return false;
    }

    // Presentation-only literals. Never pass usernames, passwords, protocol keys or plugin payloads here.
    inline const wchar_t* Text(const wchar_t* english, bool spanish)
    {
        if (!english) return L"";
        if (!spanish) return english;
        static const struct { const wchar_t* en; const wchar_t* es; } entries[] = {
            { L"Username", L"Usuario" }, { L"Password", L"Contrase\u00f1a" },
            { L"Old Password", L"Contrase\u00f1a actual" }, { L"New Password", L"Nueva contrase\u00f1a" },
            { L"Confirm New Password", L"Confirmar nueva contrase\u00f1a" }, { L"Submit", L"Entrar" },
            { L"Status", L"Estado" }, { L"Locked", L"Bloqueado" },
            { L"Logon cancelled", L"Inicio de sesi\u00f3n cancelado" },
            { L"Plugins did not provide a specific error message", L"Los complementos no proporcionaron un mensaje de error espec\u00edfico" },
            { L"OpenCredential: Your password was successfully changed", L"OpenCredential: contrase\u00f1a cambiada correctamente" },
            { L"Logging on...", L"Iniciando sesi\u00f3n..." }, { L"Logging on %u...", L"Iniciando sesi\u00f3n de %u..." },
            { L"Logon successful", L"Inicio de sesi\u00f3n correcto" }, { L"Logon failed", L"Inicio de sesi\u00f3n fallido" },
            { L"New passwords do not match", L"Las nuevas contrase\u00f1as no coinciden" },
            { L"Password was successfully changed", L"La contrase\u00f1a se cambi\u00f3 correctamente" },
            { L"Failed to change password, no message from plugins.", L"No se pudo cambiar la contrase\u00f1a; los complementos no proporcionaron detalles." },
            { L"OpenCredential service connected.", L"Servicio OpenCredential conectado." },
            { L"OpenCredential service is starting or temporarily unavailable. You can continue signing in, and offline sign-in will work when cached credentials are available.",
              L"El servicio OpenCredential se est\u00e1 iniciando o no est\u00e1 disponible temporalmente. Puede continuar; el acceso sin conexi\u00f3n funcionar\u00e1 si existen credenciales v\u00e1lidas en cach\u00e9." }
        };
        for (const auto& entry : entries) if (wcscmp(english, entry.en) == 0) return entry.es;
        return english;
    }
} }
