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
#include <assert.h>
#include "BinaryWriter.h"

#if _DEBUG
#define BOUNDS_CHECK(size) if( (m_cursor + size) - m_buffer > m_bufferLength ) assert(0)
#else
#define BOUNDS_CHECK(size)
#endif

namespace pGina
{
	namespace Memory
	{
		void BinaryWriter::Write(int v)
		{
			if(m_cursor)
			{
				BOUNDS_CHECK(sizeof(int));
				memcpy(m_cursor, &v, sizeof(int));
				m_cursor += sizeof(int);
			}

			m_bytesWritten += sizeof(int);
		}

		void BinaryWriter::Write(unsigned char v)
		{
			if(m_cursor)
			{
				BOUNDS_CHECK(1);
				m_cursor[0] = v;
				m_cursor++;
			}

			m_bytesWritten++;
		}

		void BinaryWriter::Write(bool v)
		{
			unsigned char val = (v ? 0x01 : 0x00);
			Write(val);
		}

		void BinaryWriter::Write(std::string const& v)
		{
			int numchars = static_cast<int>(v.size());
			Encode7bitLength(numchars);

			if(m_cursor)
			{
				memcpy(m_cursor, v.c_str(), numchars);
				m_cursor += numchars;
			}

			m_bytesWritten += numchars;
		}

		void BinaryWriter::Write(std::wstring const& v)
		{
			int numchars = static_cast<int>(v.size()) * sizeof(wchar_t);
			Encode7bitLength(numchars);

			if(m_cursor)
			{
				memcpy(m_cursor, v.c_str(), numchars);
				m_cursor += numchars;
			}

			m_bytesWritten += numchars;
		}

		void BinaryWriter::Encode7bitLength(int length)
		{
			unsigned int len = (unsigned int) length;

			while(len >= 0x80)
			{
				unsigned char bit = (len | 0x80);
				Write(bit);
				len >>= 7;
			}

			// Truncate and write the remaining bit
			Write((unsigned char) len);			
		}
	}
}