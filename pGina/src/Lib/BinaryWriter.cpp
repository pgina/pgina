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
			int numchars = static_cast<int>(v.size());
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