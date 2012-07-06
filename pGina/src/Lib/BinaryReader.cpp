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
#include "BinaryReader.h"

#if _DEBUG
#define BOUNDS_CHECK(size) if( (m_cursor + size) - m_buffer > m_bufferLength ) assert(0)
#else
#define BOUNDS_CHECK(size)
#endif

namespace pGina
{
	namespace Memory
	{
		int BinaryReader::ReadInt32()
		{
			BOUNDS_CHECK(sizeof(int));
			int value = 0;			
			memcpy(&value, m_cursor, sizeof(int));
			m_cursor += sizeof(int);
			return value;
		}

		unsigned char BinaryReader::ReadByte()
		{
			BOUNDS_CHECK(1);
			unsigned char v = m_cursor[0];
			m_cursor++;
			return v;
		}

		std::string   BinaryReader::ReadUTF8String()
		{	
			std::string value;
			int length = Decode7bitLength();			
			BOUNDS_CHECK(length);

			// We could do some crazy hanky casting to memcpy directly into 
			//	a std::string, but we aren't in dire need of crazy performance,
			//  so we'll take the hit for a spurious malloc/free
			char * buffer = (char *) malloc(length + 1);
			memset(buffer, 0, length + 1);
			memcpy(buffer, m_cursor, length);
			m_cursor += length;
			value = buffer;
			free(buffer);
			return value;
		}

		std::wstring  BinaryReader::ReadUnicodeString()
		{
			std::wstring value;
			int length = Decode7bitLength();			
			BOUNDS_CHECK(length);

			// We could do some crazy hanky casting to memcpy directly into 
			//	a std::string, but we aren't in dire need of crazy performance,
			//  so we'll take the hit for a spurious malloc/free
			wchar_t * buffer = (wchar_t *) malloc(length + 2);
			memset(buffer, 0, length + 2);
			memcpy(buffer, m_cursor, length);
			m_cursor += length;
			value = buffer;
			free(buffer);
			return value;
		}

		bool		  BinaryReader::ReadBool()
		{			
			unsigned char v = ReadByte();
			return (v != 0x00);			
		}

		int BinaryReader::Decode7bitLength()
		{
			unsigned char bit = 0;
			int num = 0, num2 = 0;
			
            do 
            {                 
                bit = ReadByte();
                num |= (bit & 0x7f) << num2; 
                num2 += 7; 
            } while ((bit & 0x80) != 0x00);

			return num;
		}
	}
}