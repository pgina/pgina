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

#include <pGinaNativeLib.h>

#include "DialogBase.h"
#include "resource.h"

namespace pGina
{
	namespace GINA
	{
		class DialogLoggedOutSAS : public DialogBase
		{
		public:
			typedef enum DialogResult
			{				
				SAS_ACTION_LOGON =						 WLX_SAS_ACTION_LOGON,
				SAS_ACTION_NONE =						 WLX_SAS_ACTION_NONE,
				SAS_ACTION_LOCK_WKSTA =					 WLX_SAS_ACTION_LOCK_WKSTA,
				SAS_ACTION_LOGOFF =						 WLX_SAS_ACTION_LOGOFF,
				SAS_ACTION_SHUTDOWN =					 WLX_SAS_ACTION_SHUTDOWN,
				SAS_ACTION_PWD_CHANGED =				 WLX_SAS_ACTION_PWD_CHANGED,
				SAS_ACTION_TASKLIST  =                   WLX_SAS_ACTION_TASKLIST,
				SAS_ACTION_UNLOCK_WKSTA =                WLX_SAS_ACTION_UNLOCK_WKSTA,
				SAS_ACTION_FORCE_LOGOFF =                WLX_SAS_ACTION_FORCE_LOGOFF,
				SAS_ACTION_SHUTDOWN_POWER_OFF =          WLX_SAS_ACTION_SHUTDOWN_POWER_OFF,
				SAS_ACTION_SHUTDOWN_REBOOT =             WLX_SAS_ACTION_SHUTDOWN_REBOOT,
				SAS_ACTION_SHUTDOWN_SLEEP =              WLX_SAS_ACTION_SHUTDOWN_SLEEP,
				SAS_ACTION_SHUTDOWN_SLEEP2 =             WLX_SAS_ACTION_SHUTDOWN_SLEEP2,
				SAS_ACTION_SHUTDOWN_HIBERNATE =          WLX_SAS_ACTION_SHUTDOWN_HIBERNATE,
				SAS_ACTION_RECONNECTED =                 WLX_SAS_ACTION_RECONNECTED,
				SAS_ACTION_DELAYED_FORCE_LOGOFF =        WLX_SAS_ACTION_DELAYED_FORCE_LOGOFF,
				SAS_ACTION_SWITCH_CONSOLE =              WLX_SAS_ACTION_SWITCH_CONSOLE,			
				SAS_ACTION_MIN =						 SAS_ACTION_LOGON,
				SAS_ACTION_MAX =						 SAS_ACTION_SWITCH_CONSOLE
			};

		public:
			DialogLoggedOutSAS(WinlogonInterface *iface) :
				DialogBase(iface, IDD_LOGGEDOUT_SAS),
					m_bitmap(NULL), m_statusTimerId(0)
				{					
				}
			
			virtual void DialogInit();
			virtual bool Command(int itemId);
			virtual bool Timer(int timerId);
			virtual INT_PTR DialogProcImpl(UINT msg, WPARAM wparam, LPARAM lparam);

			std::wstring Username() { return m_username; }
			void		 Username(std::wstring const& v) { m_username = v; }
			std::wstring Password() { return m_password; }
			void		 Password(std::wstring const& v) { m_password = v; }
		
		private:
			void ApplyLogoImage();
			void SetServiceStatus();

		private:
			std::wstring m_username;
			std::wstring m_password;
			HBITMAP m_bitmap;
			int m_statusTimerId;
		};
	}
}