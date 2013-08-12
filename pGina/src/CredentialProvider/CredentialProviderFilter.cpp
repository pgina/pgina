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
#include "ProviderGuid.h"

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

			// Retrieve the registry settings
			std::vector<std::wstring> rawFilterSettings = 
				pGina::Registry::GetStringArray(L"CredentialProviderFilters");

			// If there's nothing there, there's nothing to do.
			if( rawFilterSettings.size() == 0 ) return S_OK;

			// Unpack the settings
			struct FilterSetting { GUID uuid; int filter; std::wstring uuidStr; };
			std::vector<struct FilterSetting> filterSettings;
			for( DWORD i = 0; i < rawFilterSettings.size(); i++ )
			{
				std::wstring s = rawFilterSettings[i];
				size_t idx = s.find_first_of(L"\t");
				if( idx != std::wstring::npos )
				{
					std::wstring guidStr = s.substr(0, idx);
					std::wstring filterStr = s.substr(idx+1);
					
					struct FilterSetting setting;
					HRESULT hr = CLSIDFromString(guidStr.c_str(), &(setting.uuid));
					if( SUCCEEDED(hr) )
					{
						setting.uuidStr = guidStr;
						setting.filter = _wtoi(filterStr.c_str());
						filterSettings.push_back(setting);
						//pDEBUG(L"Loaded filter setting: %s %d", setting.uuidStr.c_str(), setting.filter);
					}
				}
			}

			// Loop over each cred prov and see if we need to filter it
			for( DWORD i = 0; i < cProviders; i++ )
			{
				bool doFilter = false;
				struct FilterSetting setting;
				for( DWORD j = 0; j < filterSettings.size(); j++ )
				{
					if( IsEqualGUID( filterSettings[j].uuid, rgclsidProviders[i] ) )
					{
						doFilter = true;
						setting = filterSettings[j];
					}
				}
				
				// If we are filtering this CP
				if( doFilter )
				{
					// If we are configured to filter in this scenario
					if(
						(cpus == CPUS_LOGON && ((setting.filter & 0x1) != 0)) ||
						(cpus == CPUS_UNLOCK_WORKSTATION && ((setting.filter & 0x2) != 0)) ||
						(cpus == CPUS_CHANGE_PASSWORD && ((setting.filter & 0x4) != 0)) ||
						(cpus == CPUS_CREDUI && ((setting.filter & 0x8) != 0))
						)
					{
						pDEBUG(L"Filtering %s", setting.uuidStr.c_str() );
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
			HRESULT result;

			if( pcpcsIn != NULL && pcpcsIn->cbSerialization > 0
				&& (pcpcsOut->rgbSerialization = (BYTE *)CoTaskMemAlloc(pcpcsIn->cbSerialization)) != NULL )
			{
				pcpcsOut->ulAuthenticationPackage = pcpcsIn->ulAuthenticationPackage;
				pcpcsOut->clsidCredentialProvider = CLSID_CpGinaProvider;
				pcpcsOut->cbSerialization = pcpcsIn->cbSerialization;
				CopyMemory(pcpcsOut->rgbSerialization, pcpcsIn->rgbSerialization, pcpcsIn->cbSerialization);
				result = S_OK;
			}
			else
				result = S_FALSE;

			pDEBUG(L"CredentialProviderFilter::UpdateRemoteCredential(%p) returns %ld", pcpcsIn, result);
			return result;
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