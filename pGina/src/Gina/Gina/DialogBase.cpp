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
#include "DialogBase.h"
#include "Dll.h"

namespace pGina
{
	namespace GINA
	{
		DialogBase::DialogBase(WinlogonInterface * iface, int dialogId) :
			m_winlogon(iface), m_dialogId(dialogId), m_instance(GetMyInstance()), m_hwnd(0), m_nextTimer(1)
		{
		}
		
		int DialogBase::ShowDialog()
		{
			return m_winlogon->WlxDialogBoxParam(m_instance, MAKEINTRESOURCE(m_dialogId), 0, DialogProcInternal, (LPARAM)this);
		}

		/*static*/
		INT_PTR CALLBACK DialogBase::DialogProcInternal(HWND hwnd, UINT msg, WPARAM wparam, LPARAM lparam)
		{
			// When we get an init, we can count on lparam being the value we specified
			if(msg == WM_INITDIALOG)
			{
				// lparam is us!
				DialogBase * dialog = (DialogBase *)lparam;
				dialog->Hwnd(hwnd);
				dialog->CenterWindow();

				// Now set a ptr to us in user data that will always be available
				SetWindowLongPtr(hwnd, GWLP_USERDATA, lparam);				

				// Inherited init
				dialog->DialogInit();
				return TRUE;
			}
			else
			{
				// Try and get a ptr to us out of user data, if not available, just do nothing
				DialogBase * dialog = (DialogBase *) GetWindowLongPtr(hwnd, GWLP_USERDATA);
				if(!dialog)
					return FALSE;
			
				// We call different verbs on subs based on message
				switch(msg)
				{
				case WM_COMMAND:
					if(dialog->Command(LOWORD(wparam)))
						return true;					
					break;				
				case WM_TIMER:
					if(dialog->Timer(LOWORD(wparam)))
						return true;
					break;
				case WM_DESTROY:
					dialog->InternalDestroy();					
					break;
				}			

				return dialog->DialogProcImpl(msg, wparam, lparam);					
			}
		}

		void DialogBase::InternalDestroy()
		{
			if(m_nextTimer > 1)
			{
				for(int x = 1; x < m_nextTimer; x++)
				{
					StopTimer(x);
				}
			}

			Destroy();
		}

		void DialogBase::CenterWindow()
		{						
			RECT    rect = {0, 0, 0, 0};
			GetWindowRect(m_hwnd, &rect);
			LONG Style = GetWindowLong(m_hwnd, GWL_STYLE);

			LONG dx = rect.right - rect.left;
			LONG dy = rect.bottom - rect.top;
						
			LONG    dxParent, dyParent;			
			if ((Style & WS_CHILD) == 0) 
			{
				// No parent, center on screen
				dxParent = GetSystemMetrics(SM_CXSCREEN);
				dyParent = GetSystemMetrics(SM_CYSCREEN);
			} 
			else  
			{
				// Center on parent								
				HWND hwndParent = GetParent(m_hwnd);
				if (hwndParent == NULL)  
				{
					// No parent? Use desktop...
					hwndParent = GetDesktopWindow(); 
				}

				RECT    rectParent;
				GetWindowRect(hwndParent, &rectParent);

				dxParent = rectParent.right - rectParent.left;
				dyParent = rectParent.bottom - rectParent.top;
			}

			rect.left = (dxParent - dx) / 2;
			rect.top  = (dyParent - dy) / 3;

			SetWindowPos(m_hwnd, HWND_TOPMOST, rect.left, rect.top, 0, 0, SWP_NOSIZE);
			SetForegroundWindow(m_hwnd);
		}

		HWND DialogBase::GetItem(int itemId)
		{
			return GetDlgItem(m_hwnd, itemId);
		}

		void DialogBase::SetCaption(const wchar_t *caption)
		{
			SetWindowText(m_hwnd, caption);
		}

		void DialogBase::SetItemText(int itemId, const wchar_t *text)
		{
			SetDlgItemText(m_hwnd, itemId, text);
		}
		
		void DialogBase::EnableItem(int itemId)
		{
			EnableWindow(GetItem(itemId), TRUE);
		}

		void DialogBase::DisableItem(int itemId)
		{
			EnableWindow(GetItem(itemId), FALSE);
		}

		void DialogBase::HideItem(int itemId)
		{			
			ShowWindow(GetItem(itemId), FALSE);
		}

		void DialogBase::ShowItem(int itemId)
		{
			ShowWindow(GetItem(itemId), TRUE);
		}

		void DialogBase::CheckState(int itemId, bool checkstate)
		{
			SendMessage(GetItem(itemId), BM_SETCHECK, checkstate, 0);
		}

		bool DialogBase::CheckState(int itemId)
		{
			return IsDlgButtonChecked(m_hwnd, itemId) == BST_CHECKED;
		}

		std::wstring DialogBase::GetItemText(int itemId)
		{			
			wchar_t buffer[1024 * 64];	// 64k buffer
			memset(buffer, 0, sizeof(buffer));
			GetDlgItemText(m_hwnd, itemId, buffer, 1024 * 64);
			return buffer;
		}

		void DialogBase::SetFocusItem(int itemid)
		{
			SetFocus(GetItem(itemid));
		}

		void DialogBase::SetItemBitmap(int itemid, HBITMAP bitmap)
		{
			SendDlgItemMessage(m_hwnd, itemid, STM_SETIMAGE, IMAGE_BITMAP, (LPARAM)bitmap);
		}

		int DialogBase::StartTimer(unsigned int period)
		{
			int timerId = m_nextTimer;
			m_nextTimer++;

			UINT_PTR result = SetTimer(m_hwnd, (UINT_PTR) timerId, period, NULL);
			return timerId;
		}

		void DialogBase::StopTimer(int timerId)
		{
			KillTimer(m_hwnd, (UINT_PTR) timerId);
		}
	}
}