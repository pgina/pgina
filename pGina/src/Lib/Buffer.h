#pragma once

#include <Windows.h>

namespace pGina
{
	namespace Memory
	{
		class Buffer
		{
		public:
			Buffer() :
			  m_buffer(0), m_length(0)
			{}

			Buffer(int size) :
			  m_length(size)
			{
				m_buffer = (unsigned char *) malloc(size);
				memset(m_buffer, 0, size);
			}

			~Buffer() 
			{
				if(m_buffer) free(m_buffer);
			}

			unsigned char * Raw() { return m_buffer; }
			int             Length() { return m_length; }

		private:
			unsigned char * m_buffer;
			int m_length;
		};
	}
}