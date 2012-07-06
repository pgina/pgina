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
#include <windows.h>
#include <stdio.h>
#include <assert.h>
#include <lm.h>
#include <WtsApi32.h>

#include "Helpers.h"

namespace pGina
{
	namespace Helpers
	{
		std::wstring GetMachineName()
		{
			WCHAR computerName[MAX_COMPUTERNAME_LENGTH+1];
			DWORD computerNameLen = ARRAYSIZE(computerName);			

			if (!GetComputerNameW(computerName, &computerNameLen))
				return L".";
			else
				return computerName;
		}

		bool UserIsRemote() 
		{
			return 0 != GetSystemMetrics(SM_REMOTESESSION);
		}
		
		bool IsUserLocalAdmin(std::wstring username)
		{			
			bool result = false;
			LPUSER_INFO_3 userInfo;

			if(NetUserGetInfo(NULL, username.c_str(), 3, (LPBYTE *)&userInfo) == NERR_Success)
            {
				result = (userInfo->usri3_priv == USER_PRIV_ADMIN);				
				NetApiBufferFree(userInfo);
			}

			return result;
		}

		std::wstring GetSessionUsername(DWORD sessionId)
		{
			std::wstring result;
			LPWSTR buffer = NULL;
			DWORD bufferSize = 0;

			if(WTSQuerySessionInformation(WTS_CURRENT_SERVER_HANDLE, sessionId, WTSUserName, &buffer, &bufferSize))
			{
				result = buffer;
				WTSFreeMemory(buffer);
			}

			return result;
		}

		std::wstring GetSessionDomainName(DWORD sessionId)
		{
			std::wstring result;
			LPWSTR buffer = NULL;
			DWORD bufferSize = 0;

			if(WTSQuerySessionInformation(WTS_CURRENT_SERVER_HANDLE, sessionId, WTSDomainName, &buffer, &bufferSize))
			{
				result = buffer;
				WTSFreeMemory(buffer);
			}

			return result;
		}

		DWORD GetCurrentSessionId()
		{
			DWORD sessionId = -1;
			ProcessIdToSessionId(GetCurrentProcessId(), &sessionId);
			return sessionId;
		}
	}
}