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
#include "DialogLoggedOutSAS.h"

#include <Macros.h>

namespace pGina
{
	namespace GINA
	{
		void DialogLoggedOutSAS::DialogInit()
		{
			SetItemText(IDC_USERNAME_TXT, L"Duuude!");
			SetFocusItem(IDC_PASSWORD_TXT);
		}

		bool DialogLoggedOutSAS::Command(int itemId)
		{
			switch(itemId)
			{
			case IDCANCEL:
				FinishWithResult(DialogResult::SAS_ACTION_NONE);
				return true;	

			case IDC_EMERGENCY_ESCAPE_HATCH:
				FinishWithResult(DialogResult::PGINA_EMERGENCY_ESCAPE_HATCH);
				return true;

			case IDC_LOGIN_BUTTON:
				FinishWithResult(LoginAttempt(GetItemText(IDC_USERNAME_TXT), GetItemText(IDC_PASSWORD_TXT)));
				return true;
			}

			return false;
		}

		INT_PTR DialogLoggedOutSAS::DialogProcImpl(UINT msg, WPARAM wparam, LPARAM lparam)
		{			
			return FALSE;
		}

		DialogLoggedOutSAS::DialogResult DialogLoggedOutSAS::LoginAttempt(std::wstring const& username, std::wstring const& password)
		{
			// WLX_SAS_ACTION_LOGON
			pDEBUG(L"DialogLoggedOutSAS::LoginAttempt: Processing login for %s", username.c_str());
			m_loginResult = pGina::Transactions::User::ProcessLoginForUser(username.c_str(), NULL, password.c_str());
			if(!m_loginResult.Result())
			{
				pERROR(L"DialogLoggedOutSAS::LoginAttempt: %s", m_loginResult.Message().c_str());
				return DialogResult::PGINA_LOGIN_FAILED;
			}			

			pDEBUG(L"DialogLoggedOutSAS::LoginAttempt: Successful, resulting username: %s", m_loginResult.Username().c_str());
			return DialogResult::SAS_ACTION_LOGON;
		}
	}
}