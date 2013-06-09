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
#include <Windows.h>

#include "Credential.h"
#include "Dll.h"

#pragma warning(push)
#pragma warning(disable : 4995)
#include <shlwapi.h>
#pragma warning(pop)

#include <Macros.h>

#include "ClassFactory.h"
#include "TileUiTypes.h"
#include "TileUiLogon.h"
#include "TileUiUnlock.h"
#include "SerializationHelpers.h"
#include "ServiceStateHelper.h"
#include "ProviderGuid.h"
#include "resource.h"

#include <wincred.h>

namespace pGina
{
	namespace CredProv
	{
		IFACEMETHODIMP Credential::QueryInterface(__in REFIID riid, __deref_out void **ppv)
		{
			static const QITAB qitBaseOnly[] =
			{
				QITABENT(Credential, ICredentialProviderCredential),				
				{0},
			};

			static const QITAB qitFull[] =
			{
				QITABENT(Credential, ICredentialProviderCredential),
				QITABENT(Credential, IConnectableCredentialProviderCredential), 
				{0},
			};

			if(m_usageScenario == CPUS_CREDUI)
			{			
				return QISearch(this, qitBaseOnly, riid, ppv);
			}
			else
			{
				return QISearch(this, qitFull, riid, ppv);
			}			
		}

		IFACEMETHODIMP_(ULONG) Credential::AddRef()
		{
	        return InterlockedIncrement(&m_referenceCount);
		}

		IFACEMETHODIMP_(ULONG) Credential::Release()
		{
			LONG count = InterlockedDecrement(&m_referenceCount);
			if (!count)
				delete this;
			return count;
		}

		IFACEMETHODIMP Credential::Advise(__in ICredentialProviderCredentialEvents* pcpce)
		{
			// Release any ref for current ptr (if any)
			UnAdvise();

			m_logonUiCallback = pcpce;
			
			if(m_logonUiCallback)
			{
				m_logonUiCallback->AddRef();			
			}

			return S_OK;
		}
		
		IFACEMETHODIMP Credential::UnAdvise()
		{
			if(m_logonUiCallback)
			{
				m_logonUiCallback->Release();
				m_logonUiCallback = NULL;
			}

			return S_OK;
		}

		IFACEMETHODIMP Credential::SetSelected(__out BOOL* pbAutoLogon)
		{
			// We don't do anything special here, but twould be the place to react to our tile being selected
			*pbAutoLogon = FALSE;
			return S_OK;
		}

		IFACEMETHODIMP Credential::SetDeselected()
		{
			// No longer selected, if we have any password fields set, lets zero/clear/free them
			ClearZeroAndFreeAnyPasswordFields(true);
			return S_OK;
		}

		IFACEMETHODIMP Credential::GetFieldState(__in DWORD dwFieldID, __out CREDENTIAL_PROVIDER_FIELD_STATE* pcpfs, __out CREDENTIAL_PROVIDER_FIELD_INTERACTIVE_STATE* pcpfis)
		{			
			if(!m_fields || dwFieldID >= m_fields->fieldCount || !pcpfs || !pcpfis)
				return E_INVALIDARG;

			*pcpfs = m_fields->fields[dwFieldID].fieldStatePair.fieldState;
			*pcpfis = m_fields->fields[dwFieldID].fieldStatePair.fieldInteractiveState;
			return S_OK;
		}
		
		IFACEMETHODIMP Credential::GetStringValue(__in DWORD dwFieldID, __deref_out PWSTR* ppwsz)
		{
			if(!m_fields || dwFieldID >= m_fields->fieldCount || !ppwsz)
				return E_INVALIDARG;

			if(IsFieldDynamic(dwFieldID))
			{
				std::wstring text = GetTextForField(dwFieldID);
				if( ! text.empty() )
					return SHStrDupW( text.c_str(), ppwsz );
			}	

			// We copy our value with SHStrDupW which uses CoTask alloc, caller owns result
			if(m_fields->fields[dwFieldID].wstr)
				return SHStrDupW(m_fields->fields[dwFieldID].wstr, ppwsz);

			*ppwsz = NULL;
			return S_OK;			
		}

		IFACEMETHODIMP Credential::GetBitmapValue(__in DWORD dwFieldID, __out HBITMAP* phbmp)
		{
			if(!m_fields || dwFieldID >= m_fields->fieldCount || !phbmp)
				return E_INVALIDARG;

			if(m_fields->fields[dwFieldID].fieldDescriptor.cpft != CPFT_TILE_IMAGE)
				return E_INVALIDARG;

			HBITMAP bitmap = NULL;
			std::wstring tileImage = pGina::Registry::GetString(L"TileImage", L"");
			if(tileImage.empty() || tileImage.length() == 1)
			{
				// Use builtin
				bitmap = LoadBitmap(GetMyInstance(), MAKEINTRESOURCE(IDB_LOGO_MONOCHROME_200));
			}
			else
			{
				pDEBUG(L"Credential::GetBitmapValue: Loading image from: %s", tileImage.c_str());
				bitmap = (HBITMAP) LoadImageW((HINSTANCE) NULL, tileImage.c_str(), IMAGE_BITMAP, 0, 0, LR_LOADFROMFILE);			
			}
			
			if(!bitmap)
				return HRESULT_FROM_WIN32(GetLastError());
			
			*phbmp = bitmap;
			return S_OK;
		}

		IFACEMETHODIMP Credential::GetCheckboxValue(__in DWORD dwFieldID, __out BOOL* pbChecked, __deref_out PWSTR* ppwszLabel)
		{
			return E_NOTIMPL;
		}

		IFACEMETHODIMP Credential::GetComboBoxValueCount(__in DWORD dwFieldID, __out DWORD* pcItems, __out_range(<,*pcItems) DWORD* pdwSelectedItem)
		{
			return E_NOTIMPL;
		}

		IFACEMETHODIMP Credential::GetComboBoxValueAt(__in DWORD dwFieldID, __in DWORD dwItem, __deref_out PWSTR* ppwszItem)
		{
			return E_NOTIMPL;
		}

		IFACEMETHODIMP Credential::GetSubmitButtonValue(__in DWORD dwFieldID, __out DWORD* pdwAdjacentTo)
		{
			if(!m_fields || dwFieldID >= m_fields->fieldCount || !pdwAdjacentTo)
				return E_INVALIDARG;

			if(m_fields->fields[dwFieldID].fieldDescriptor.cpft != CPFT_SUBMIT_BUTTON)
				return E_INVALIDARG;

			*pdwAdjacentTo = m_fields->submitAdjacentTo;
			return S_OK;
		}

		IFACEMETHODIMP Credential::SetStringValue(__in DWORD dwFieldID, __in PCWSTR pwz)
		{			
			if(!m_fields || dwFieldID >= m_fields->fieldCount)
				return E_INVALIDARG;

			if(m_fields->fields[dwFieldID].fieldDescriptor.cpft != CPFT_EDIT_TEXT &&
			   m_fields->fields[dwFieldID].fieldDescriptor.cpft != CPFT_PASSWORD_TEXT &&
			   m_fields->fields[dwFieldID].fieldDescriptor.cpft != CPFT_SMALL_TEXT &&
			   m_fields->fields[dwFieldID].fieldDescriptor.cpft != CPFT_LARGE_TEXT)
				return E_INVALIDARG;

			if(m_fields->fields[dwFieldID].wstr)
			{
				CoTaskMemFree(m_fields->fields[dwFieldID].wstr);
				m_fields->fields[dwFieldID].wstr = NULL;
			}
			
			if(pwz)
			{
				return SHStrDupW(pwz, &m_fields->fields[dwFieldID].wstr);
			}
			return S_OK;
		}

		IFACEMETHODIMP Credential::SetCheckboxValue(__in DWORD dwFieldID, __in BOOL bChecked)
		{
			return E_NOTIMPL;
		}

		IFACEMETHODIMP Credential::SetComboBoxSelectedValue(__in DWORD dwFieldID, __in DWORD dwSelectedItem)
		{
			return E_NOTIMPL;
		}

		IFACEMETHODIMP Credential::CommandLinkClicked(__in DWORD dwFieldID)
		{
			return E_NOTIMPL;
		}

		IFACEMETHODIMP Credential::GetSerialization(__out CREDENTIAL_PROVIDER_GET_SERIALIZATION_RESPONSE* pcpgsr, __out CREDENTIAL_PROVIDER_CREDENTIAL_SERIALIZATION* pcpcs,
													__deref_out_opt PWSTR* ppwszOptionalStatusText, __out CREDENTIAL_PROVIDER_STATUS_ICON* pcpsiOptionalStatusIcon)
		{
			pDEBUG(L"Credential::GetSerialization, enter");
			// If we are operating in a non CredUI scenario, then 
			// Credential::Connect will have executed prior to this method, which contacts the
			// service, so m_loginResult should have the result from the plugins. Otherwise,
			// consider this the attempt to login!
			if(m_usageScenario == CPUS_CREDUI)
			{
				ProcessLoginAttempt(NULL);
			}

			if( m_logonCancelled )
			{
				// User clicked cancel during logon
				pDEBUG(L"Credential::GetSerialization - Logon was cancelled, returning S_FALSE");
				SHStrDupW(L"Logon cancelled", ppwszOptionalStatusText);
				*pcpgsr = CPGSR_NO_CREDENTIAL_FINISHED;										
				*pcpsiOptionalStatusIcon = CPSI_ERROR;
				return S_FALSE;
			}

			if(!m_loginResult.Result())
			{
				pERROR(L"Credential::GetSerialization: Failed login");
				if(m_loginResult.Message().length() > 0)
				{
					SHStrDupW(m_loginResult.Message().c_str(), ppwszOptionalStatusText);					
				}
				else
				{
					SHStrDupW(L"ProcessLoginForUser failed, but a specific error message was not provided", ppwszOptionalStatusText);
				}
				
				*pcpgsr = CPGSR_NO_CREDENTIAL_FINISHED;										
				*pcpsiOptionalStatusIcon = CPSI_ERROR;
				return S_FALSE;
			}

			// At this point the info has passed to the service and been validated, so now we have to pack it up and provide it back to
			// LogonUI/Winlogon as a serialized/packed logon structure.

			pGina::Memory::ObjectCleanupPool cleanup;

			PWSTR username = m_loginResult.Username().length() > 0 ? _wcsdup(m_loginResult.Username().c_str()) : NULL;
			PWSTR password = m_loginResult.Password().length() > 0 ? _wcsdup(m_loginResult.Password().c_str()) : NULL;
			PWSTR domain = m_loginResult.Domain().length() > 0 ? _wcsdup(m_loginResult.Domain().c_str()) : NULL;			

			cleanup.AddFree(username);
			cleanup.AddFree(password);
			cleanup.AddFree(domain);

			PWSTR protectedPassword = NULL;			
			HRESULT result = Microsoft::Sample::ProtectIfNecessaryAndCopyPassword(password, m_usageScenario, &protectedPassword);			
			if(!SUCCEEDED(result))
				return result;

			cleanup.Add(new pGina::Memory::CoTaskMemFreeCleanup(protectedPassword));			

			// CredUI we use CredPackAuthenticationBuffer
			if(m_usageScenario == CPUS_CREDUI)
			{
				// Need username/domain as a single string
				PWSTR domainUsername = NULL;
				result = Microsoft::Sample::DomainUsernameStringAlloc(domain, username, &domainUsername);
				if(SUCCEEDED(result))
				{
					DWORD size = 0;
					BYTE* rawbits = NULL;
					
					if(!CredPackAuthenticationBufferW((CREDUIWIN_PACK_32_WOW & m_usageFlags) ? CRED_PACK_WOW_BUFFER : 0, domainUsername, protectedPassword, rawbits, &size))
					{
						if(GetLastError() == ERROR_INSUFFICIENT_BUFFER)
						{
							rawbits = (BYTE *)HeapAlloc(GetProcessHeap(), 0, size);
							if(!CredPackAuthenticationBufferW((CREDUIWIN_PACK_32_WOW & m_usageFlags) ? CRED_PACK_WOW_BUFFER : 0, domainUsername, protectedPassword, rawbits, &size))
							{
								HeapFree(GetProcessHeap(), 0, rawbits);
								HeapFree(GetProcessHeap(), 0, domainUsername);
								return HRESULT_FROM_WIN32(GetLastError());
							}

							pcpcs->rgbSerialization = rawbits;
							pcpcs->cbSerialization = size;
						}
						else
						{
							HeapFree(GetProcessHeap(), 0, domainUsername);
							return E_FAIL;
						}
					}
				}
			}
			else
			{
				// Init kiul
				KERB_INTERACTIVE_UNLOCK_LOGON kiul;
				result = Microsoft::Sample::KerbInteractiveUnlockLogonInit(domain, username, password, m_usageScenario, &kiul);
				if(!SUCCEEDED(result))
					return result;

				// Pack for the negotiate package and include our CLSID
				result = Microsoft::Sample::KerbInteractiveUnlockLogonPack(kiul, &pcpcs->rgbSerialization, &pcpcs->cbSerialization);
				if(!SUCCEEDED(result))
					return result;
			}
			
			ULONG authPackage = 0;
			result = Microsoft::Sample::RetrieveNegotiateAuthPackage(&authPackage);
			if(!SUCCEEDED(result))
				return result;
						
			pcpcs->ulAuthenticationPackage = authPackage;
			pcpcs->clsidCredentialProvider = CLSID_CpGinaProvider;
			*pcpgsr = CPGSR_RETURN_CREDENTIAL_FINISHED;            
            
			return S_OK;
        }
    
		IFACEMETHODIMP Credential::ReportResult(__in NTSTATUS ntsStatus, __in NTSTATUS ntsSubstatus, __deref_out_opt PWSTR* ppwszOptionalStatusText, 
												__out CREDENTIAL_PROVIDER_STATUS_ICON* pcpsiOptionalStatusIcon)
		{
			pDEBUG(L"Credential::ReportResult(0x%08x, 0x%08x) called", ntsStatus, ntsSubstatus);
			/**ppwszOptionalStatusText = NULL;
			*pcpsiOptionalStatusIcon = CPSI_NONE;*/
			return E_NOTIMPL;
		}

		Credential::Credential() :
			m_referenceCount(1),
			m_usageScenario(CPUS_INVALID),
			m_logonUiCallback(NULL),
			m_fields(NULL),			
			m_usageFlags(0),
			m_logonCancelled(false)
		{
			AddDllReference();

			if( pGina::Registry::GetBool(L"ShowServiceStatusInLogonUi", true) )
				pGina::Service::StateHelper::AddTarget(this);
		}
		
		Credential::~Credential()
		{
			pGina::Service::StateHelper::RemoveTarget(this);
			ClearZeroAndFreeAnyTextFields(false);	// Free memory used to back text fields, no ui update
			ReleaseDllReference();
		}

		void Credential::Initialize(CREDENTIAL_PROVIDER_USAGE_SCENARIO cpus, UI_FIELDS const& fields, DWORD usageFlags, const wchar_t *username, const wchar_t *password)
		{
			m_usageScenario = cpus;
			m_usageFlags = usageFlags;

			// Allocate and copy our UI_FIELDS struct, we need our own copy to set/track the state of
			//  our fields over time
			m_fields = (UI_FIELDS *) (malloc(sizeof(UI_FIELDS) + (sizeof(UI_FIELD) * fields.fieldCount)));
			m_fields->fieldCount = fields.fieldCount;
			m_fields->submitAdjacentTo = fields.submitAdjacentTo;
			m_fields->usernameFieldIdx = fields.usernameFieldIdx;
			m_fields->passwordFieldIdx = fields.passwordFieldIdx;
			m_fields->statusFieldIdx = fields.statusFieldIdx;
			for(DWORD x = 0; x < fields.fieldCount; x++)
			{
				m_fields->fields[x].fieldDescriptor = fields.fields[x].fieldDescriptor;
				m_fields->fields[x].fieldStatePair = fields.fields[x].fieldStatePair;
				m_fields->fields[x].fieldDataSource = fields.fields[x].fieldDataSource;
				m_fields->fields[x].wstr = NULL;

				if(fields.fields[x].wstr && !IsFieldDynamic(x))
				{
					SHStrDup(fields.fields[x].wstr, &m_fields->fields[x].wstr);
				}

				if(IsFieldDynamic(x))
				{
					std::wstring text = GetTextForField(x);
					if( ! text.empty() )
					{
						SHStrDup( text.c_str(), &m_fields->fields[x].wstr );
					}
				}				
			}			


			if(username != NULL)
			{				
				SHStrDupW(username, &(m_fields->fields[m_fields->usernameFieldIdx].wstr));
			}
			else if(m_usageScenario == CPUS_UNLOCK_WORKSTATION)
			{
				DWORD mySession = pGina::Helpers::GetCurrentSessionId();
				std::wstring username, domain;    // Username and domain to be determined
				std::wstring usernameFieldValue;  // The value for the username field
				std::wstring machineName = pGina::Helpers::GetMachineName();

				// Get user information from service (if available)
				pDEBUG(L"Retrieving user information from service.");
				pGina::Transactions::LoginInfo::UserInformation userInfo = 
					pGina::Transactions::LoginInfo::GetUserInformation(mySession);
				pDEBUG(L"Received: original uname: '%s' uname: '%s' domain: '%s'", 
					userInfo.OriginalUsername().c_str(), userInfo.Username().c_str(), userInfo.Domain().c_str());

				// Grab the domain if available
				if( ! userInfo.Domain().empty() )
					domain = userInfo.Domain();

				// Are we configured to use the original username?
				if( pGina::Registry::GetBool(L"UseOriginalUsernameInUnlockScenario", false) )
					username = userInfo.OriginalUsername();
				else
					username = userInfo.Username();

				// If we didn't get a username/domain from the service, try to get it from WTS
				if( username.empty() )
					username = pGina::Helpers::GetSessionUsername(mySession);
				if( domain.empty() )
					domain = pGina::Helpers::GetSessionDomainName(mySession);
					

				if(!domain.empty() && _wcsicmp(domain.c_str(), machineName.c_str()) != 0)
				{
					usernameFieldValue += domain;
					usernameFieldValue += L"\\";
				}

				usernameFieldValue += username;
				
				SHStrDupW(usernameFieldValue.c_str(), &(m_fields->fields[m_fields->usernameFieldIdx].wstr));
			}

			if(password != NULL)
			{				
				SHStrDupW(password, &(m_fields->fields[m_fields->passwordFieldIdx].wstr));
			}

			// Hide MOTD field if not enabled
			if( ! pGina::Registry::GetBool(L"EnableMotd", true) )
				if( m_usageScenario == CPUS_LOGON )
					m_fields->fields[CredProv::LUIFI_MOTD].fieldStatePair.fieldState = CPFS_HIDDEN;

			// Hide service status if configured to do so
			if( ! pGina::Registry::GetBool(L"ShowServiceStatusInLogonUi", true) )
			{
				if( m_usageScenario == CPUS_UNLOCK_WORKSTATION )
					m_fields->fields[CredProv::LOIFI_STATUS].fieldStatePair.fieldState = CPFS_HIDDEN;
				else if( m_usageScenario == CPUS_LOGON )
					m_fields->fields[CredProv::LUIFI_STATUS].fieldStatePair.fieldState = CPFS_HIDDEN;
			}
		}

		void Credential::ClearZeroAndFreeAnyPasswordFields(bool updateUi)
		{
			ClearZeroAndFreeFields(CPFT_PASSWORD_TEXT, updateUi);					
    	}

		void Credential::ClearZeroAndFreeAnyTextFields(bool updateUi)
		{
			ClearZeroAndFreeFields(CPFT_PASSWORD_TEXT, updateUi);
			ClearZeroAndFreeFields(CPFT_EDIT_TEXT, updateUi);
		}

		void Credential::ClearZeroAndFreeFields(CREDENTIAL_PROVIDER_FIELD_TYPE type, bool updateUi)
		{
			if(!m_fields) return;

			for(DWORD x = 0; x < m_fields->fieldCount; x++)
			{
				if(m_fields->fields[x].fieldDescriptor.cpft == type)
				{
					if(m_fields->fields[x].wstr)
					{
						size_t len = wcslen(m_fields->fields[x].wstr);
						SecureZeroMemory(m_fields->fields[x].wstr, len * sizeof(wchar_t));
						CoTaskMemFree(m_fields->fields[x].wstr);						
						m_fields->fields[x].wstr = NULL;

						// If we've been advised, we can tell the UI so the UI correctly reflects that this
						//	field is not set any longer (set it to empty string)
						if(m_logonUiCallback && updateUi)
						{
							m_logonUiCallback->SetFieldString(this, m_fields->fields[x].fieldDescriptor.dwFieldID, L"");
						}
					}
				}
			}	
		}

		PWSTR Credential::FindUsernameValue()
		{
			if(!m_fields) return NULL;
			return m_fields->fields[m_fields->usernameFieldIdx].wstr;
		}

		PWSTR Credential::FindPasswordValue()
		{
			if(!m_fields) return NULL;			
			return m_fields->fields[m_fields->passwordFieldIdx].wstr;
		}

		DWORD Credential::FindStatusId()
		{
			if(!m_fields) return 0;
			return m_fields->statusFieldIdx;
		}

		bool Credential::IsFieldDynamic(DWORD dwFieldID)
		{
			// Retrieve data for dynamic fields
			return (m_fields->fields[dwFieldID].fieldDataSource == SOURCE_DYNAMIC ||
					(m_fields->fields[dwFieldID].fieldDataSource == SOURCE_CALLBACK && m_fields->fields[dwFieldID].labelCallback != NULL) ||
					m_fields->fields[dwFieldID].fieldDataSource == SOURCE_STATUS);			
		}

		std::wstring Credential::GetTextForField(DWORD dwFieldID)
		{
			// Retrieve data for dynamic fields
			if( m_fields->fields[dwFieldID].fieldDataSource == SOURCE_DYNAMIC )
			{
				return pGina::Transactions::TileUi::GetDynamicLabel( m_fields->fields[dwFieldID].fieldDescriptor.pszLabel );				
			}
			else if(m_fields->fields[dwFieldID].fieldDataSource == SOURCE_CALLBACK && m_fields->fields[dwFieldID].labelCallback != NULL)
			{
				return m_fields->fields[dwFieldID].labelCallback(m_fields->fields[dwFieldID].fieldDescriptor.pszLabel, m_fields->fields[dwFieldID].fieldDescriptor.dwFieldID);				
			}
			else if(m_fields->fields[dwFieldID].fieldDataSource == SOURCE_STATUS)
			{
				return pGina::Service::StateHelper::GetStateText();
			}

			return L"";
		}

		void Credential::ServiceStateChanged(bool newState)
		{
			if(m_logonUiCallback)
			{
				std::wstring text = pGina::Service::StateHelper::GetStateText();
				m_logonUiCallback->SetFieldString(this, FindStatusId(), text.c_str());
			}
		}

		// Called just after the "submit" button is clicked and just before GetSerialization
		IFACEMETHODIMP Credential::Connect( IQueryContinueWithStatus *pqcws )
		{
			pDEBUG(L"Credential::Connect()");
			ProcessLoginAttempt(pqcws);
			return S_OK;
		}

		IFACEMETHODIMP Credential::Disconnect()
		{
			return E_NOTIMPL;
		}

		void Credential::ProcessLoginAttempt(IQueryContinueWithStatus *pqcws)
		{
			// Reset m_loginResult
			m_loginResult.Username(L"");
			m_loginResult.Password(L"");
			m_loginResult.Domain(L"");
			m_loginResult.Message(L"");
			m_loginResult.Result(false);
			m_logonCancelled = false;

			// Workout what our username, and password are.  Plugins are responsible for
			// parsing out domain\machine name if needed
			PWSTR username = FindUsernameValue();			
			PWSTR password = FindPasswordValue();
			PWSTR domain = NULL;
			
			pGina::Protocol::LoginRequestMessage::LoginReason reason = pGina::Protocol::LoginRequestMessage::Login;
			switch(m_usageScenario)
			{
			case CPUS_LOGON:
				break;
			case CPUS_UNLOCK_WORKSTATION:
				reason = pGina::Protocol::LoginRequestMessage::Unlock;
				break;
			case CPUS_CREDUI:
				reason = pGina::Protocol::LoginRequestMessage::CredUI;
				break;
			}

			pDEBUG(L"ProcessLoginAttempt: Processing login for %s", username);
			
			// Set the status message
			if( pqcws )
			{
				std::wstring message = pGina::Registry::GetString(L"LogonProgressMessage", L"Logging on...");

				// Replace occurences of %u with the username
				std::wstring unameCopy = username;
				std::wstring::size_type unameSize = unameCopy.size();
				for( std::wstring::size_type pos = 0; 
					(pos = message.find(L"%u", pos)) != std::wstring::npos;
					pos += unameSize )
				{
					message.replace(pos, unameSize, unameCopy);
				}

				pqcws->SetStatusMessage(message.c_str());
			}

			// Execute plugins
			m_loginResult = pGina::Transactions::User::ProcessLoginForUser(username, NULL, password, reason);
			
			if( pqcws )
			{
				if( m_loginResult.Result() )
				{
					pDEBUG(L"Plugins registered logon success");
					pqcws->SetStatusMessage(L"Logon successful");
				}
				else
				{
					pDEBUG(L"Plugins registered logon failure");
					pqcws->SetStatusMessage(L"Logon failed");
				}

				// Did the user click the "Cancel" button?
				if( pqcws->QueryContinue() != S_OK )
				{
					pDEBUG(L"User clicked cancel button during plugin processing");
					m_logonCancelled = true;
				}
			}			
		}
	}
}
