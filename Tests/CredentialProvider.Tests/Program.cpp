#include "../../OpenCredential/src/CredentialProvider/SerializationHelpers.h"
#include <wincred.h>
#include <vector>
#include <cstdio>

static void FreePassword(PWSTR password)
{
    if (password) { SecureZeroMemory(password, (wcslen(password) + 1) * sizeof(WCHAR)); LocalFree(password); }
}

static BOOL WINAPI CannotUnprotect(BOOL, LPWSTR, DWORD, LPWSTR, LPDWORD)
{
    SetLastError(ERROR_NOT_CAPABLE);
    return FALSE;
}

static BOOL WINAPI FailsAfterSizing(BOOL, LPWSTR, DWORD, LPWSTR output, LPDWORD chars)
{
    if (!output) { *chars = 64; SetLastError(ERROR_INSUFFICIENT_BUFFER); }
    else { output[0] = L'x'; SetLastError(ERROR_NOT_CAPABLE); }
    return FALSE;
}

int wmain()
{
    // Synthetic text only: never access registry, install/register a provider,
    // contact a service/database or print either clear or protected credentials.
    const WCHAR original[] = L" Synthetic-only RDP regression 123! \u00E1\u4E2D ";
    PWSTR copy = NULL;
    if (FAILED(Microsoft::Sample::UnprotectAndCopyPassword(original, &copy)) || !copy || wcscmp(copy, original)) { puts("FAIL Plain credential copy"); FreePassword(copy); return 1; }
    FreePassword(copy);
    copy = NULL;
    if (FAILED(Microsoft::Sample::UnprotectAndCopyPassword(L"", &copy)) || !copy || *copy) { puts("FAIL Empty credential"); FreePassword(copy); return 1; }
    FreePassword(copy);
    copy = reinterpret_cast<PWSTR>(1);
    if (FAILED(Microsoft::Sample::UnprotectAndCopyPassword(NULL, &copy)) || copy != NULL) { puts("FAIL Null credential"); return 1; }
    if (Microsoft::Sample::UnprotectAndCopyPassword(original, NULL) != E_POINTER) { puts("FAIL Null output pointer"); return 1; }

    std::vector<WCHAR> input(original, original + ARRAYSIZE(original));
    DWORD protectedChars = 0;
    CRED_PROTECTION_TYPE type = CredUnprotected;
    if (CredProtectW(FALSE, input.data(), static_cast<DWORD>(input.size()), NULL, &protectedChars, &type) || GetLastError() != ERROR_INSUFFICIENT_BUFFER) { puts("FAIL CredProtect sizing"); return 1; }
    std::vector<WCHAR> protectedText(protectedChars);
    if (!CredProtectW(FALSE, input.data(), static_cast<DWORD>(input.size()), protectedText.data(), &protectedChars, &type)) { puts("FAIL Synthetic credential protection"); return 1; }
    copy = NULL;
    HRESULT result = Microsoft::Sample::UnprotectAndCopyPassword(protectedText.data(), &copy);
    bool matches = SUCCEEDED(result) && copy && wcscmp(copy, original) == 0;
    FreePassword(copy);
    if (!matches) { puts("FAIL Protected RDP credential must reach plugins as original plaintext"); return 1; }
    // Simulate a protection-context mismatch on a genuine protected payload.
    // Never send that payload to plugins when Windows cannot unprotect it.
    copy = NULL;
    result = Microsoft::Sample::UnprotectAndCopyPassword(protectedText.data(), &copy, CannotUnprotect);
    bool rejected = FAILED(result) && copy == NULL;
    FreePassword(copy);
    copy = NULL;
    result = Microsoft::Sample::UnprotectAndCopyPassword(protectedText.data(), &copy, FailsAfterSizing);
    rejected = rejected && FAILED(result) && copy == NULL;
    FreePassword(copy);
    SecureZeroMemory(input.data(), input.size() * sizeof(WCHAR));
    SecureZeroMemory(protectedText.data(), protectedText.size() * sizeof(WCHAR));
    if (!rejected) { puts("FAIL Undecryptable protected credential must fail closed"); return 1; }
    puts("PASS Credential serialization: plain, Unicode, empty, null, CredProtect round-trip and unprotect failure; no real authentication performed.");
    return 0;
}
