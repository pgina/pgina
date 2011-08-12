#pragma once

#include <Windows.h>
#include <string>

#include <Buffer.h>

namespace pGina
{
	namespace NamedPipes
	{
		class PipeClient
		{
		public:
			PipeClient(std::wstring const& path);
			PipeClient(std::wstring const& path, int connectTimeout);
			~PipeClient();

			bool Connect();
			bool Connect(int timeout);

			pGina::Memory::Buffer * ReadBuffer();
			void WriteBuffer(pGina::Memory::Buffer *buffer);

			int Read(unsigned char * buffer, int len);
			int Write(unsigned char * buffer, int len);

			void Close();

		private:
			std::wstring m_path;
			int m_connectTimeout;
			HANDLE m_pipe;
		};
	}
}