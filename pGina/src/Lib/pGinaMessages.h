#pragma once

#include <Windows.h>
#include <string>

#include <Message.h>
#include <PipeClient.h>

namespace pGina
{
	namespace Protocol
	{
		typedef enum MessageType
		{
			Unknown         = 0x00,
			Hello           = 0x01,
			Disconnect      = 0x02,
			Ack             = 0x03,
			Log             = 0x04,
			LoginRequest    = 0x05,
			LoginResponse   = 0x06,
		};
				
		class MessageBase 
		{
		public:			
			virtual void FromDynamicMessage(pGina::Messaging::Message * msg)
			{
				if(msg->Exists<unsigned char>(L"MessageType"))
				{
					Type(static_cast<MessageType>(msg->Property<unsigned char>(L"MessageType")));
				}
			}

			virtual pGina::Messaging::Message * ToDynamicMessage()
			{				
				pGina::Messaging::Message * msg = new pGina::Messaging::Message();
				msg->Property<unsigned char>(L"MessageType", (unsigned char) Type(), pGina::Messaging::Byte);
				return msg;
			}

			MessageType Type() { return m_type; }
			void        Type(MessageType t) { m_type = t; }
			
		protected:
			MessageType m_type;
		};

		MessageBase * SendRecvPipeMessage(pGina::NamedPipes::PipeClient &client, MessageBase *msg);		
		MessageBase * SendRecvPipeMessage(pGina::NamedPipes::PipeClient &client, MessageBase &msg);		

		class HelloMessage : public MessageBase
		{
		public:
			HelloMessage()
			{
				Type(Hello);
			}
		};
		
		class  DisconnectMessage : public MessageBase
		{
		public:
			DisconnectMessage()
			{
				Type(Disconnect);
			}
		};

		class  AckMessage : public MessageBase
		{
		public:
			AckMessage()
			{
				Type(Ack);
			}
		};
		
		class LogMessage : public MessageBase
		{
		public:
			LogMessage()
			{
				Type(Log);
			}

			std::wstring const& LoggerName() { return m_loggerName; }
			void				LoggerName(std::wstring const& v) { m_loggerName = v; }

			std::wstring const& LoggedMessage() { return m_loggedMessage; }
			void			    LoggedMessage(std::wstring const& v) { m_loggedMessage = v; }

			std::wstring const& Level() { return m_level; }
			void			    Level(std::wstring const& v) { m_level = v; }

			virtual void FromDynamicMessage(pGina::Messaging::Message * msg)
			{
				MessageBase::FromDynamicMessage(msg);

				if(msg->Exists<std::wstring>(L"LoggerName"))
					LoggerName(msg->Property<std::wstring>(L"LoggerName"));

				if(msg->Exists<std::wstring>(L"LoggedMessage"))
					LoggedMessage(msg->Property<std::wstring>(L"LoggedMessage"));

				if(msg->Exists<std::wstring>(L"Level"))
					Level(msg->Property<std::wstring>(L"Level"));
			}

			virtual pGina::Messaging::Message * ToDynamicMessage()
			{				
				pGina::Messaging::Message * msg = MessageBase::ToDynamicMessage();				
				msg->Property<std::wstring>(L"LoggerName", LoggerName(), pGina::Messaging::String);
				msg->Property<std::wstring>(L"LoggedMessage", LoggedMessage(), pGina::Messaging::String);
				msg->Property<std::wstring>(L"Level", Level(), pGina::Messaging::String);
				return msg;
			}

		private:
			std::wstring m_loggerName;
			std::wstring m_loggedMessage;
			std::wstring m_level;
		};

		class LoginRequestMessage : MessageBase
		{
		};

		class LoginResponseMessage : LoginRequestMessage
		{
		};
	}
}