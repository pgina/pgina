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
#include "HookedLoggedOutSAS.h"

#include <Macros.h>

// ID's for MSGINA's WlxLoggedOutSAS dialog
#define IDC_MSGINA_USERNAME 1502
#define IDC_MSGINA_PASSWORD 1503

namespace pGina
{
	namespace GINA
	{
		/* static */ DLGPROC HookedLoggedOutSAS::s_hookedDlgProc = 0;
		/* static */ bool    HookedLoggedOutSAS::s_hookingEnabled = false;
		/* static */ pGina::Transactions::User::LoginResult HookedLoggedOutSAS::s_loginResult;

		/* static */
		INT_PTR HookedLoggedOutSAS::MicrosoftDialogProcWrapper(HWND hwnd, UINT msg, WPARAM wparam, LPARAM lparam)
		{
			if(s_hookedDlgProc == 0)
			{
				pERROR(L"HookedLoggedOutSAS::MicrosoftDialogProcWrapper: Dialog wrapper called before we know who we're hooking!");
				return 0;	// Nothin we can do!
			}

			// Fall through if hooking is not enabled
			if(!s_hookingEnabled)
				return s_hookedDlgProc(hwnd, msg, wparam, lparam);

			// Hooking is on, so we let msgina do its thing
			INT_PTR msginaResult = s_hookedDlgProc(hwnd, msg, wparam, lparam);

			// If we're init'ing, then we set username, password and queue a message to login
			if(msg == WM_INITDIALOG)
			{
				std::wstring domainUsername = s_loginResult.Domain();
				domainUsername += L"\\";
				domainUsername += s_loginResult.Username();

				pDEBUG(L"HookedLoggedOutSAS::MicrosoftDialogProcWrapper: Hooked dialog, setting username/password and submitting for user: %s", domainUsername.c_str());
				SetDlgItemText(hwnd, IDC_MSGINA_USERNAME, domainUsername.c_str());
				SetDlgItemText(hwnd, IDC_MSGINA_PASSWORD, s_loginResult.Password().c_str());			
				SendMessage(hwnd, WM_COMMAND, 1, 1);
			}

			return msginaResult;
		}
	}
}