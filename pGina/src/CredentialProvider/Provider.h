/*
	Copyright (c) 2016, pGina Team
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

			virtual void	ServiceStateChanged(bool newState);
			CREDENTIAL_PROVIDER_USAGE_SCENARIO	m_usageScenario;
			static bool m_redraw;
		protected:
			Provider();
			__override ~Provider();

			IFACEMETHODIMP GetFieldDescriptorForUi(UI_FIELDS const& fields, DWORD index, CREDENTIAL_PROVIDER_FIELD_DESCRIPTOR **ppcpfd);
			bool SerializedUserNameAvailable();
			bool SerializedPasswordAvailable();
			bool SerializedDomainNameAvailable();
			void GetSerializedCredentials(PWSTR *username, PWSTR *password, PWSTR *domain);

		private:
			long m_referenceCount;
			ICredentialProviderEvents *			m_logonUiCallbackEvents;
			UINT_PTR							m_logonUiCallbackContext;
			Credential *						m_credential;
			DWORD								m_usageFlags;
			KERB_INTERACTIVE_UNLOCK_LOGON *		m_setSerialization;
		};
	}
}
