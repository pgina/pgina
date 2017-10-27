/*
	Copyright (c) 2017, pGina Team
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
#include <assert.h>

#include <Macros.h>

#include "Provider.h"
#include "Dll.h"

#pragma warning(push)
#pragma warning(disable : 4995)
#include <shlwapi.h>
#pragma warning(pop)

#include "TileUiLogon.h"
#include "TileUiUnlock.h"
#include "TileUiChangePassword.h"
#include "ProviderGuid.h"
#include "SerializationHelpers.h"
#include "ServiceStateHelper.h"

#include <wincred.h>

namespace pGina
{
	namespace CredProv
	{
		/*static */ bool Provider::m_redraw;

		IFACEMETHODIMP Provider::QueryInterface(__in REFIID riid, __deref_out void **ppv)
		{
			// And more crazy ass v-table madness, yay COM again!
			static const QITAB qit[] =
			{
				QITABENT(Provider, ICredentialProvider),
				{0},
			};
			return QISearch(this, qit, riid, ppv);
		}

		IFACEMETHODIMP_(ULONG) Provider::AddRef()
		{
			// Win 10 workaround to redraw the CP after an error
			if (m_redraw)
			{
				m_redraw = false;
				m_logonUiCallbackEvents->CredentialsChanged(m_logonUiCallbackContext);
			}
			return InterlockedIncrement(&m_referenceCount);
		}

		IFACEMETHODIMP_(ULONG) Provider::Release()
		{
			LONG count = InterlockedDecrement(&m_referenceCount);
			if (!count)
				delete this;
			return count;
		}

		Provider::Provider() :
			m_referenceCount(1),
			m_usageScenario(CPUS_INVALID),
			m_logonUiCallbackEvents(NULL),
			m_logonUiCallbackContext(0),
			m_credential(NULL),
			m_usageFlags(0),
			m_setSerialization(NULL)
		{
			AddDllReference();

			pDEBUG(L"Starting service state helper thread");
			pGina::Service::StateHelper::AddTarget(this);
			pGina::Service::StateHelper::Start();
		}

		Provider::~Provider()
		{
			pDEBUG(L"Stopping service state helper thread (if necessary)");
			if (pGina::Service::StateHelper::GetLoginChangePassword())
				pGina::Transactions::LoginInfo::Move(pGina::Service::StateHelper::GetUsername().c_str(), L"", L"", pGina::Helpers::GetCurrentSessionId(), -1);
			pGina::Service::StateHelper::RemoveTarget(this);
			pGina::Service::StateHelper::Stop();

			UnAdvise();
			ReleaseDllReference();

			if(m_credential)
			{
				m_credential->Release();
				m_credential = NULL;
			}

			if(m_setSerialization)
			{
				LocalFree(m_setSerialization);
				m_setSerialization = NULL;
			}
		}

		// Poorly named, should be QueryUsageScenarioSupport - LogonUI calls this to find out whether the provided
		//  scenario is one which our provider supports.  It also doubles as our shot to do anything before being
		//  called for the scenario in question.
		IFACEMETHODIMP Provider::SetUsageScenario(__in CREDENTIAL_PROVIDER_USAGE_SCENARIO cpus, __in DWORD dwFlags)
		{
			pDEBUG(L"Provider::SetUsageScenario(%d, 0x%08x)", cpus, dwFlags);

			// Returning E_NOTIMPL indicates no support for the requested scenario, otherwise S_OK suffices.
			switch(cpus)
			{
			case CPUS_LOGON:
			case CPUS_UNLOCK_WORKSTATION:
			case CPUS_CREDUI:
				m_usageScenario = cpus;
				m_usageFlags = dwFlags;
				return S_OK;

			case CPUS_CHANGE_PASSWORD:
				m_usageScenario = cpus;
				m_usageFlags = dwFlags;
				return S_OK;

			case CPUS_PLAP:
			case CPUS_INVALID:
				return E_NOTIMPL;

			default:
				return E_INVALIDARG;	// Say wha?
			}
		}

		IFACEMETHODIMP Provider::SetSerialization(__in const CREDENTIAL_PROVIDER_CREDENTIAL_SERIALIZATION* pcpcs)
		{
			pDEBUG(L"SetSerialization(%p)", pcpcs);
			HRESULT result = E_NOTIMPL;

			if ((CLSID_CpGinaProvider != pcpcs->clsidCredentialProvider) && (m_usageScenario == CPUS_CREDUI))
			{
				return E_INVALIDARG;
			}

			// Must match our auth package (negotiate)
			ULONG authPackage = 0;
			result = Microsoft::Sample::RetrieveNegotiateAuthPackage(&authPackage);
			if(!SUCCEEDED(result))
			{
				pDEBUG(L"Failed to retrieve negotiate auth package");
				return result;
			}

			// Slightly modified behavior depending on flags provided to SetUsageScenario
			if(m_usageScenario == CPUS_CREDUI)
			{
				// Must support the auth package specified in CREDUIWIN_IN_CRED_ONLY and CREDUIWIN_AUTHPACKAGE_ONLY
				if( ((m_usageFlags & CREDUIWIN_IN_CRED_ONLY) || (m_usageFlags & CREDUIWIN_AUTHPACKAGE_ONLY))
					&& authPackage != pcpcs->ulAuthenticationPackage)
				{
					pDEBUG(L"Invalid auth package (%x)", pcpcs->ulAuthenticationPackage);
					return E_INVALIDARG;
				}

				// CREDUIWIN_AUTHPACKAGE_ONLY should NOT return S_OK unless we can serialize correctly,
				//  so we default to S_FALSE here and change to S_OK on success.
				if(m_usageFlags & CREDUIWIN_AUTHPACKAGE_ONLY)
				{
					pDEBUG(L"CPUS_CREDUI but flags doesn't indicate CREDUIWIN_AUTHPACKAGE_ONLY");
					result = S_FALSE;
				}
			}

			// As long as the package matches, and there is something to read from
			pDEBUG(L"authPackage: 0x%08x serializedPackage: 0x%08x serializedBytes: 0x%08x @ %p", authPackage, pcpcs->ulAuthenticationPackage, pcpcs->cbSerialization, pcpcs->rgbSerialization);
			if(authPackage == pcpcs->ulAuthenticationPackage && pcpcs->cbSerialization > 0 && pcpcs->rgbSerialization)
			{
				KERB_INTERACTIVE_UNLOCK_LOGON* pkil = (KERB_INTERACTIVE_UNLOCK_LOGON*) pcpcs->rgbSerialization;
				if(pkil->Logon.MessageType == KerbInteractiveLogon)
				{
					// Must have a username
					if(pkil->Logon.UserName.Length && pkil->Logon.UserName.Buffer)
					{
						BYTE * nativeSerialization = NULL;
						DWORD nativeSerializationSize = 0;

						// Do we need to repack in native format? (32 bit client talking to 64 bit host or vice versa)
						if(m_usageScenario == CPUS_CREDUI && (CREDUIWIN_PACK_32_WOW & m_usageFlags))
						{
							if(!SUCCEEDED(Microsoft::Sample::KerbInteractiveUnlockLogonRepackNative(pcpcs->rgbSerialization, pcpcs->cbSerialization,
								&nativeSerialization, &nativeSerializationSize)))
							{
								return result;
							}
						}
						else
						{
							nativeSerialization = (BYTE*) LocalAlloc(LMEM_ZEROINIT, pcpcs->cbSerialization);
							nativeSerializationSize = pcpcs->cbSerialization;

							if(!nativeSerialization)
								return E_OUTOFMEMORY;

							CopyMemory(nativeSerialization, pcpcs->rgbSerialization, pcpcs->cbSerialization);
						}

						Microsoft::Sample::KerbInteractiveUnlockLogonUnpackInPlace((KERB_INTERACTIVE_UNLOCK_LOGON *) nativeSerialization, nativeSerializationSize);
						if(m_setSerialization) LocalFree(m_setSerialization);
						m_setSerialization = (KERB_INTERACTIVE_UNLOCK_LOGON *) nativeSerialization;
						pDEBUG(L"m_setSerialization = %p", m_setSerialization);
						result = S_OK;	// All is well!
					}
				}
			}

			return result;
		}

		IFACEMETHODIMP Provider::Advise(__in ICredentialProviderEvents* pcpe, __in UINT_PTR upAdviseContext)
		{
			// If we already have a callback handle, release our reference to it
			UnAdvise();
			
			// Store what we've been given
			m_logonUiCallbackEvents = pcpe;
			m_logonUiCallbackContext = upAdviseContext;

			// Up ref count as we hold a pointer to this guy
			if(m_logonUiCallbackEvents)
			{
				if (pGina::Service::StateHelper::GetLoginChangePassword())
				{
					if (m_usageScenario != CPUS_CHANGE_PASSWORD)
					{
						pGina::Transactions::LoginInfo::Move(pGina::Service::StateHelper::GetUsername().c_str(), L"", L"", pGina::Helpers::GetCurrentSessionId(), -1);
						pGina::Service::StateHelper::PushUsername(L"", L"", false);
						m_credential = new Credential();
						m_credential->Initialize(m_usageScenario, s_logonFields, m_usageFlags, NULL, NULL);
					}
					else
					{
						if (m_credential)
						{
							m_credential->Initialize(m_usageScenario, s_changePasswordFields, m_usageFlags, pGina::Service::StateHelper::GetUsername().c_str(), pGina::Service::StateHelper::GetPassword().c_str());
						}
					}
					m_logonUiCallbackEvents->CredentialsChanged(m_logonUiCallbackContext);
				}

				pDEBUG(L"Provider::Advise(%p, %p) - provider events callback reference added", pcpe, upAdviseContext);
				m_logonUiCallbackEvents->AddRef();
			}

			return S_OK;
		}

		IFACEMETHODIMP Provider::UnAdvise()
		{
			if(m_logonUiCallbackEvents)
			{
				pDEBUG(L"Provider::UnAdvise() - provider events callback reference released");
				m_logonUiCallbackEvents->Release();
				m_logonUiCallbackEvents = NULL;
				m_logonUiCallbackContext = 0;
			}

			return S_OK;
		}

		IFACEMETHODIMP Provider::GetFieldDescriptorCount(__out DWORD* pdwCount)
		{
			// # of fields depends on our usage scenario:
			switch(m_usageScenario)
			{
			case CPUS_LOGON:
			case CPUS_CREDUI:
				*pdwCount = LUIFI_NUM_FIELDS;
				return S_OK;
			case CPUS_UNLOCK_WORKSTATION:
				*pdwCount = LOIFI_NUM_FIELDS;
				return S_OK;
			case CPUS_CHANGE_PASSWORD:
				*pdwCount = CPUIFI_NUM_FIELDS;
				return S_OK;
			default:
				pERROR(L"Provider::GetFieldDescriptorCount: No UI known for the usage scenario: 0x%08x", m_usageScenario);
				return S_FALSE;
			}

			// Should never reach this
			assert(0);
			return S_FALSE;
		}

		IFACEMETHODIMP Provider::GetFieldDescriptorAt(__in DWORD dwIndex,  __deref_out CREDENTIAL_PROVIDER_FIELD_DESCRIPTOR** ppcpfd)
		{
			switch(m_usageScenario)
			{
			case CPUS_LOGON:
			case CPUS_CREDUI:
				return GetFieldDescriptorForUi(s_logonFields, dwIndex, ppcpfd);
			case CPUS_UNLOCK_WORKSTATION:
				return GetFieldDescriptorForUi(s_unlockFields, dwIndex, ppcpfd);
			case CPUS_CHANGE_PASSWORD:
				return GetFieldDescriptorForUi(s_changePasswordFields, dwIndex, ppcpfd);
			default:
				return E_INVALIDARG;
			}

			// Should never reach this
			assert(0);
			return S_FALSE;
		}

		IFACEMETHODIMP Provider::GetCredentialCount(__out DWORD* pdwCount, __out_range(<,*pdwCount) DWORD* pdwDefault, __out BOOL* pbAutoLogonWithDefault)
		{
			// We currently always support only a single tile
			*pdwCount = 1;
			*pdwDefault = 0;
			*pbAutoLogonWithDefault = FALSE;

			// If we were given creds via SetSerialization, and they appear complete, then we can
			//  make that credential our default and attempt an autologon.
			if(SerializedUserNameAvailable() && SerializedPasswordAvailable() && !pGina::Service::StateHelper::GetLoginChangePassword())
			{
				*pdwDefault = 0;
				*pbAutoLogonWithDefault = TRUE;
			}
			return S_OK;
		}

		IFACEMETHODIMP Provider::GetCredentialAt(__in DWORD dwIndex, __deref_out ICredentialProviderCredential** ppcpc)
		{
			// Currently we have just the one, we lazy init it here when first requested
			if(!m_credential)
			{
				m_credential = new Credential();

				pGina::Memory::ObjectCleanupPool cleanup;

				PWSTR serializedUser, serializedPass, serializedDomain;
				GetSerializedCredentials(&serializedUser, &serializedPass, &serializedDomain);
				if(serializedUser != NULL)
					cleanup.Add(new pGina::Memory::LocalFreeCleanup(serializedUser));
				if(serializedPass != NULL)
					cleanup.Add(new pGina::Memory::LocalFreeCleanup(serializedPass));
				if(serializedDomain != NULL)
					cleanup.Add(new pGina::Memory::LocalFreeCleanup(serializedDomain));

				switch(m_usageScenario)
				{
				case CPUS_LOGON:
				case CPUS_CREDUI:
					m_credential->Initialize(m_usageScenario, s_logonFields, m_usageFlags, serializedUser, serializedPass);
					break;
				case CPUS_UNLOCK_WORKSTATION:
					m_credential->Initialize(m_usageScenario, s_unlockFields, m_usageFlags, serializedUser, serializedPass);
					break;
				case CPUS_CHANGE_PASSWORD:
					m_credential->Initialize(m_usageScenario, s_changePasswordFields, m_usageFlags, serializedUser, serializedPass);
					break;
				default:
					return E_INVALIDARG;
				}
			}

			// Did we fail to create it? OOM
			if(!m_credential) return E_OUTOFMEMORY;

			// Better be index 0 (we only have 1 currently)
			if(dwIndex != 0 || !ppcpc)
				return E_INVALIDARG;

			// Alright... QueryIface for ICredentialProviderCredential
			if (m_usageScenario == CPUS_CREDUI)
			{
				return m_credential->QueryInterface(IID_ICredentialProviderCredential, reinterpret_cast<void **>(ppcpc));
			}
			return m_credential->QueryInterface(IID_IConnectableCredentialProviderCredential, reinterpret_cast<void **>(ppcpc));
		}

		IFACEMETHODIMP Provider::GetFieldDescriptorForUi(UI_FIELDS const& fields, DWORD index, CREDENTIAL_PROVIDER_FIELD_DESCRIPTOR **ppcpfd)
		{
			// Must be in our count of fields, and we have to have somewhere to stuff the result
			if(index >= fields.fieldCount && ppcpfd) return E_INVALIDARG;

			// Should we fail, we want to return a NULL for result
			*ppcpfd = NULL;

			// Use CoTaskMemAlloc for the resulting value, then copy in our descriptor
			DWORD structSize = sizeof(**ppcpfd);
			CREDENTIAL_PROVIDER_FIELD_DESCRIPTOR *pcpfd = (CREDENTIAL_PROVIDER_FIELD_DESCRIPTOR *) CoTaskMemAlloc(structSize);
			if(pcpfd == NULL) return E_OUTOFMEMORY;

			// Use compilers struct copy, in case fields change down the road
			*pcpfd = fields.fields[index].fieldDescriptor;

			// But now we have to fixup the label, which is a ptr, we'll use SHStrDupW which does CoTask alloc
			if(pcpfd->pszLabel)
			{
				if(!SUCCEEDED(SHStrDupW(fields.fields[index].fieldDescriptor.pszLabel, &pcpfd->pszLabel)))
				{
					// Dup failed, free up what we've got so far, then get out
					CoTaskMemFree(pcpfd);
					return E_OUTOFMEMORY;
				}
			}

			// Got here? Then we win!
			*ppcpfd = pcpfd;
			return S_OK;
		}

		bool Provider::SerializedUserNameAvailable()
		{
			// Did we get any creds?
			if(!m_setSerialization)
			{
				pDEBUG(L"SerializedUserNameAvailable: No serialized creds set");
				return false;
			}

			// Did we get a username?
			if(m_setSerialization->Logon.UserName.Length && m_setSerialization->Logon.UserName.Buffer)
				return true;

			pDEBUG(L"SerializedUserNameAvailable: couldn't work out username");
			return false;
		}

		bool Provider::SerializedPasswordAvailable()
		{
			// Did we get any creds?
			if(!m_setSerialization)
			{
				pDEBUG(L"SerializedPasswordAvailable: No serialized creds set");
				return false;
			}

			// Did we get a password?
			if(m_setSerialization->Logon.Password.Length && m_setSerialization->Logon.Password.Buffer)
				return true;

			pDEBUG(L"SerializedPasswordAvailable: couldn't work out password");
			return false;
		}

		bool Provider::SerializedDomainNameAvailable()
		{
			// Did we get any creds?
			if(!m_setSerialization)
			{
				pDEBUG(L"SerializedDomainNameAvailable: No serialized creds set");
				return false;
			}

			// Did we get a domain name?
			if(m_setSerialization->Logon.LogonDomainName.Length && m_setSerialization->Logon.LogonDomainName.Buffer)
				return true;

			pDEBUG(L"SerializedDomainNameAvailable: couldn't work out domain name");
			return false;
		}

		void Provider::GetSerializedCredentials(PWSTR *username, PWSTR *password, PWSTR *domain)
		{
			if (username)
			{
				if (SerializedUserNameAvailable())
				{
					if (SerializedDomainNameAvailable())
					{
						*username = (PWSTR) LocalAlloc(LMEM_ZEROINIT, m_setSerialization->Logon.UserName.Length + sizeof(wchar_t) + m_setSerialization->Logon.LogonDomainName.Length + sizeof(wchar_t));

						HLOCAL u = LocalAlloc(LMEM_ZEROINIT, m_setSerialization->Logon.UserName.Length + sizeof(wchar_t));
						CopyMemory(u, m_setSerialization->Logon.UserName.Buffer, m_setSerialization->Logon.UserName.Length);
						HLOCAL d = LocalAlloc(LMEM_ZEROINIT, m_setSerialization->Logon.LogonDomainName.Length + sizeof(wchar_t));
						CopyMemory(d, m_setSerialization->Logon.LogonDomainName.Buffer, m_setSerialization->Logon.LogonDomainName.Length);
						std::wstring t = (PWSTR)u + std::wstring(L"@") + (PWSTR)d;
						CopyMemory(*username, t.c_str(), m_setSerialization->Logon.UserName.Length + sizeof(wchar_t) + m_setSerialization->Logon.LogonDomainName.Length);
						LocalFree(u);
						LocalFree(d);
					}
					else
					{
						*username = (PWSTR) LocalAlloc(LMEM_ZEROINIT, m_setSerialization->Logon.UserName.Length + sizeof(wchar_t));
						CopyMemory(*username, m_setSerialization->Logon.UserName.Buffer, m_setSerialization->Logon.UserName.Length);
					}
				}
				else
					*username = NULL;
			}

			if (password)
			{
				if (SerializedPasswordAvailable())
				{
					*password = (PWSTR) LocalAlloc(LMEM_ZEROINIT, m_setSerialization->Logon.Password.Length + sizeof(wchar_t));
					CopyMemory(*password, m_setSerialization->Logon.Password.Buffer, m_setSerialization->Logon.Password.Length);
				}
				else
					*password = NULL;
			}

			if (domain)
			{
				if (SerializedDomainNameAvailable())
				{
					*domain = (PWSTR) LocalAlloc(LMEM_ZEROINIT, m_setSerialization->Logon.LogonDomainName.Length + sizeof(wchar_t));
					CopyMemory(*domain, m_setSerialization->Logon.LogonDomainName.Buffer, m_setSerialization->Logon.LogonDomainName.Length);
				}
				else
					*domain = NULL;
			}
		}

		void Provider::ServiceStateChanged(bool newState)
		{
			if(m_logonUiCallbackEvents)
			{
				m_logonUiCallbackEvents->CredentialsChanged(m_logonUiCallbackContext);
			}
		}


	}
}