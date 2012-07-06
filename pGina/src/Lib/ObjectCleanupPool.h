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
#include <vector>

namespace pGina
{
	namespace Memory
	{
		class ObjectCleanupBase
		{
		public:
			ObjectCleanupBase(void * memory) : m_memory(memory) {}
			virtual ~ObjectCleanupBase() {}

		protected:
			void * m_memory;
		};

		class FreeCleanup : public ObjectCleanupBase
		{
		public:
			FreeCleanup(void *memory) : ObjectCleanupBase(memory) {}
			virtual ~FreeCleanup()
			{
				if(m_memory)
					free(m_memory);
			}
		};

		class DeleteCleanup : public ObjectCleanupBase
		{
		public:
			DeleteCleanup(void *memory) : ObjectCleanupBase(memory) {}
			virtual ~DeleteCleanup()
			{
				if(m_memory)
					delete m_memory;
			}
		};

		class LocalFreeCleanup : public ObjectCleanupBase
		{
		public:
			LocalFreeCleanup(void *memory) : ObjectCleanupBase(memory) {}
			virtual ~LocalFreeCleanup()
			{
				if(m_memory)
					LocalFree(m_memory);
			}
		};

		class CoTaskMemFreeCleanup : public ObjectCleanupBase
		{
		public:
			CoTaskMemFreeCleanup(void *memory) : ObjectCleanupBase(memory) {}
			virtual ~CoTaskMemFreeCleanup()
			{
				if(m_memory)
					CoTaskMemFree(m_memory);
			}
		};
		
		class ObjectCleanupPool
		{
		public:
			ObjectCleanupPool() {}
			~ObjectCleanupPool()
			{				
				for(std::vector<ObjectCleanupBase *>::iterator itr = m_cleanupVector.begin(); itr != m_cleanupVector.end(); ++itr)
				{
					delete *itr;					
				}

				m_cleanupVector.clear();
			}
			
			void AddFree(void *mem)
			{
				if(mem)
				{
					Add(new FreeCleanup(mem));
				}
			}

			void Add(void *obj)
			{
				Add(new DeleteCleanup(obj));
			}

			void Add(ObjectCleanupBase *obj)
			{
				if(obj)
				{
					m_cleanupVector.push_back(obj);
				}
			}
		
		private:			
			std::vector<ObjectCleanupBase *> m_cleanupVector;
		};
	}
}