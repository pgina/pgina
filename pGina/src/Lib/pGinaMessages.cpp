#include "pGinaMessages.h"

namespace pGina
{
	namespace Protocol
	{						
		MessageBase * SendRecvPipeMessage(pGina::NamedPipes::PipeClient &client, MessageBase &msg)
		{
			return SendRecvPipeMessage(client, &msg);
		}

		MessageBase * SendRecvPipeMessage(pGina::NamedPipes::PipeClient &client, MessageBase *msg)
		{
			MessageBase * reply = 0;
			pGina::Messaging::Message * dynamicMsg = msg->ToDynamicMessage();			
			pGina::Memory::Buffer * msgBuffer = pGina::Messaging::Message::Marshal(dynamicMsg);

			if(client.WriteLengthEncodedBuffer(msgBuffer))
			{
				pGina::Memory::Buffer * replyBuffer = client.ReadLengthEncodedBuffer();
				if(replyBuffer)
				{
					pGina::Messaging::Message * replyMsg = pGina::Messaging::Message::Demarshal(replyBuffer);

					if(replyMsg->Exists<unsigned char>(L"MessageType"))
					{
						MessageType type = (MessageType) replyMsg->Property<unsigned char>(L"MessageType");
						switch(type)
						{
						case Hello:							
							reply = (MessageBase *) (new HelloMessage());
							break;
						case Disconnect:
							reply = (MessageBase *) (new DisconnectMessage());
							break;
						case Ack:
							reply = (MessageBase *) (new AckMessage());
							break;
						case Log:
							reply = (MessageBase *) (new LogMessage());
							break;
						case LoginRequest:
							reply = (MessageBase *) (new LoginRequestMessage());
							break;
						case LoginResponse:
							reply = (MessageBase *) (new LoginResponseMessage());
							break;
						}
					}

					if(reply)
					{
						// Decode any further data needed
						reply->FromDynamicMessage(replyMsg);
					}

					delete replyMsg;
					delete replyBuffer;
				}
			}

			delete msgBuffer;
			delete dynamicMsg;

			return reply;		
		}
	}
}