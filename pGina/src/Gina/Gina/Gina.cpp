/*
	Copyright (c) 2011, pGina Team
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
#include "Gina.h"

#include "WinlogonDebug.h"
#include "WinlogonReal.h"
#include "GinaChain.h"	// Currently this is the only GINA type supported

namespace pGina
{
	namespace GINA
	{
		static WinlogonInterface * WinlogonInterfaceFactory(HANDLE hWlx, void * pFuncs)
		{
			// If hWlx == NULL || pFuncs is null, then we
			//	create a winlogon interface that is fake for testing purposes.
			if(hWlx == NULL || pFuncs == NULL)			
				return new DebugWinlogonInterface();			

			return new RealWinlogonInterface(hWlx, pFuncs);
		}

		/*static*/
		bool Gina::InitializeFactory(HANDLE hWlx, void * pWinlogonFunctions, Gina **context)
		{
			// Create a winlogon interface class, and a Gina class, and pair them
			//  the result becomes our context, which Winlogon will give us on all
			//  calls.
			*context = new GinaChain(WinlogonInterfaceFactory(hWlx, pWinlogonFunctions));			
			return true;
		}

		Gina::Gina(WinlogonInterface *pWinLogonIface) : 
			m_winlogon(pWinLogonIface)
		{
		}

		Gina::~Gina()
		{
			if(m_winlogon)
			{
				delete m_winlogon;
				m_winlogon = NULL;
			}
		}
	}
}