
#pragma once

#include "credentialprovider.h"
namespace pGina
{
	namespace CredProv
	{
		class CredentialProviderFilter :
			public ICredentialProviderFilter
		{
		public:

			// IUnknown
			IFACEMETHODIMP_(ULONG) AddRef();    
			IFACEMETHODIMP_(ULONG) Release();    
			IFACEMETHODIMP QueryInterface(__in REFIID riid, __deref_out void** ppv);
			
			// ICredentialProviderFilter
			HRESULT STDMETHODCALLTYPE Filter( 
            /* [in] */ CREDENTIAL_PROVIDER_USAGE_SCENARIO cpus,
            /* [in] */ DWORD dwFlags,
            /* [size_is][in] */ GUID *rgclsidProviders,
            /* [size_is][out][in] */ BOOL *rgbAllow,
            /* [in] */ DWORD cProviders);

			HRESULT STDMETHODCALLTYPE UpdateRemoteCredential( 
            /* [in] */ const CREDENTIAL_PROVIDER_CREDENTIAL_SERIALIZATION *pcpcsIn,
            /* [out] */ CREDENTIAL_PROVIDER_CREDENTIAL_SERIALIZATION *pcpcsOut);
        
			// CredentialProviderFilter
			CredentialProviderFilter(void);
			~CredentialProviderFilter(void);

		private:
			long m_referenceCount;
		
		};

	}
}
