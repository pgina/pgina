#include <Macros.h>

#include "CredentialProviderFilter.h"

#include "Dll.h"

#pragma warning(push)
#pragma warning(disable : 4995)
#include <shlwapi.h>
#pragma warning(pop)

#include <wincred.h>

namespace pGina {
	namespace CredProv {

		IFACEMETHODIMP CredentialProviderFilter::QueryInterface(__in REFIID riid, __deref_out void **ppv)
		{
			static const QITAB qit[] =
			{
				QITABENT(CredentialProviderFilter, ICredentialProviderFilter), 
				{0},
			};
			return QISearch(this, qit, riid, ppv);
		}

		IFACEMETHODIMP_(ULONG) CredentialProviderFilter::AddRef()
		{
	        return InterlockedIncrement(&m_referenceCount);
		}

		IFACEMETHODIMP_(ULONG) CredentialProviderFilter::Release()
		{
			LONG count = InterlockedDecrement(&m_referenceCount);
			if (!count)
				delete this;
			return count;
		}

		HRESULT STDMETHODCALLTYPE CredentialProviderFilter::Filter(            
			/* [in] */ CREDENTIAL_PROVIDER_USAGE_SCENARIO cpus,
            /* [in] */ DWORD dwFlags,
            /* [size_is][in] */ GUID *rgclsidProviders,
            /* [size_is][out][in] */ BOOL *rgbAllow,
            /* [in] */ DWORD cProviders)
		{
			pDEBUG(L"CredentialProviderFilter::Filter");
			
			switch(cpus)
			{
			case CPUS_LOGON:
				for( DWORD i = 0; i < cProviders; i++ )
				{
					// Disable MS password provider
					if( CLSID_PasswordCredentialProvider == rgclsidProviders[i] )
					{
						pDEBUG(L"CredentialProviderFilter::Filter: Disabling MS password provider");
						rgbAllow[i] = FALSE;
					}
				}
				return S_OK;
			}
			return E_NOTIMPL;
		}

		HRESULT STDMETHODCALLTYPE CredentialProviderFilter::UpdateRemoteCredential( 
            /* [in] */ const CREDENTIAL_PROVIDER_CREDENTIAL_SERIALIZATION *pcpcsIn,
            /* [out] */ CREDENTIAL_PROVIDER_CREDENTIAL_SERIALIZATION *pcpcsOut)
		{
			pDEBUG(L"CredentialProviderFilter::UpdateRemoteCredential: not implemented");
			return E_NOTIMPL;
		}

		CredentialProviderFilter::CredentialProviderFilter(void) :
			m_referenceCount(1)
		{ 
			AddDllReference();
		}


		CredentialProviderFilter::~CredentialProviderFilter(void)
		{
			ReleaseDllReference();
		}
	}
}