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
#include <string>
#include <vector>

#include "Winlogon.h"

namespace pGina
{
	namespace GINA
	{
		// Simple base class that 'ui' dialogs can inherit from, thanks to Keith Brown and his 
		//	excellent GINA series for this idea - basically we just proxy all DialogProcs through
		//  a static proc with a class ptr as user data.  This lets each dialog have its own
		//  class and handler, without the added work of individual WlxDialogBox() calls all over
		//  the place.  Instead, just YourDialogBox->ShowDialog().
		class DialogBase
		{
		public:
			DialogBase(WinlogonInterface * iface, int dialogId);			
			int ShowDialog();
			
		protected:
			virtual void DialogInit() = 0;			// WM_INITDIALOG
			virtual bool Command(int itemId) = 0;	// WM_COMMAND
			virtual bool Timer(int timerId) = 0;	// WM_TIMER
			virtual void Destroy() {} 				// WM_DESTROY
			virtual INT_PTR DialogProcImpl(UINT msg, WPARAM wparam, LPARAM lparam) = 0;
			
			// Helpers that subclasses can use to center, get/set data etc
			void CenterWindow();
			HWND GetItem(int itemId);
			void SetCaption(const wchar_t *caption);
			void SetItemText(int itemId, const wchar_t *text);
			void SetItemText(int itemId, std::wstring const& text) { SetItemText(itemId, text.c_str()); }
			std::wstring GetItemText(int itemId);
			void EnableItem(int itemId);
			void DisableItem(int itemId);
			void HideItem(int itemId);
			void ShowItem(int itemId);
			void CheckState(int itemId, bool checked);
			bool CheckState(int itemId);
			void SetFocusItem(int itemId);
			void SetItemBitmap(int itemId, HBITMAP bitmap);
			void FinishWithResult(INT_PTR result) { EndDialog(m_hwnd, result); }
			int StartTimer(unsigned int period);
			void StopTimer(int timerId);

		private:
			static INT_PTR CALLBACK DialogProcInternal(HWND hwnd, UINT msg, WPARAM wparam, LPARAM lparam);
			void Hwnd(HWND v) { m_hwnd = v; }
			void InternalDestroy();

		protected:
			WinlogonInterface * m_winlogon;
			int m_dialogId;
			HINSTANCE m_instance;
			HWND m_hwnd;
			int m_nextTimer;
		};
	}
}