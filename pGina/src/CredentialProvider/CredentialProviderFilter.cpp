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
			
			bool doDisable = false;
			switch(cpus)
			{
			case CPUS_LOGON:
				doDisable = pGina::Registry::GetBool(L"DisableMSProviderLogon", false);
				break;
			case CPUS_UNLOCK_WORKSTATION:
				doDisable = pGina::Registry::GetBool(L"DisableMSProviderUnlock", false);
				break;
			case CPUS_CHANGE_PASSWORD:
				doDisable = pGina::Registry::GetBool(L"DisableMSProviderChangePassword", false);
				break;
			}

			if(doDisable)
			{
				for( DWORD i = 0; i < cProviders; i++ )
				{
					// Disable MS password provider
					if( CLSID_PasswordCredentialProvider == rgclsidProviders[i] )
					{
						pDEBUG(L"CredentialProviderFilter::Filter: Disabling MS password provider");
						rgbAllow[i] = FALSE;
					}
				}
			}

			return S_OK;
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