#pragma once

#include <Windows.h>
#include <vector>

namespace pGina
{
	namespace Memory
	{
		class ObjectCleanupPool
		{
		public:
			ObjectCleanupPool() {}
			~ObjectCleanupPool()
			{
				for(std::vector<void *>::iterator itr = m_delobjects.begin(); itr != m_delobjects.end(); ++itr)
				{
					delete *itr;
				}

				m_delobjects.clear();

				for(std::vector<DestroyerType>::iterator itr = m_destroyers.begin(); itr != m_destroyers.end(); ++itr)
				{
					itr->cleanupFunc(itr->cleanupArg);					
				}

				m_destroyers.clear();
			}

			void Add(void *object)
			{
				if(object)
				{
					m_delobjects.push_back(object);
				}
			}

			void Add(void *object, void (*func)(void *))
			{
				if(object)
				{
					DestroyerType foo;
					foo.cleanupArg = func;
					foo.cleanupArg = object;
					m_destroyers.push_back(foo);
				}
			}

		private:			
			struct DestroyerType
			{
				void (*cleanupFunc)(void *);
				void * cleanupArg;
			};

		private:
			std::vector<void *> m_delobjects;
			std::vector<DestroyerType> m_destroyers;
		};
	}
}