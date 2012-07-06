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
#include "PipeClient.h"
#include "BinaryReader.h"
#include "BinaryWriter.h"

namespace pGina
{
	namespace NamedPipes
	{
		PipeClient::PipeClient(std::wstring const& path)
			: m_path(path), m_connectTimeout(NMPWAIT_USE_DEFAULT_WAIT), m_pipe(INVALID_HANDLE_VALUE)
		{
		}

		PipeClient::PipeClient(std::wstring const& path, int connectTimeout)
			: m_path(path), m_connectTimeout(connectTimeout), m_pipe(INVALID_HANDLE_VALUE)
		{
		}

		PipeClient::~PipeClient()
		{
			Close();
		}

		bool PipeClient::Connect()
		{
			return Connect(m_connectTimeout);
		}

		bool PipeClient::Connect(int timeout)
		{
			if(!WaitNamedPipe(m_path.c_str(), (DWORD) timeout))
				return false;

			if((m_pipe = CreateFile(m_path.c_str(), GENERIC_READ | GENERIC_WRITE, 0, NULL, OPEN_EXISTING, 0, NULL)) ==
				INVALID_HANDLE_VALUE)
				return false;

			return true;
		}

		pGina::Memory::Buffer * PipeClient::ReadLengthEncodedBuffer()
		{
			pGina::Memory::Buffer * lengthBuffer = ReadBuffer(sizeof(int));
			if(lengthBuffer == 0) return 0;

			int length = 0;
			{
				pGina::Memory::BinaryReader reader(lengthBuffer);
				length = reader.ReadInt32();
				delete lengthBuffer;	// No longer needed, not that reader() should no longer be used, hence its scoping
				lengthBuffer = 0;
			}			

			if(length <= 0)
				return 0;

			return ReadBuffer(length);
		}

		bool PipeClient::WriteLengthEncodedBuffer(pGina::Memory::Buffer *buffer)
		{
			pGina::Memory::Buffer lengthBuffer(4);
			pGina::Memory::BinaryWriter writer(lengthBuffer);
			writer.Write(buffer->Length());

			if(WriteBuffer(&lengthBuffer))
			{
				return WriteBuffer(buffer);
			}

			return false;
		}

		pGina::Memory::Buffer * PipeClient::ReadBuffer(int size)
		{
			if(size <= 0)
				return 0;

			pGina::Memory::Buffer * buffer = new pGina::Memory::Buffer(size);
			
			unsigned char * ptr = buffer->Raw();
			int length = buffer->Length();

			while(length > 0)
			{
				int read = Read(ptr, length);
				if(read == 0)
				{
					delete buffer;
					return false;
				}

				ptr += read;
				length -= read;
			}

			return buffer;
		}

		bool PipeClient::WriteBuffer(pGina::Memory::Buffer *buffer)
		{
			if(buffer && buffer->Length() > 0)
			{
				unsigned char * ptr = buffer->Raw();
				int length = buffer->Length();

				while(length > 0)
				{
					int written = Write(ptr, length);
					if(written == 0)
						return false;

					ptr += written;
					length -= written;
				}
			}

			return true;
		}

		int PipeClient::Read(unsigned char * buffer, int len)
		{
			DWORD bytesRead = 0;			

			if(ReadFile(m_pipe, buffer, (DWORD) len, &bytesRead, NULL))
				return bytesRead;

			return 0;	
		}

		int PipeClient::Write(unsigned char * buffer, int len)
		{
			DWORD bytesWritten = 0;

			if(WriteFile(m_pipe, buffer, (DWORD) len, &bytesWritten, NULL))
				return bytesWritten;

			return 0;			
		}

		void PipeClient::Close()
		{
			if(m_pipe != INVALID_HANDLE_VALUE)
			{
				CloseHandle(m_pipe);
				m_pipe = INVALID_HANDLE_VALUE;
			}
		}
	}
}