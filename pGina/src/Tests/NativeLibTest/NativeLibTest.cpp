// NativeLibTest.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <pGinaNativeLib.h>

int _tmain(int argc, _TCHAR* argv[])
{		
	std::wstring pipeName = pGina::Registry::GetString(L"ServicePipeName", L"Unknown");
	std::wstring pipePath = L"\\\\.\\pipe\\";
	pipePath += pipeName;

	pGina::Messaging::Message * msg = new pGina::Messaging::Message();
	msg->Property<unsigned char>(L"MessageType", (unsigned char) 0x01, pGina::Messaging::Byte);
	msg->Property<std::wstring>(L"Username", L"Foo", pGina::Messaging::String);
	msg->Property<std::wstring>(L"Domain", L"", pGina::Messaging::String);
	
	pGina::Memory::Buffer * buffer = pGina::Messaging::Message::Marshal(msg);

	pGina::Messaging::Message * msg2 = pGina::Messaging::Message::Demarshal(*buffer);

	pGina::Memory::ObjectCleanupPool cleanup;
	pGina::NamedPipes::PipeClient pipeClient(pipePath, 1000);
	if(pipeClient.Connect())
	{
		// Always send hello first, expect hello in return
		pGina::Protocol::HelloMessage hello;
		pGina::Protocol::MessageBase * reply = pGina::Protocol::SendRecvPipeMessage(pipeClient, hello);		
		if(!reply)
			return -1;
		cleanup.Add(reply);

		if(reply->Type() != pGina::Protocol::Hello)
			return -1;
				
		// Then send a log message, expect ack in return
		pGina::Protocol::LogMessage log;
		log.Level(L"Debug");
		log.LoggerName(L"NativeTestApp");
		log.LoggedMessage(L"Test log message");
		reply = pGina::Protocol::SendRecvPipeMessage(pipeClient, log);

		if(!reply)
			return -1;
		cleanup.Add(reply);

		if(reply->Type() != pGina::Protocol::Ack)
			return -1;

		// Send disconnect, expect ack, then close
		pGina::Protocol::DisconnectMessage disconnect;
		reply = pGina::Protocol::SendRecvPipeMessage(pipeClient, disconnect);

		if(!reply)
			return -1;
		cleanup.Add(reply);		

		// We close regardless, no need to check reply type..
		pipeClient.Close();		
	}

	// Put it all together, use the transactions log:
	pGina::Transactions::Log::Debug(L"What the what? %s", L"What!");

	// And auth, all in one...
	if(pGina::Transactions::User::ProcessLoginForUser(L"puser", L"", L"foobar"))
	{
		pGina::Transactions::Log::Info(L"User: %s login successful!", pGina::Transactions::User::AuthenticatedUsername());
	}
	

	return 0;
}

