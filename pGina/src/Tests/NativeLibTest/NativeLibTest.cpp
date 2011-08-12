// NativeLibTest.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <pGinaNativeLib.h>

int _tmain(int argc, _TCHAR* argv[])
{		
	std::wstring pipeName = pGina::Registry::GetString(L"ServicePipeName", L"Unknown");

	pGina::Messaging::Message * msg = new pGina::Messaging::Message();
	msg->Property<unsigned char>(L"MessageType", (unsigned char) 0x01, pGina::Messaging::Byte);
	msg->Property<std::wstring>(L"Username", L"Foo", pGina::Messaging::String);
	msg->Property<std::wstring>(L"Domain", L"", pGina::Messaging::String);
	
	pGina::Memory::Buffer * buffer = pGina::Messaging::Message::Marshal(msg);

	pGina::Messaging::Message * msg2 = pGina::Messaging::Message::Demarshal(*buffer);

	return 0;
}

