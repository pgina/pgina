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

#include "WinlogonRouter.h"

namespace pGina
{
	namespace GINA
	{
		WinlogonInterface * WinlogonRouter::s_interface = NULL;
		WLX_DISPATCH_VERSION_1_4 WinlogonRouter::s_table = 
		{
			(PWLX_USE_CTRL_ALT_DEL)					&WinlogonRouter::WlxUseCtrlAltDel,
			(PWLX_SET_CONTEXT_POINTER)				&WinlogonRouter::WlxSetContextPointer,
			(PWLX_SAS_NOTIFY)						&WinlogonRouter::WlxSasNotify,
			(PWLX_SET_TIMEOUT)						&WinlogonRouter::WlxSetTimeout,
			(PWLX_ASSIGN_SHELL_PROTECTION)			&WinlogonRouter::WlxAssignShellProtection,
			(PWLX_MESSAGE_BOX)						&WinlogonRouter::WlxMessageBox,
			(PWLX_DIALOG_BOX)						&WinlogonRouter::WlxDialogBox,
			(PWLX_DIALOG_BOX_PARAM)					&WinlogonRouter::WlxDialogBoxParam,
			(PWLX_DIALOG_BOX_INDIRECT)				&WinlogonRouter::WlxDialogBoxIndirect,
			(PWLX_DIALOG_BOX_INDIRECT_PARAM)		&WinlogonRouter::WlxDialogBoxIndirectParam,
			(PWLX_SWITCH_DESKTOP_TO_USER)			&WinlogonRouter::WlxSwitchDesktopToUser,
			(PWLX_SWITCH_DESKTOP_TO_WINLOGON)		&WinlogonRouter::WlxSwitchDesktopToWinlogon,
			(PWLX_CHANGE_PASSWORD_NOTIFY)			&WinlogonRouter::WlxChangePasswordNotify,
			(PWLX_GET_SOURCE_DESKTOP)				&WinlogonRouter::WlxGetSourceDesktop,
			(PWLX_SET_RETURN_DESKTOP)				&WinlogonRouter::WlxSetReturnDesktop,
			(PWLX_CREATE_USER_DESKTOP)				&WinlogonRouter::WlxCreateUserDesktop,
			(PWLX_CHANGE_PASSWORD_NOTIFY_EX)		&WinlogonRouter::WlxChangePasswordNotifyEx,
			(PWLX_CLOSE_USER_DESKTOP)				&WinlogonRouter::WlxCloseUserDesktop,
			(PWLX_SET_OPTION)						&WinlogonRouter::WlxSetOption,
			(PWLX_GET_OPTION)						&WinlogonRouter::WlxGetOption,
			(PWLX_WIN31_MIGRATE)					&WinlogonRouter::WlxWin31Migrate,
			(PWLX_QUERY_CLIENT_CREDENTIALS)			&WinlogonRouter::WlxQueryClientCredentials,
			(PWLX_QUERY_IC_CREDENTIALS)				&WinlogonRouter::WlxQueryInetConnectorCredentials,
			(PWLX_DISCONNECT)						&WinlogonRouter::WlxDisconnect,
			(PWLX_QUERY_TERMINAL_SERVICES_DATA)		&WinlogonRouter::WlxQueryTerminalServicesData,
			(PWLX_QUERY_CONSOLESWITCH_CREDENTIALS)	&WinlogonRouter::WlxQueryConsoleSwitchCredentials,
			(PWLX_QUERY_TS_LOGON_CREDENTIALS)		&WinlogonRouter::WlxQueryTsLogonCredentials,
		};

		/*static*/ 
		void WinlogonRouter::WlxUseCtrlAltDel(HANDLE hWlx)
		{
			// We could require that the user who'se passed our table to someone 
			//	also pass a ptr to s_interface as hWlx.  Instead, we ignore hWlx
			//	and always use s_interface.  Simpler that way...
			if(s_interface)
				s_interface->WlxUseCtrlAltDel();
		}

		/*static*/ 
		void WinlogonRouter::WlxSetContextPointer(HANDLE hWlx, void *newContext)
		{
			if(s_interface)
				s_interface->WlxSetContextPointer(newContext);
		}

		/*static*/ 
		void WinlogonRouter::WlxSasNotify(HANDLE hWlx, DWORD sas)
		{
			if(s_interface)
				s_interface->WlxSasNotify(sas);
		}

		/*static*/ 
		bool WinlogonRouter::WlxSetTimeout(HANDLE hWlx, DWORD newTimeout)
		{
			if(!s_interface) return false;
			return s_interface->WlxSetTimeout(newTimeout);
		}

		/*static*/ 
		int  WinlogonRouter::WlxAssignShellProtection(HANDLE hWlx, HANDLE token, HANDLE process, HANDLE thread)
		{
			if(!s_interface) return -1;
			return s_interface->WlxAssignShellProtection(token, process, thread);
		}

		/*static*/ 
		int  WinlogonRouter::WlxMessageBox(HANDLE hWlx, HWND owner, LPWSTR text, LPWSTR title, UINT style)
		{
			if(!s_interface) return -1;
			return s_interface->WlxMessageBox(owner, text, title, style);
		}

		/*static*/ 
		int  WinlogonRouter::WlxDialogBox(HANDLE hWlx, HANDLE hInst, LPWSTR lpszTemplate, HWND hwndOwner, DLGPROC dlgprc)
		{
			if(!s_interface) return -1;
			return s_interface->WlxDialogBox(hInst, lpszTemplate, hwndOwner, dlgprc);
		}

		/*static*/ 
		int  WinlogonRouter::WlxDialogBoxParam(HANDLE hWlx, HANDLE hInst, LPWSTR lpszTemplate, HWND hwndOwner, DLGPROC dlgprc, LPARAM dwInitParam)
		{
			if(!s_interface) return -1;
			return s_interface->WlxDialogBoxParam(hInst, lpszTemplate, hwndOwner, dlgprc, dwInitParam);
		}

		/*static*/ 
		int  WinlogonRouter::WlxDialogBoxIndirect(HANDLE hWlx, HANDLE hInst, LPCDLGTEMPLATE hDialogTemplate, HWND hwndOwner, DLGPROC dlgprc)
		{
			if(!s_interface) return -1;
			return s_interface->WlxDialogBoxIndirect(hInst, hDialogTemplate, hwndOwner, dlgprc);
		}
		
		/*static*/ 
		int  WinlogonRouter::WlxDialogBoxIndirectParam(HANDLE hWlx, HANDLE hInst, LPCDLGTEMPLATE hDialogTemplate, HWND hwndOwner, DLGPROC dlgprc, LPARAM dwInitParam)
		{
			if(!s_interface) return -1;
			return s_interface->WlxDialogBoxIndirectParam(hInst, hDialogTemplate, hwndOwner, dlgprc, dwInitParam);
		}

		/*static*/ 
		int  WinlogonRouter::WlxSwitchDesktopToUser(HANDLE hWlx)
		{
			if(!s_interface) return -1;
			return s_interface->WlxSwitchDesktopToUser();
		}

		/*static*/ 
		int  WinlogonRouter::WlxSwitchDesktopToWinlogon(HANDLE hWlx)
		{
			if(!s_interface) return -1;
			return s_interface->WlxSwitchDesktopToWinlogon();
		}
		
		/*static*/ 
		int  WinlogonRouter::WlxChangePasswordNotify(HANDLE hWlx, PWLX_MPR_NOTIFY_INFO pMprInfo, DWORD dwChangeInfo)
		{
			if(!s_interface) return -1;
			return s_interface->WlxChangePasswordNotify(pMprInfo, dwChangeInfo);
		}

		/*static*/ 
		bool WinlogonRouter::WlxGetSourceDesktop(HANDLE hWlx, PWLX_DESKTOP *ppDesktop)
		{
			if(!s_interface) return false;
			return s_interface->WlxGetSourceDesktop(ppDesktop);
		}
		
		/*static*/ 
		bool WinlogonRouter::WlxSetReturnDesktop(HANDLE hWlx, PWLX_DESKTOP pDesktop)
		{
			if(!s_interface) return false;
			return s_interface->WlxSetReturnDesktop(pDesktop);
		}

		/*static*/ 
		bool WinlogonRouter::WlxCreateUserDesktop(HANDLE hWlx, HANDLE hToken, DWORD Flags, PWSTR pszDesktopName, PWLX_DESKTOP *ppDesktop)
		{
			if(!s_interface) return false;
			return s_interface->WlxCreateUserDesktop(hToken, Flags, pszDesktopName, ppDesktop);
		}

		/*static*/ 
		int  WinlogonRouter::WlxChangePasswordNotifyEx(HANDLE hWlx, PWLX_MPR_NOTIFY_INFO pMprInfo, DWORD dwChangeInfo, PWSTR ProviderName, PVOID Reserved)
		{
			if(!s_interface) return -1;
			return s_interface->WlxChangePasswordNotifyEx(pMprInfo, dwChangeInfo, ProviderName, Reserved);
		}

		/*static*/ 
		bool WinlogonRouter::WlxCloseUserDesktop(HANDLE hWlx, PWLX_DESKTOP pDesktop, HANDLE hToken)
		{
			if(!s_interface) return false;
			return s_interface->WlxCloseUserDesktop(pDesktop, hToken);
		}

		/*static*/ 
		bool WinlogonRouter::WlxSetOption(HANDLE hWlx, DWORD Option, ULONG_PTR Value, ULONG_PTR *OldValue)
		{
			if(!s_interface) return false;
			return s_interface->WlxSetOption(Option, Value, OldValue);
		}
		
		/*static*/ 
		bool WinlogonRouter::WlxGetOption(HANDLE hWlx, DWORD Option, ULONG_PTR *Value)
		{
			if(!s_interface) return false;
			return s_interface->WlxGetOption(Option, Value);
		}

		/*static*/ 
		void WinlogonRouter::WlxWin31Migrate(HANDLE hWlx)
		{
			s_interface->WlxWin31Migrate();
		}

		/*static*/ 
		int  WinlogonRouter::WlxQueryTerminalServicesData(HANDLE hWlx, PWLX_TERMINAL_SERVICES_DATA pTSData, WCHAR *UserName, WCHAR *Domain)
		{
			if(!s_interface) return -1;
			return s_interface->WlxQueryTerminalServicesData(pTSData, UserName, Domain);
		}

		// These don't have an hWlx param for getting back to us directly, we have to use a /*static*/ global :/			
		/*static*/ 
		bool WinlogonRouter::WlxQueryClientCredentials(PWLX_CLIENT_CREDENTIALS_INFO_V1_0 pCred)
		{
			if(!s_interface) return false;
			return s_interface->WlxQueryClientCredentials(pCred);
		}

		/*static*/
		bool WinlogonRouter::WlxQueryInetConnectorCredentials(PWLX_CLIENT_CREDENTIALS_INFO_V1_0 pCred)
		{
			if(!s_interface) return false;
			return s_interface->WlxQueryInetConnectorCredentials(pCred);
		}

		/*static*/ 
		bool WinlogonRouter::WlxDisconnect()
		{
			if(!s_interface) return false;
			return s_interface->WlxDisconnect();
		}

		/*static*/ 
		int  WinlogonRouter::WlxQueryConsoleSwitchCredentials(PWLX_CONSOLESWITCH_CREDENTIALS_INFO_V1_0 pCred)
		{
			if(!s_interface) return -1;
			return s_interface->WlxQueryConsoleSwitchCredentials(pCred);
		}

		/*static*/ 
		bool WinlogonRouter::WlxQueryTsLogonCredentials(PWLX_CLIENT_CREDENTIALS_INFO_V2_0 pCred)
		{
			if(!s_interface) return false;
			return s_interface->WlxQueryTsLogonCredentials(pCred);
		}
	}
}