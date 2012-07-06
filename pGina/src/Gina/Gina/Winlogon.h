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

namespace pGina
{
	namespace GINA
	{
		// Base class for wrapping up interaction with winlogon via it's dispatch table
		class WinlogonInterface
		{
		public:
			static DWORD Version() { return s_winlogonVersion; }
			static void  Version(DWORD v) { s_winlogonVersion = v; }
		
			// Winlogon functions
			virtual void WlxUseCtrlAltDel() = 0;
			virtual void WlxSetContextPointer(void *newContext) = 0;
			virtual void WlxSasNotify(DWORD sas) = 0;
			virtual bool WlxSetTimeout(DWORD newTimeout) = 0;
			virtual int  WlxAssignShellProtection(HANDLE token, HANDLE process, HANDLE thread) = 0;
			virtual int  WlxMessageBox(HWND owner, LPWSTR text, LPWSTR title, UINT style) = 0;
			virtual int  WlxDialogBox(HANDLE hInst, LPWSTR lpszTemplate, HWND hwndOwner, DLGPROC dlgprc) = 0;
			virtual int  WlxDialogBoxParam(HANDLE hInst, LPWSTR lpszTemplate, HWND hwndOwner, DLGPROC dlgprc, LPARAM dwInitParam) = 0;
			virtual int  WlxDialogBoxIndirect(HANDLE hInst, LPCDLGTEMPLATE hDialogTemplate, HWND hwndOwner, DLGPROC dlgprc) = 0;
			virtual int  WlxDialogBoxIndirectParam(HANDLE hInst, LPCDLGTEMPLATE hDialogTemplate, HWND hwndOwner, DLGPROC dlgprc, LPARAM dwInitParam) = 0;
			virtual int  WlxSwitchDesktopToUser() = 0;
			virtual int  WlxSwitchDesktopToWinlogon() = 0;
			virtual int  WlxChangePasswordNotify(PWLX_MPR_NOTIFY_INFO pMprInfo, DWORD dwChangeInfo) = 0;
			virtual bool WlxGetSourceDesktop(PWLX_DESKTOP *ppDesktop) = 0;
			virtual bool WlxSetReturnDesktop(PWLX_DESKTOP pDesktop) = 0;
			virtual bool WlxCreateUserDesktop(HANDLE hToken, DWORD Flags, PWSTR pszDesktopName, PWLX_DESKTOP *ppDesktop) = 0;
			virtual int  WlxChangePasswordNotifyEx(PWLX_MPR_NOTIFY_INFO pMprInfo, DWORD dwChangeInfo, PWSTR ProviderName, PVOID Reserved) = 0;
			virtual bool WlxCloseUserDesktop(PWLX_DESKTOP pDesktop, HANDLE hToken) = 0;
			virtual bool WlxSetOption(DWORD Option, ULONG_PTR Value, ULONG_PTR *OldValue) = 0;
			virtual bool WlxGetOption(DWORD Option, ULONG_PTR *Value) = 0;
			virtual void WlxWin31Migrate() = 0;
			virtual bool WlxQueryClientCredentials(PWLX_CLIENT_CREDENTIALS_INFO_V1_0 pCred) = 0;
			virtual bool WlxQueryInetConnectorCredentials(PWLX_CLIENT_CREDENTIALS_INFO_V1_0 pCred) = 0;
			virtual bool WlxDisconnect() = 0;
			virtual int  WlxQueryTerminalServicesData(PWLX_TERMINAL_SERVICES_DATA pTSData, WCHAR *UserName, WCHAR *Domain) = 0;
			virtual int  WlxQueryConsoleSwitchCredentials(PWLX_CONSOLESWITCH_CREDENTIALS_INFO_V1_0 pCred) = 0;
			virtual bool WlxQueryTsLogonCredentials(PWLX_CLIENT_CREDENTIALS_INFO_V2_0 pCred) = 0;

		protected:
			// No public constructor, only a Real or Test impl should be created
			WinlogonInterface() {}			

		private:
			static DWORD s_winlogonVersion;
		};
	}
}