/*
	Copyright (c) 2012, pGina Team
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
#include <windows.h>
#include <Unknwn.h>

#include "ProviderGuid.h"
#include "ClassFactory.h"

// Handle to dll hinstance available for everyone globally via GetMy*()
static HINSTANCE g_dllHandle = NULL; 

// Internal refernce count for dll
static long internal_dllRefCount = 0;   

HINSTANCE GetMyInstance()
{
	return g_dllHandle;
}

HMODULE GetMyModule() 
{
	return (HMODULE) g_dllHandle;
}

// Dll loaded entry point
BOOL WINAPI DllMain(__in HINSTANCE hDll, __in DWORD dwReason, __in void * reserved)
{
    switch (dwReason)
    {
    case DLL_PROCESS_ATTACH:
        DisableThreadLibraryCalls(hDll);
        break;
    case DLL_PROCESS_DETACH:    
        break;
	// No thread attach/detach will be signaled, as we called DisableThreadLibraryCalls, 
	// cases included here for completeness in enum values only!
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

