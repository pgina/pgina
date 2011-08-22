#pragma once

#include <windows.h>
#include <credentialprovider.h>

#include "ClassFactory.h"
#include "TileUiTypes.h"
#include "Credential.h"

#include <ntsecapi.h>

namespace pGina
{
	namespace CredProv
	{
		class Provider : public ICredentialProvider
		{
		public:
			// IUnknown
			IFACEMETHODIMP_(ULONG) AddRef();    
			IFACEMETHODIMP_(ULONG) Release();    
			IFACEMETHODIMP QueryInterface(__in REFIID riid, __deref_out void** ppv);
			
			// ICredentialProvider
			IFACEMETHODIMP SetUsageScenario(__in CREDENTIAL_PROVIDER_USAGE_SCENARIO cpus, __in DWORD dwFlags);
			IFACEMETHODIMP SetSerialization(__in const CREDENTIAL_PROVIDER_CREDENTIAL_SERIALIZATION* pcpcs);
			IFACEMETHODIMP Advise(__in ICredentialProviderEvents* pcpe, __in UINT_PTR upAdviseContext);
			IFACEMETHODIMP UnAdvise();
			IFACEMETHODIMP GetFieldDescriptorCount(__out DWORD* pdwCount);
			IFACEMETHODIMP GetFieldDescriptorAt(__in DWORD dwIndex,  __deref_out CREDENTIAL_PROVIDER_FIELD_DESCRIPTOR** ppcpfd);
			IFACEMETHODIMP GetCredentialCount(__out DWORD* pdwCount, __out_range(<,*pdwCount) DWORD* pdwDefault, __out BOOL* pbAutoLogonWithDefault);
			IFACEMETHODIMP GetCredentialAt(__in DWORD dwIndex, __deref_out ICredentialProviderCredential** ppcpc);

			friend class pGina::COM::CClassFactory;
		protected:
			Provider();
			__override ~Provider();

			IFACEMETHODIMP GetFieldDescriptorForUi(UI_FIELDS const& fields, DWORD index, CREDENTIAL_PROVIDER_FIELD_DESCRIPTOR **ppcpfd);
			bool SerializedCredsAppearComplete();
			void GetSerializedCredentials(PWSTR *username, PWSTR *password, PWSTR *domain);			

		private:
			long m_referenceCount;
			CREDENTIAL_PROVIDER_USAGE_SCENARIO	m_usageScenario;
			ICredentialProviderEvents *			m_logonUiCallbackEvents;
			UINT_PTR							m_logonUiCallbackContext;
			Credential *						m_credential;
			DWORD								m_usageFlags;
			KERB_INTERACTIVE_UNLOCK_LOGON *		m_setSerialization;
		};
	}
}
