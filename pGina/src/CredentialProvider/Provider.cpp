#include "Provider.h"
#include "Dll.h"

#pragma warning(push)
#pragma warning(disable : 4995)
#include <shlwapi.h>
#pragma warning(pop)

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
			m_referenceCount(1)
		{		
			AddDllReference();
		}

		Provider::~Provider()
		{
			ReleaseDllReference();
		}

		IFACEMETHODIMP Provider::SetUsageScenario(__in CREDENTIAL_PROVIDER_USAGE_SCENARIO cpus, __in DWORD dwFlags)
		{
			return S_FALSE;
		}

		IFACEMETHODIMP Provider::SetSerialization(__in const CREDENTIAL_PROVIDER_CREDENTIAL_SERIALIZATION* pcpcs)
		{
			return S_FALSE;
		}

		IFACEMETHODIMP Provider::Advise(__in ICredentialProviderEvents* pcpe, __in UINT_PTR upAdviseContext)
		{
			return S_FALSE;
		}

		IFACEMETHODIMP Provider::UnAdvise()
		{
			return S_FALSE;
		}

		IFACEMETHODIMP Provider::GetFieldDescriptorCount(__out DWORD* pdwCount)
		{
			return S_FALSE;
		}

		IFACEMETHODIMP Provider::GetFieldDescriptorAt(__in DWORD dwIndex,  __deref_out CREDENTIAL_PROVIDER_FIELD_DESCRIPTOR** ppcpfd)
		{
			return S_FALSE;
		}

		IFACEMETHODIMP Provider::GetCredentialCount(__out DWORD* pdwCount, __out_range(<,*pdwCount) DWORD* pdwDefault, __out BOOL* pbAutoLogonWithDefault)
		{
			return S_FALSE;
		}

		IFACEMETHODIMP Provider::GetCredentialAt(__in DWORD dwIndex, __deref_out ICredentialProviderCredential** ppcpc)
		{
			return S_FALSE;
		}
	}
}