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