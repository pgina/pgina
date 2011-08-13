#include "Provider.h"
#include "Dll.h"

#pragma warning(push)
#pragma warning(disable : 4995)
#include <shlwapi.h>
#pragma warning(pop)

namespace pGina
{
	namespace Provider
	{
		IFACEMETHODIMP CredProv::QueryInterface(__in REFIID riid, __deref_out void **ppv)
		{
			// TBD: This is magic I do not understand, but it works, based on the CredentialProvider samples,
			//	stupid COM :/
			static const QITAB qit[] =
			{
				QITABENT(CredProv, ICredentialProvider), 
				{0},
			};
			return QISearch(this, qit, riid, ppv);
		}

		IFACEMETHODIMP_(ULONG) CredProv::AddRef()
		{
	        return InterlockedIncrement(&m_referenceCount);
		}

		IFACEMETHODIMP_(ULONG) CredProv::Release()
		{
			LONG count = InterlockedDecrement(&m_referenceCount);
			if (!count)
				delete this;
			return count;
		}

		CredProv::CredProv()
		{
			AddDllReference();
		}

		CredProv::~CredProv()
		{
			ReleaseDllReference();
		}

		IFACEMETHODIMP CredProv::SetUsageScenario(__in CREDENTIAL_PROVIDER_USAGE_SCENARIO cpus, __in DWORD dwFlags)
		{
			return S_FALSE;
		}

		IFACEMETHODIMP CredProv::SetSerialization(__in const CREDENTIAL_PROVIDER_CREDENTIAL_SERIALIZATION* pcpcs)
		{
			return S_FALSE;
		}

		IFACEMETHODIMP CredProv::Advise(__in ICredentialProviderEvents* pcpe, __in UINT_PTR upAdviseContext)
		{
			return S_FALSE;
		}

		IFACEMETHODIMP CredProv::UnAdvise()
		{
			return S_FALSE;
		}

		IFACEMETHODIMP CredProv::GetFieldDescriptorCount(__out DWORD* pdwCount)
		{
			return S_FALSE;
		}

		IFACEMETHODIMP CredProv::GetFieldDescriptorAt(__in DWORD dwIndex,  __deref_out CREDENTIAL_PROVIDER_FIELD_DESCRIPTOR** ppcpfd)
		{
			return S_FALSE;
		}

		IFACEMETHODIMP CredProv::GetCredentialCount(__out DWORD* pdwCount, __out_range(<,*pdwCount) DWORD* pdwDefault, __out BOOL* pbAutoLogonWithDefault)
		{
			return S_FALSE;
		}

		IFACEMETHODIMP CredProv::GetCredentialAt(__in DWORD dwIndex, __deref_out ICredentialProviderCredential** ppcpc)
		{
			return S_FALSE;
		}
	}
}