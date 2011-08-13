#include <windows.h>
#include <Unknwn.h>

// Handle to dll hinstance available for everyone globally
HINSTANCE g_dllHandle = NULL; 

// Internal refernce count for dll
static LONG internal_dllRefCount = 0;   

BOOL WINAPI DllMain(__in HINSTANCE hDll, __in DWORD dwReason, __in void * reserved)
{
    switch (dwReason)
    {
    case DLL_PROCESS_ATTACH:
        DisableThreadLibraryCalls(hDll);
        break;
    case DLL_PROCESS_DETACH:
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
        break;
    }
    
    g_dllHandle = hDll;
    return TRUE;
}

void AddDllReference()
{
	InterlockedIncrement(&internal_dllRefCount);
}

void ReleaseDllReference()
{
	InterlockedDecrement(&internal_dllRefCount);
}
