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
#pragma once

#include <Windows.h>
#include <WinWlx.h>

#include "Winlogon.h"

namespace pGina
{
	namespace GINA
	{
		// Debugging (no winlogon) interface
		class DebugWinlogonInterface : public WinlogonInterface
		{
		public:
			DebugWinlogonInterface() {}

			// Winlogon functions
			virtual void WlxUseCtrlAltDel();
			virtual void WlxSetContextPointer(void *newContext);
			virtual void WlxSasNotify(DWORD sas);
			virtual bool WlxSetTimeout(DWORD newTimeout);
			virtual int  WlxAssignShellProtection(HANDLE token, HANDLE process, HANDLE thread);
			virtual int  WlxMessageBox(HWND owner, LPWSTR text, LPWSTR title, UINT style);
			virtual int  WlxDialogBox(HANDLE hInst, LPWSTR lpszTemplate, HWND hwndOwner, DLGPROC dlgprc);
			virtual int  WlxDialogBoxParam(HANDLE hInst, LPWSTR lpszTemplate, HWND hwndOwner, DLGPROC dlgprc, LPARAM dwInitParam);
			virtual int  WlxDialogBoxIndirect(HANDLE hInst, LPCDLGTEMPLATE hDialogTemplate, HWND hwndOwner, DLGPROC dlgprc);
			virtual int  WlxDialogBoxIndirectParam(HANDLE hInst, LPCDLGTEMPLATE hDialogTemplate, HWND hwndOwner, DLGPROC dlgprc, LPARAM dwInitParam);
			virtual int  WlxSwitchDesktopToUser();
			virtual int  WlxSwitchDesktopToWinlogon();
			virtual int  WlxChangePasswordNotify(PWLX_MPR_NOTIFY_INFO pMprInfo, DWORD dwChangeInfo);
			virtual bool WlxGetSourceDesktop(PWLX_DESKTOP *ppDesktop);
			virtual bool WlxSetReturnDesktop(PWLX_DESKTOP pDesktop);
			virtual bool WlxCreateUserDesktop(HANDLE hToken, DWORD Flags, PWSTR pszDesktopName, PWLX_DESKTOP *ppDesktop);
			virtual int  WlxChangePasswordNotifyEx(PWLX_MPR_NOTIFY_INFO pMprInfo, DWORD dwChangeInfo, PWSTR ProviderName, PVOID Reserved);
			virtual bool WlxCloseUserDesktop(PWLX_DESKTOP pDesktop, HANDLE hToken);
			virtual bool WlxSetOption(DWORD Option, ULONG_PTR Value, ULONG_PTR *OldValue);
			virtual bool WlxGetOption(DWORD Option, ULONG_PTR *Value);
			virtual void WlxWin31Migrate();
			virtual bool WlxQueryClientCredentials(PWLX_CLIENT_CREDENTIALS_INFO_V1_0 pCred);
			virtual bool WlxQueryInetConnectorCredentials(PWLX_CLIENT_CREDENTIALS_INFO_V1_0 pCred);
			virtual bool WlxDisconnect();
			virtual int  WlxQueryTerminalServicesData(PWLX_TERMINAL_SERVICES_DATA pTSData, WCHAR *UserName, WCHAR *Domain);
			virtual int  WlxQueryConsoleSwitchCredentials(PWLX_CONSOLESWITCH_CREDENTIALS_INFO_V1_0 pCred);
			virtual bool WlxQueryTsLogonCredentials(PWLX_CLIENT_CREDENTIALS_INFO_V2_0 pCred);

		private:			
		};
	}
}