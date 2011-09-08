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
				SAS_ACTION_LOGON =						 (1),
				SAS_ACTION_NONE =						 (2),
				SAS_ACTION_LOCK_WKSTA =					 (3),
				SAS_ACTION_LOGOFF =						 (4),
				SAS_ACTION_SHUTDOWN =					 (5),
				SAS_ACTION_PWD_CHANGED =				 (6),
				SAS_ACTION_TASKLIST  =                   (7),
				SAS_ACTION_UNLOCK_WKSTA =                (8),
				SAS_ACTION_FORCE_LOGOFF =                (9),
				SAS_ACTION_SHUTDOWN_POWER_OFF =          (10),
				SAS_ACTION_SHUTDOWN_REBOOT =             (11),
				SAS_ACTION_SHUTDOWN_SLEEP =              (12),
				SAS_ACTION_SHUTDOWN_SLEEP2 =             (13),
				SAS_ACTION_SHUTDOWN_HIBERNATE =          (14),
				SAS_ACTION_RECONNECTED =                 (15),
				SAS_ACTION_DELAYED_FORCE_LOGOFF =        (16),
				SAS_ACTION_SWITCH_CONSOLE =              (17),				
				PGINA_EMERGENCY_ESCAPE_HATCH,
			};

		public:
			DialogLoggedOutSAS(WinlogonInterface *iface) :
				DialogBase(iface, IDD_LOGGEDOUT_SAS)
				{					
				}
			
			virtual void DialogInit();
			virtual bool Command(int itemId);
			virtual INT_PTR DialogProcImpl(UINT msg, WPARAM wparam, LPARAM lparam);

			std::wstring Username() { return m_username; }
			void		 Username(std::wstring const& v) { m_username = v; }
			std::wstring Password() { return m_password; }
			void		 Password(std::wstring const& v) { m_password = v; }
			

		private:
			std::wstring m_username;
			std::wstring m_password;
		};
	}
}