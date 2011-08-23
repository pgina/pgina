#include "Credential.h"
#include "Dll.h"

#pragma warning(push)
#pragma warning(disable : 4995)
#include <shlwapi.h>
#pragma warning(pop)

#include <pGinaNativeLib.h>

#include "ClassFactory.h"
#include "TileUiTypes.h"
#include "TileUiLogon.h"
#include "TileUiUnlock.h"
#include "Macros.h"
#include "SerializationHelpers.h"
#include "ProviderGuid.h"

#include <wincred.h>

namespace pGina
{
	namespace CredProv
	{
		IFACEMETHODIMP Credential::QueryInterface(__in REFIID riid, __deref_out void **ppv)
		{
			// And more crazy ass v-table madness, yay COM again!
			static const QITAB qit[] =
			{
				QITABENT(Credential, ICredentialProviderCredential), 
				{0},
			};
			return QISearch(this, qit, riid, ppv);
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

			if(m_bitmap == NULL)
			{
				std::wstring tileImage = pGina::Registry::GetString(L"TileImage", L"");
				pDEBUG(L"Credential::GetBitmapValue: Loading image from: %s", tileImage.c_str());
				m_bitmap = (HBITMAP) LoadImageW((HINSTANCE) NULL, tileImage.c_str(), IMAGE_BITMAP, 0, 0, LR_LOADFROMFILE);			
				if(!m_bitmap)
					return HRESULT_FROM_WIN32(GetLastError());
			}

			*phbmp = m_bitmap;
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
			   m_fields->fields[dwFieldID].fieldDescriptor.cpft != CPFT_PASSWORD_TEXT)
				return E_INVALIDARG;

			PWSTR *currentValue = &m_fields->fields[dwFieldID].wstr;
			if(*currentValue)
			{
				CoTaskMemFree(*currentValue);
				*currentValue = NULL;
			}

			return SHStrDupW(pwz, currentValue);			
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
			// Workout what our username, and password are.  Plugins are responsible for parsing out domain\machine name if needed
			PWSTR username = FindUsernameValue();			
			PWSTR password = FindPasswordValue();
			PWSTR domain = NULL;
						
			pDEBUG(L"Credential::GetSerialization: Processing login for %s", username);
			if(!pGina::Transactions::User::ProcessLoginForUser(username, NULL, password))
			{
				pERROR(L"Credential::GetSerialization: Failed login");
				if(pGina::Transactions::User::AuthenticationMessage() != NULL)
				{
					SHStrDupW(pGina::Transactions::User::AuthenticationMessage(), ppwszOptionalStatusText);					
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

			username = _wcsdup(pGina::Transactions::User::AuthenticatedUsername());
			password = _wcsdup(pGina::Transactions::User::AuthenticatedPassword());
			domain = _wcsdup(pGina::Transactions::User::AuthenticatedDomain());			

			cleanup.Add(username, free);
			cleanup.Add(password, free);
			cleanup.Add(domain, free);

			PWSTR protectedPassword = NULL;			
			HRESULT result = Microsoft::Sample::ProtectIfNecessaryAndCopyPassword(password, m_usageScenario, &protectedPassword);			
			if(!SUCCEEDED(result))
				return result;

			cleanup.Add(static_cast<void *>(protectedPassword), (void (*)(void *))(CoTaskMemFree));

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
			m_bitmap(NULL),
			m_usageFlags(0)
		{
			AddDllReference();
		}
		
		Credential::~Credential()
		{
			ClearZeroAndFreeAnyTextFields(false);	// Free memory used to back text fields, no ui update
			if(m_bitmap) DeleteObject(m_bitmap);
			m_bitmap = NULL;
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
			for(DWORD x = 0; x < fields.fieldCount; x++)
			{
				m_fields->fields[x].fieldDescriptor = fields.fields[x].fieldDescriptor;
				m_fields->fields[x].fieldStatePair = fields.fields[x].fieldStatePair;
				m_fields->fields[x].wstr = NULL;
				if(fields.fields[x].wstr)
				{
					SHStrDup(fields.fields[x].wstr, &m_fields->fields[x].wstr);
				}								
			}			

			if(username != NULL)
			{
				PWSTR dest = FindUsernameValue();
				SHStrDupW(username, &(dest));
			}

			if(password != NULL)
			{
				PWSTR dest = FindPasswordValue();
				SHStrDupW(password, &(dest));
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
						size_t len = lstrlen(m_fields->fields[x].wstr);
						SecureZeroMemory(m_fields->fields[x].wstr, len * sizeof(wchar_t));
						CoTaskMemFree(m_fields->fields[x].wstr);						

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
	}
}
