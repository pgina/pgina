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
		// This WinlogonInterface is useful when you want to intercept a subset of 
		//	the winlogon interface calls made by a stubbed/hooked gina.  Any you
		//  don't implement just get called straight through to the 'actual' iface
		//  provided.  Just inherit from this class, be sure to call the protected
		//	constructor with the actual iface, then implement any of the WinlogonInterface
		//  methods you care to be involved in.
		class WinlogonProxy : public WinlogonInterface
		{
		public:
			virtual void WlxUseCtrlAltDel() { m_iface->WlxUseCtrlAltDel(); }
			virtual void WlxSetContextPointer(void *newContext) { m_iface->WlxSetContextPointer(newContext); }
			virtual void WlxSasNotify(DWORD sas) { m_iface->WlxSasNotify(sas); }
			virtual bool WlxSetTimeout(DWORD newTimeout) { return m_iface->WlxSetTimeout(newTimeout); }
			virtual int  WlxAssignShellProtection(HANDLE token, HANDLE process, HANDLE thread) { return m_iface->WlxAssignShellProtection(token, process, thread); }
			virtual int  WlxMessageBox(HWND owner, LPWSTR text, LPWSTR title, UINT style) { return m_iface->WlxMessageBox(owner, text, title, style); }
			virtual int  WlxDialogBox(HANDLE hInst, LPWSTR lpszTemplate, HWND hwndOwner, DLGPROC dlgprc) { return m_iface->WlxDialogBox(hInst, lpszTemplate, hwndOwner, dlgprc); }
			virtual int  WlxDialogBoxParam(HANDLE hInst, LPWSTR lpszTemplate, HWND hwndOwner, DLGPROC dlgprc, LPARAM dwInitParam) { return m_iface->WlxDialogBoxParam(hInst, lpszTemplate, hwndOwner, dlgprc, dwInitParam); }
			virtual int  WlxDialogBoxIndirect(HANDLE hInst, LPCDLGTEMPLATE hDialogTemplate, HWND hwndOwner, DLGPROC dlgprc) { return m_iface->WlxDialogBoxIndirect(hInst, hDialogTemplate, hwndOwner, dlgprc); }
			virtual int  WlxDialogBoxIndirectParam(HANDLE hInst, LPCDLGTEMPLATE hDialogTemplate, HWND hwndOwner, DLGPROC dlgprc, LPARAM dwInitParam) { return m_iface->WlxDialogBoxIndirectParam(hInst, hDialogTemplate, hwndOwner, dlgprc, dwInitParam); }
			virtual int  WlxSwitchDesktopToUser() { return m_iface->WlxSwitchDesktopToUser(); }
			virtual int  WlxSwitchDesktopToWinlogon() { return m_iface->WlxSwitchDesktopToWinlogon(); }
			virtual int  WlxChangePasswordNotify(PWLX_MPR_NOTIFY_INFO pMprInfo, DWORD dwChangeInfo) { return m_iface->WlxChangePasswordNotify(pMprInfo, dwChangeInfo); }
			virtual bool WlxGetSourceDesktop(PWLX_DESKTOP *ppDesktop) { return m_iface->WlxGetSourceDesktop(ppDesktop); }
			virtual bool WlxSetReturnDesktop(PWLX_DESKTOP pDesktop) { return m_iface->WlxSetReturnDesktop(pDesktop); }
			virtual bool WlxCreateUserDesktop(HANDLE hToken, DWORD Flags, PWSTR pszDesktopName, PWLX_DESKTOP *ppDesktop) { return m_iface->WlxCreateUserDesktop(hToken, Flags, pszDesktopName, ppDesktop); }
			virtual int  WlxChangePasswordNotifyEx(PWLX_MPR_NOTIFY_INFO pMprInfo, DWORD dwChangeInfo, PWSTR ProviderName, PVOID Reserved) { return m_iface->WlxChangePasswordNotifyEx(pMprInfo, dwChangeInfo, ProviderName, Reserved); }
			virtual bool WlxCloseUserDesktop(PWLX_DESKTOP pDesktop, HANDLE hToken) { return m_iface->WlxCloseUserDesktop(pDesktop, hToken); }
			virtual bool WlxSetOption(DWORD Option, ULONG_PTR Value, ULONG_PTR *OldValue) { return m_iface->WlxSetOption(Option, Value, OldValue); }
			virtual bool WlxGetOption(DWORD Option, ULONG_PTR *Value) { return m_iface->WlxGetOption(Option, Value); }
			virtual void WlxWin31Migrate() { m_iface->WlxWin31Migrate(); }
			virtual bool WlxQueryClientCredentials(PWLX_CLIENT_CREDENTIALS_INFO_V1_0 pCred) { return m_iface->WlxQueryClientCredentials(pCred); }
			virtual bool WlxQueryInetConnectorCredentials(PWLX_CLIENT_CREDENTIALS_INFO_V1_0 pCred) { return m_iface->WlxQueryInetConnectorCredentials(pCred); }
			virtual bool WlxDisconnect() { return m_iface->WlxDisconnect(); }
			virtual int  WlxQueryTerminalServicesData(PWLX_TERMINAL_SERVICES_DATA pTSData, WCHAR *UserName, WCHAR *Domain) { return m_iface->WlxQueryTerminalServicesData(pTSData, UserName, Domain); }
			virtual int  WlxQueryConsoleSwitchCredentials(PWLX_CONSOLESWITCH_CREDENTIALS_INFO_V1_0 pCred) { return m_iface->WlxQueryConsoleSwitchCredentials(pCred); }
			virtual bool WlxQueryTsLogonCredentials(PWLX_CLIENT_CREDENTIALS_INFO_V2_0 pCred) { return m_iface->WlxQueryTsLogonCredentials(pCred); }

		protected:
			WinlogonProxy(WinlogonInterface *iface) :
				 m_iface(iface) {}

		private:
			WinlogonInterface * m_iface;
		};
	}
}