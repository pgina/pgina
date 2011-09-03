#include "Gina.h"

namespace pGina
{
	namespace GINA
	{
		static WinlogonInterface * WinlogonInterfaceFactory(HANDLE hWlx, void * pFuncs)
		{
#if _DEBUG
			// In debug only, if hWlx == NULL || pFuncs is null, then we
			//	create a winlogon interface that is fake for testing purposes.
			if(hWlx == NULL || pFuncs == NULL)			
				return new DebugWinlogonInterface();			
#endif
			return new RealWinlogonInterface(hWlx, pFuncs);
		}

		/*static*/
		bool Gina::Initialize(HANDLE hWlx, void * pWinlogonFunctions, Gina **context)
		{
			// Create a winlogon interface class, and a Gina class, and pair them
			//  the result becomes our context, which Winlogon will give us on all
			//  calls.
			*context = new Gina(WinlogonInterfaceFactory(hWlx, pWinlogonFunctions));			
			return true;
		}

		Gina::Gina(WinlogonInterface *pWinLogonIface) : 
			m_winlogon(pWinLogonIface)
		{
		}
	}
}