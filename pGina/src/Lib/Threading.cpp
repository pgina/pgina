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
#include "Threading.h"

namespace pGina
{
	namespace Threading
	{
		Thread::Thread() :
			m_threadHandle(0),
			m_running(false)
		{
		}

		Thread::~Thread()
		{
			if(Running())
				Stop();
			
			if(m_threadHandle != 0)
				CloseHandle(m_threadHandle);
		}

		void Thread::Start()
		{
			if(Running())
				return;

			Running(true);
			m_threadHandle = CreateThread(NULL, 0, _internal_threadmain, this, 0, 0);
		}

		void Thread::Stop()
		{
			if(!Running())
				return;

			Running(false);
			WaitForSingleObject(m_threadHandle, INFINITE);
			CloseHandle(m_threadHandle);
			m_threadHandle = 0;
		}

		bool Thread::Running()
		{
			ScopedLock lock(m_mutex);
			return m_running;
		}

		void Thread::Running(bool v)
		{
			ScopedLock lock(m_mutex);
			m_running = v;
		}
			    		
		/* static */
		DWORD WINAPI Thread::_internal_threadmain(LPVOID arg)
		{
			Thread *thread = static_cast<Thread *>(arg);
			return thread->ThreadMain();
		}
			
		Mutex::Mutex()
		{
			m_mutexHandle = CreateMutex(NULL, FALSE, NULL);
		}

		bool Mutex::Lock()
		{
			DWORD res = WaitForSingleObject(m_mutexHandle, INFINITE);
			if(res == WAIT_OBJECT_0 || res == WAIT_ABANDONED)
				return true;
  
			return false;
		}

		bool Mutex::Unlock()
		{
			if(ReleaseMutex(m_mutexHandle))
				return true;

			return false;
		}
	}
}