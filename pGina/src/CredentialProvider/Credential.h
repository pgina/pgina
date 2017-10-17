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

#include <pGinaNativeLib.h>

#include "ClassFactory.h"
#include "TileUiTypes.h"

namespace pGina
{
	namespace CredProv
	{
		class Provider;
		class Credential : public IConnectableCredentialProviderCredential
		{
		public:
			// IUnknown
			IFACEMETHODIMP_(ULONG) AddRef();
			IFACEMETHODIMP_(ULONG) Release();
			IFACEMETHODIMP QueryInterface(__in REFIID riid, __deref_out void** ppv);

			// ICredentialProviderCredential
			IFACEMETHODIMP Advise(__in ICredentialProviderCredentialEvents* pcpce);
			IFACEMETHODIMP UnAdvise();
			IFACEMETHODIMP SetSelected(__out BOOL* pbAutoLogon);
			IFACEMETHODIMP SetDeselected();
			IFACEMETHODIMP GetFieldState(__in DWORD dwFieldID, __out CREDENTIAL_PROVIDER_FIELD_STATE* pcpfs, __out CREDENTIAL_PROVIDER_FIELD_INTERACTIVE_STATE* pcpfis);
			IFACEMETHODIMP GetStringValue(__in DWORD dwFieldID, __deref_out PWSTR* ppwsz);
			IFACEMETHODIMP GetBitmapValue(__in DWORD dwFieldID, __out HBITMAP* phbmp);
			IFACEMETHODIMP GetCheckboxValue(__in DWORD dwFieldID, __out BOOL* pbChecked, __deref_out PWSTR* ppwszLabel);
			IFACEMETHODIMP GetComboBoxValueCount(__in DWORD dwFieldID, __out DWORD* pcItems, __out_range(<,*pcItems) DWORD* pdwSelectedItem);
			IFACEMETHODIMP GetComboBoxValueAt(__in DWORD dwFieldID, __in DWORD dwItem, __deref_out PWSTR* ppwszItem);
			IFACEMETHODIMP GetSubmitButtonValue(__in DWORD dwFieldID, __out DWORD* pdwAdjacentTo);
			IFACEMETHODIMP SetStringValue(__in DWORD dwFieldID, __in PCWSTR pwz);
			IFACEMETHODIMP SetCheckboxValue(__in DWORD dwFieldID, __in BOOL bChecked);
			IFACEMETHODIMP SetComboBoxSelectedValue(__in DWORD dwFieldID, __in DWORD dwSelectedItem);
			IFACEMETHODIMP CommandLinkClicked(__in DWORD dwFieldID);
			IFACEMETHODIMP GetSerialization(__out CREDENTIAL_PROVIDER_GET_SERIALIZATION_RESPONSE* pcpgsr, __out CREDENTIAL_PROVIDER_CREDENTIAL_SERIALIZATION* pcpcs,
											__deref_out_opt PWSTR* ppwszOptionalStatusText, __out CREDENTIAL_PROVIDER_STATUS_ICON* pcpsiOptionalStatusIcon);
			IFACEMETHODIMP ReportResult(__in NTSTATUS ntsStatus, __in NTSTATUS ntsSubstatus, __deref_out_opt PWSTR* ppwszOptionalStatusText,
										__out CREDENTIAL_PROVIDER_STATUS_ICON* pcpsiOptionalStatusIcon);
			IFACEMETHODIMP Connect(IQueryContinueWithStatus *pqcws);
			IFACEMETHODIMP Disconnect();

			Credential();
			virtual ~Credential();

			void	Initialize(CREDENTIAL_PROVIDER_USAGE_SCENARIO cpus, UI_FIELDS const& fields,
							   DWORD usageFlags, const wchar_t *username, const wchar_t *password);

			virtual void	ServiceStateChanged(bool newState);
			
			typedef NTSTATUS(WINAPI* RtlGetVersionPtr)(PRTL_OSVERSIONINFOW);
			BOOL ISwin10();
		private:
			void	ClearZeroAndFreeAnyPasswordFields(bool updateUi);
			void	ClearZeroAndFreeAnyTextFields(bool updateUi);
			void	ClearZeroAndFreeFields(CREDENTIAL_PROVIDER_FIELD_TYPE type, bool updateUi);
			PWSTR   FindUsernameValue();
			PWSTR   FindPasswordValue();
			DWORD   FindStatusId();

			bool	IsFieldDynamic(DWORD dwFieldID);
			std::wstring GetTextForField(DWORD dwFieldID);

			static DWORD WINAPI Thread_dialog(LPVOID lpParameter);
			static void Thread_dialog_close(HANDLE thread);

		private:
			long m_referenceCount;
			CREDENTIAL_PROVIDER_USAGE_SCENARIO	m_usageScenario;
			ICredentialProviderCredentialEvents * m_logonUiCallback;
			UI_FIELDS *m_fields;
			DWORD	m_usageFlags;
			pGina::Transactions::User::LoginResult	m_loginResult;
			HWND									hdialog;
			HANDLE									hThread_dialog;
		};
	}
}