#include <assert.h>

#include "Provider.h"
#include "Dll.h"

#pragma warning(push)
#pragma warning(disable : 4995)
#include <shlwapi.h>
#pragma warning(pop)

#include "Macros.h"
#include "TileUiLogonUnlock.h"

namespace pGina
{
	namespace CredProv
	{
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
			m_credential(NULL)
		{		
			AddDllReference();			
		}

		Provider::~Provider()
		{
			UnAdvise();
			ReleaseDllReference();

			if(m_credential)
			{
				m_credential->Release();
				m_credential = NULL;
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
				m_usageScenario = cpus;
				return S_OK;
			
			case CPUS_CHANGE_PASSWORD:
			case CPUS_CREDUI:
				return E_NOTIMPL;	// Todo: Support these

			case CPUS_PLAP:
			case CPUS_INVALID:
				return E_NOTIMPL;

			default:
				return E_INVALIDARG;	// Say wha?
			}			    
		}

		IFACEMETHODIMP Provider::SetSerialization(__in const CREDENTIAL_PROVIDER_CREDENTIAL_SERIALIZATION* pcpcs)
		{
			return S_OK;	// Todo: Implement me!
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
			case CPUS_UNLOCK_WORKSTATION:
				*pdwCount = LUIFI_NUM_FIELDS;
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
			case CPUS_UNLOCK_WORKSTATION:
				return GetFieldDescriptorForUi(s_logonFields, dwIndex, ppcpfd);
			default:
				return E_INVALIDARG;
			}

			// Should never reach this
			assert(0);
			return S_FALSE;
		}

		IFACEMETHODIMP Provider::GetCredentialCount(__out DWORD* pdwCount, __out_range(<,*pdwCount) DWORD* pdwDefault, __out BOOL* pbAutoLogonWithDefault)
		{
			// Currently we only support a single tile, no default tile, and no autologin scenario			
			*pdwCount = 1;
			*pdwDefault = CREDENTIAL_PROVIDER_NO_DEFAULT;
			*pbAutoLogonWithDefault = FALSE;
			return S_OK;
		}

		IFACEMETHODIMP Provider::GetCredentialAt(__in DWORD dwIndex, __deref_out ICredentialProviderCredential** ppcpc)
		{
			// Currently we have just the one, we lazy init it here when first requested
			if(!m_credential)
			{
				m_credential = new Credential();
				m_credential->Initialize(m_usageScenario);
			}

			// Did we fail to create it? OOM
			if(!m_credential) return E_OUTOFMEMORY;

			// Better be index 0 (we only have 1 currently)
			if(dwIndex != 0 || !ppcpc)
				return E_INVALIDARG;

			// Alright... QueryIface for ICredentialProviderCredential
			return m_credential->QueryInterface(IID_ICredentialProviderCredential, reinterpret_cast<void **>(ppcpc));			 
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
	}
}