#include "ClassFactory.h"
#include "Dll.h"

#pragma warning(push)
#pragma warning(disable : 4995)
#include <shlwapi.h>
#pragma warning(pop)

namespace pGina
{
	namespace COM
	{
		CClassFactory::CClassFactory() : m_referenceCount(1)
		{
		}
				
		// IUnknown
		IFACEMETHODIMP CClassFactory::QueryInterface(__in REFIID riid, __deref_out void **ppv)
		{
			// TBD: This is magic I do not understand, but it works, based on the CredentialProvider samples,
			//	stupid COM :/
			static const QITAB qit[] = 
			{
	            QITABENT(CClassFactory, IClassFactory),
				{ 0 },
			};
			return QISearch(this, qit, riid, ppv);
		}

		IFACEMETHODIMP_(ULONG) CClassFactory::AddRef()
		{
	        return InterlockedIncrement(&m_referenceCount);
		}

		IFACEMETHODIMP_(ULONG) CClassFactory::Release()
		{
			LONG cRef = InterlockedDecrement(&m_referenceCount);
			if (!cRef)
				delete this;
			return cRef;
		}

		// IClassFactory
		IFACEMETHODIMP CClassFactory::CreateInstance(__in IUnknown* pUnkOuter, __in REFIID riid, __deref_out void **ppv)
		{
			HRESULT hr = CLASS_E_NOAGGREGATION;
			*ppv = NULL;

			if (!pUnkOuter)
			{
				// TBD: hr = CSample_CreateInstance(riid, ppv);
			}
			
			return hr;
		}

		IFACEMETHODIMP CClassFactory::LockServer(__in BOOL bLock)
		{
			if (bLock)
			{
				AddDllReference();
			}
			else
			{
				ReleaseDllReference();
			}
			return S_OK;
		}

		CClassFactory::~CClassFactory()
		{
		}    
	}
}