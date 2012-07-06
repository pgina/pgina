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
		// This static class allows for the creation of a WLX_DISPATCH_VERSION_1_4 table
		//	that routes back to the Winlogon interface of choice.  Useful when you have to
		//  load a GINA and want to re-route some of it's calls into winlogon through you're
		//  own methods.
		class WinlogonRouter
		{
		public:
			static void Interface(WinlogonInterface *pIface) { s_interface = pIface; }
			static WinlogonInterface * Interface() { return s_interface; }

			static PWLX_DISPATCH_VERSION_1_4 DispatchTable() { return &s_table; }			

		private:
			// No instansiation!
			WinlogonRouter() {}

		private:
			// Static methods for dispatching back into our winlogon interface from the 
			//	table we provide to our chained GINA.
			static void WlxUseCtrlAltDel(HANDLE hWlx);
			static void WlxSetContextPointer(HANDLE hWlx, void *newContext);
			static void WlxSasNotify(HANDLE hWlx, DWORD sas);
			static bool WlxSetTimeout(HANDLE hWlx, DWORD newTimeout);
			static int  WlxAssignShellProtection(HANDLE hWlx, HANDLE token, HANDLE process, HANDLE thread);
			static int  WlxMessageBox(HANDLE hWlx, HWND owner, LPWSTR text, LPWSTR title, UINT style);
			static int  WlxDialogBox(HANDLE hWlx, HANDLE hInst, LPWSTR lpszTemplate, HWND hwndOwner, DLGPROC dlgprc);
			static int  WlxDialogBoxParam(HANDLE hWlx, HANDLE hInst, LPWSTR lpszTemplate, HWND hwndOwner, DLGPROC dlgprc, LPARAM dwInitParam);
			static int  WlxDialogBoxIndirect(HANDLE hWlx, HANDLE hInst, LPCDLGTEMPLATE hDialogTemplate, HWND hwndOwner, DLGPROC dlgprc);
			static int  WlxDialogBoxIndirectParam(HANDLE hWlx, HANDLE hInst, LPCDLGTEMPLATE hDialogTemplate, HWND hwndOwner, DLGPROC dlgprc, LPARAM dwInitParam);
			static int  WlxSwitchDesktopToUser(HANDLE hWlx);
			static int  WlxSwitchDesktopToWinlogon(HANDLE hWlx);
			static int  WlxChangePasswordNotify(HANDLE hWlx, PWLX_MPR_NOTIFY_INFO pMprInfo, DWORD dwChangeInfo);
			static bool WlxGetSourceDesktop(HANDLE hWlx, PWLX_DESKTOP *ppDesktop);
			static bool WlxSetReturnDesktop(HANDLE hWlx, PWLX_DESKTOP pDesktop);
			static bool WlxCreateUserDesktop(HANDLE hWlx, HANDLE hToken, DWORD Flags, PWSTR pszDesktopName, PWLX_DESKTOP *ppDesktop);
			static int  WlxChangePasswordNotifyEx(HANDLE hWlx, PWLX_MPR_NOTIFY_INFO pMprInfo, DWORD dwChangeInfo, PWSTR ProviderName, PVOID Reserved);
			static bool WlxCloseUserDesktop(HANDLE hWlx, PWLX_DESKTOP pDesktop, HANDLE hToken);
			static bool WlxSetOption(HANDLE hWlx, DWORD Option, ULONG_PTR Value, ULONG_PTR *OldValue);
			static bool WlxGetOption(HANDLE hWlx, DWORD Option, ULONG_PTR *Value);
			static void WlxWin31Migrate(HANDLE hWlx);
			static int  WlxQueryTerminalServicesData(HANDLE hWlx, PWLX_TERMINAL_SERVICES_DATA pTSData, WCHAR *UserName, WCHAR *Domain);

			// These don't have an hWlx param for getting back to us directly, we have to use our global :/			
			static bool WlxQueryClientCredentials(PWLX_CLIENT_CREDENTIALS_INFO_V1_0 pCred);
			static bool WlxQueryInetConnectorCredentials(PWLX_CLIENT_CREDENTIALS_INFO_V1_0 pCred);
			static bool WlxDisconnect();			
			static int  WlxQueryConsoleSwitchCredentials(PWLX_CONSOLESWITCH_CREDENTIALS_INFO_V1_0 pCred);
			static bool WlxQueryTsLogonCredentials(PWLX_CLIENT_CREDENTIALS_INFO_V2_0 pCred);

		private:			
			static WinlogonInterface * s_interface;
			static WLX_DISPATCH_VERSION_1_4 s_table;
		};
	}
}