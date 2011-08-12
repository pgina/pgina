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
				for(std::vector<void *>::iterator itr = m_objects.begin(); itr != m_objects.end(); ++itr)
				{
					delete *itr;
				}

				m_objects.clear();
			}

			void Add(void *object)
			{
				if(object)
				{
					m_objects.push_back(object);
				}
			}

		private:
			std::vector<void *> m_objects;
		};
	}
}