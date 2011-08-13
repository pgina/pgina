#pragma once

#include <windows.h>
#include <unknwn.h>

namespace pGina
{
	namespace COM
	{
		class CClassFactory : public IClassFactory
		{
		public:
			CClassFactory();
    
			// IUnknown
			IFACEMETHODIMP QueryInterface(__in REFIID riid, __deref_out void **ppv);    
			IFACEMETHODIMP_(ULONG) AddRef();    
			IFACEMETHODIMP_(ULONG) Release();
    
			// IClassFactory
			IFACEMETHODIMP CreateInstance(__in IUnknown* pUnkOuter, __in REFIID riid, __deref_out void **ppv);    
			IFACEMETHODIMP LockServer(__in BOOL bLock);    

		private:
			~CClassFactory();    
			long m_referenceCount;
		};
	}
}