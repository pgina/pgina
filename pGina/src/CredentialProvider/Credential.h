#pragma once

#include <windows.h>
#include <credentialprovider.h>

#include "ClassFactory.h"
#include "TileUiTypes.h"

namespace pGina
{
	namespace CredProv
	{
		class Credential : public ICredentialProviderCredential
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

			Credential();
			virtual ~Credential();

			void	Initialize(CREDENTIAL_PROVIDER_USAGE_SCENARIO cpus, UI_FIELDS const& fields, 
							   DWORD usageFlags, const wchar_t *username, const wchar_t *password);

		private:
			void	ClearZeroAndFreeAnyPasswordFields(bool updateUi);
			void	ClearZeroAndFreeAnyTextFields(bool updateUi);
			void	ClearZeroAndFreeFields(CREDENTIAL_PROVIDER_FIELD_TYPE type, bool updateUi);
			PWSTR   FindUsernameValue();
			PWSTR   FindPasswordValue();

		private:
			long m_referenceCount;
			CREDENTIAL_PROVIDER_USAGE_SCENARIO	m_usageScenario;
			ICredentialProviderCredentialEvents * m_logonUiCallback;
			UI_FIELDS *m_fields;
			HBITMAP m_bitmap;
			DWORD	m_usageFlags;
		};
	}
}