#pragma once

#include <Windows.h>
#include <string>
#include <Buffer.h>

namespace pGina
{
	namespace Memory
	{
		class BinaryWriter
		{
		public:
			BinaryWriter(Buffer &buffer)			  
			{
				m_buffer = buffer.Raw();
				m_bufferLength = buffer.Length();
				m_cursor = m_buffer;
			}

			BinaryWriter(unsigned char * buffer, int length)
			{
				m_buffer = buffer;
				m_bufferLength = length;
				m_cursor = m_buffer;
			}

			void Write(int);
			void Write(unsigned char);
			void Write(bool);
			void Write(std::string const&);			

		private:
			unsigned char * m_buffer;
			unsigned char * m_cursor;
			int m_bufferLength;			
		};
	}
}