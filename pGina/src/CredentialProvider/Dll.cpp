#include <windows.h>
#include <Unknwn.h>

#include "ProviderGuid.h"
#include "ClassFactory.h"

// Handle to dll hinstance available for everyone globally
HINSTANCE g_dllHandle = NULL; 

// Internal refernce count for dll
static long internal_dllRefCount = 0;   

// Dll loaded entry point
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

HRESULT WINAPI DllCanUnloadNow()
{
    return (internal_dllRefCount > 0) ? S_FALSE : S_OK;
}

// COM entry point
HRESULT WINAPI DllGetClassObject(__in REFCLSID rclsid, __in REFIID riid, __deref_out void** ppv)
{
    HRESULT hr = CLASS_E_CLASSNOTAVAILABLE;
	*ppv = NULL;

	// We provide class factory support for our provider only
    if (rclsid == CLSID_CpGinaProvider)
    {
		pGina::COM::CClassFactory * factory = new pGina::COM::CClassFactory();        
        if (factory)
        {
            hr = factory->QueryInterface(riid, ppv);
            factory->Release();
        }
        else
        {
            hr = E_OUTOFMEMORY;
        }
    }
    
    return hr;
}

