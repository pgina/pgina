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

#include "stdafx.h"
#include <pGinaNativeLib.h>

class ThreadTest : public pGina::Threading::Thread
{
	virtual DWORD ThreadMain()
	{
		while(Running())
		{
			printf("Hello world from the background thread...\n");
			Sleep(1000);
		}

		return 0;
	}
};

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

	// Stress test, lots of logging!
	for(int x = 0; x < 100; x++)
		pGina::Transactions::Log::Debug(L"Log test: %d", x);

	// And auth, all in one...
	pGina::Transactions::User::LoginResult result = pGina::Transactions::User::ProcessLoginForUser(L"footle", L"", L"foo", pGina::Protocol::LoginRequestMessage::Login);
	if(result.Result())
	{
		pGina::Transactions::Log::Info(L"User: %s login successful!", result.Username().c_str());
	}
	
	std::wstring str = pGina::Transactions::TileUi::GetDynamicLabel(L"MOTD");
	pGina::Transactions::Log::Info(L"TileUi::GetDynamicLabel(\"MOTD\") received: %s", str.c_str());
	
	ThreadTest ping;
	ping.Start();
	Sleep(10000);
	ping.Stop();
	return 0;
}

