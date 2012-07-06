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
#include "Dll.h"

#include <wchar.h>
#include <Macros.h>

// ID's for MSGINA's WlxLoggedOutSAS dialog
#define IDC_MSGINA_USERNAME 1502
#define IDC_MSGINA_PASSWORD 1503

namespace pGina
{
	namespace GINA
	{				
		void DialogLoggedOutSAS::DialogInit()
		{
			if(!m_username.empty()) SetItemText(IDC_USERNAME_TXT, m_username.c_str());
			if(!m_password.empty()) SetItemText(IDC_PASSWORD_TXT, m_password.c_str());
			ApplyLogoImage();
			SetFocusItem(IDC_USERNAME_TXT);

			bool specialActionEnabled = pGina::Registry::GetBool(L"EnableSpecialActionButton", false);
			if(specialActionEnabled)
			{
				EnableItem(IDC_SPECIAL);
				SetItemText(IDC_SPECIAL, pGina::Registry::GetString(L"SpecialAction", L"Shutdown").c_str());
			}
			else
				DisableItem(IDC_SPECIAL);			

			// Service status
			if( pGina::Registry::GetBool(L"ShowServiceStatusInLogonUi", true) )
			{
				SetServiceStatus();

				// Start a timer to update service status
				m_statusTimerId = StartTimer(pGina::Registry::GetDword(L"PingSleepTime", 5000));
			}
			else
			{
				DisableItem(IDC_STATUS);
			}

			// MOTD
			if( pGina::Registry::GetBool(L"EnableMotd", true) )
			{
				std::wstring motd = pGina::Transactions::TileUi::GetDynamicLabel(L"MOTD");
				SetItemText(IDC_MOTD, motd.c_str());
			}
			else
			{
				DisableItem(IDC_MOTD);
			}
		}

		bool DialogLoggedOutSAS::Command(int itemId)
		{
			switch(itemId)
			{
			case IDCANCEL:
				FinishWithResult(SAS_ACTION_NONE);
				return true;	
			
			case IDC_LOGIN_BUTTON:
				m_username = GetItemText(IDC_USERNAME_TXT);
				m_password = GetItemText(IDC_PASSWORD_TXT);
				FinishWithResult(SAS_ACTION_LOGON);
				return true;

			case IDC_SPECIAL:
				{
					std::wstring action = pGina::Registry::GetString(L"SpecialAction", L"Shutdown");
					if(_wcsicmp(action.c_str(), L"Shutdown") == 0)
						FinishWithResult(SAS_ACTION_SHUTDOWN_POWER_OFF);
					else if(_wcsicmp(action.c_str(), L"Reboot") == 0)
						FinishWithResult(SAS_ACTION_SHUTDOWN_REBOOT);
					else if(_wcsicmp(action.c_str(), L"Sleep") == 0)
						FinishWithResult(SAS_ACTION_SHUTDOWN_SLEEP);
					else if(_wcsicmp(action.c_str(), L"Hibernate") == 0)
						FinishWithResult(SAS_ACTION_SHUTDOWN_HIBERNATE);					
				}
				return true;
			}

			return false;
		}
		
		void DialogLoggedOutSAS::SetServiceStatus()
		{
			if( pGina::Registry::GetBool(L"ShowServiceStatusInLogonUi", true) )
			{
				bool up = pGina::Transactions::Service::Ping();
				if(up)
				{
					SetItemText(IDC_STATUS, L"Service Status: Connected");
				}
				else
				{
					SetItemText(IDC_STATUS, L"Service Status: Disconnected");
				}
			}
		}

		bool DialogLoggedOutSAS::Timer(int timerId)
		{
			if(timerId == m_statusTimerId)
			{
				SetServiceStatus();	
				return true;
			}

			return false;
		}

		INT_PTR DialogLoggedOutSAS::DialogProcImpl(UINT msg, WPARAM wparam, LPARAM lparam)
		{			
			return FALSE;
		}		

		void DialogLoggedOutSAS::ApplyLogoImage()
		{
			if(m_bitmap == NULL)
			{
				std::wstring tileImage = pGina::Registry::GetString(L"TileImage", L"");
				if(tileImage.empty() || tileImage.length() == 1)
				{
					// Use builtin
					m_bitmap = LoadBitmap(GetMyInstance(), MAKEINTRESOURCE(IDB_PGINA_LOGO));
				}
				else
				{
					pDEBUG(L"Credential::GetBitmapValue: Loading image from: %s", tileImage.c_str());
					m_bitmap = (HBITMAP) LoadImageW((HINSTANCE) NULL, tileImage.c_str(), IMAGE_BITMAP, 0, 0, LR_LOADFROMFILE);			
				}				
			}

			if(m_bitmap)
			{
				SetItemBitmap(IDC_LOGO, m_bitmap);
			}
		}
	}
}