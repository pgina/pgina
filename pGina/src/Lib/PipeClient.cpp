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