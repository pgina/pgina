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
			BinaryWriter()
			{
				m_buffer = 0;
				m_cursor = 0;
				m_bufferLength = 0;	// We'll just return write sizes
				m_bytesWritten = 0;
			}

			BinaryWriter(Buffer &buffer)				
			{
				m_buffer = buffer.Raw();
				m_bufferLength = buffer.Length();
				m_cursor = m_buffer;
				m_bytesWritten = 0;
			}

			BinaryWriter(Buffer *buffer) 
			{
				m_buffer = buffer ? buffer->Raw() : 0;
				m_bufferLength = buffer ? buffer->Length() : 0;
				m_cursor = m_buffer;
				m_bytesWritten = 0;				
			}

			BinaryWriter(unsigned char * buffer, int length)
			{
				m_buffer = buffer;
				m_bufferLength = length;
				m_cursor = m_buffer;
				m_bytesWritten = 0;
			}

			void Write(int);
			void Write(unsigned char);
			void Write(bool);
			void Write(std::string const&);			
			void Write(std::wstring const&);

			bool EndOfBuffer() { return (m_cursor - m_buffer >= m_bufferLength); }
			int  BytesWritten() { return m_bytesWritten; }

		private:
			void Encode7bitLength(int length);

			unsigned char * m_buffer;
			unsigned char * m_cursor;
			int m_bufferLength;			
			int m_bytesWritten;
		};
	}
}