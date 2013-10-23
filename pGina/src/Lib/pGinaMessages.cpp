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
						case DynLabelRequest:
							reply = (MessageBase *) (new DynamicLabelRequestMessage());
							break;
						case DynLabelResponse:
							reply = (MessageBase *) (new DynamicLabelResponseMessage());
							break;
						case UserInfoRequest:
							reply = (MessageBase *) (new UserInformationRequestMessage());
							break;
						case UserInfoResponse:
							reply = (MessageBase *) (new UserInformationResponseMessage());
							break;
						case ChangePasswordRequest:
							reply = (MessageBase *) (new ChangePasswordRequestMessage());
							break;
						case ChangePasswordResponse:
							reply = (MessageBase *) (new ChangePasswordResponseMessage());
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