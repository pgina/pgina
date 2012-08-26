/*
	Copyright (c) 2011, pGina Team
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
#include "ClassFactory.h"
#include "Dll.h"

#include "Provider.h"
#include "CredentialProviderFilter.h"

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
			// Crazy ass v-table madness, yay COM!
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
			LONG count = InterlockedDecrement(&m_referenceCount);
			if (!count)
				delete this;
			return count;
		}

		// IClassFactory
		IFACEMETHODIMP CClassFactory::CreateInstance(__in IUnknown* pUnkOuter, __in REFIID riid, __deref_out void **ppv)
		{
			HRESULT hr = CLASS_E_NOAGGREGATION;
			*ppv = NULL;

			if (!pUnkOuter)
			{				
				if( IID_ICredentialProvider == riid )
				{
					pGina::CredProv::Provider* pProvider = new pGina::CredProv::Provider();

					if (pProvider)
					{
						hr = pProvider->QueryInterface(riid, ppv);
						pProvider->Release();
					}
					else
					{
						hr = E_OUTOFMEMORY;
					}
				}
				else if( IID_ICredentialProviderFilter == riid )
				{
					pGina::CredProv::CredentialProviderFilter* pFilter = 
						new pGina::CredProv::CredentialProviderFilter();

					if (pFilter)
					{
						hr = pFilter->QueryInterface(riid, ppv);
						pFilter->Release();
					}
					else
					{
						hr = E_OUTOFMEMORY;
					}
				}
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