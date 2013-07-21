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
			DynLabelRequest = 0x07,
			DynLabelResponse= 0x08,
			LoginInfoChange = 0x09,
			UserInfoRequest = 0x0a,
			UserInfoResponse = 0x0b,
			ChangePasswordRequest = 0x0c,
			ChangePasswordResponse = 0x0d,
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

			LogMessage(std::wstring const& name, std::wstring const& level, std::wstring const& message)
			{
				Type(Log);
				LoggerName(name);
				LoggedMessage(message);
				Level(level);
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

		class LoginRequestMessage : public MessageBase
		{
		public:
			enum LoginReason
			{
				Login = 0,
				Unlock,
				CredUI,
			};

			LoginRequestMessage()
			{
				Type(LoginRequest);
				SessionFromProcessId();
			}

			LoginRequestMessage(std::wstring const& username, std::wstring const& domain, std::wstring const& password, LoginReason reason)
			{
				Type(LoginRequest);
				Username(username);
				Domain(domain);
				Password(password);
				SessionFromProcessId();
				Reason(reason);
			}

			LoginRequestMessage(const wchar_t * username, const wchar_t * domain, const wchar_t *password, LoginReason reason)
			{
				Type(LoginRequest);
				Username(username ? username : L"");
				Domain(domain ? domain : L"");
				Password(password ? password : L"");
				SessionFromProcessId();
				Reason(reason);
			}

			std::wstring const& Username() { return m_username; }
			void				Username(std::wstring const& v) { m_username = v; }

			std::wstring const& Password() { return m_password; }
			void			    Password(std::wstring const& v) { m_password = v; }

			std::wstring const& Domain() { return m_domain; }
			void			    Domain(std::wstring const& v) { m_domain = v; }

			DWORD				Session() { return m_session; }
			void				Session(DWORD const& v) { m_session = v; }

			LoginReason			Reason() { return m_reason; }
			void				Reason(LoginReason v) { m_reason = v; }

			virtual void FromDynamicMessage(pGina::Messaging::Message * msg)
			{
				MessageBase::FromDynamicMessage(msg);

				if(msg->Exists<std::wstring>(L"Username"))
					Username(msg->Property<std::wstring>(L"Username"));

				if(msg->Exists<std::wstring>(L"Domain"))
					Domain(msg->Property<std::wstring>(L"Domain"));

				if(msg->Exists<std::wstring>(L"Password"))
					Password(msg->Property<std::wstring>(L"Password"));

				if(msg->Exists<int>(L"Session"))
					Session(msg->Property<int>(L"Session"));

				if(msg->Exists<unsigned char>(L"Reason"))
					Reason((LoginReason) msg->Property<unsigned char>(L"Reason"));
			}

			virtual pGina::Messaging::Message * ToDynamicMessage()
			{				
				pGina::Messaging::Message * msg = MessageBase::ToDynamicMessage();				
				msg->Property<std::wstring>(L"Username", Username(), pGina::Messaging::String);
				msg->Property<std::wstring>(L"Domain", Domain(), pGina::Messaging::String);
				msg->Property<std::wstring>(L"Password", Password(), pGina::Messaging::String);
				msg->Property<int>(L"Session", Session(), pGina::Messaging::Integer);
				msg->Property<unsigned char>(L"Reason", (unsigned char) (Reason()), pGina::Messaging::Byte);
				return msg;
			}

		protected:
			std::wstring m_username;
			std::wstring m_domain;
			std::wstring m_password;
			int m_session;
			LoginReason m_reason;

		protected:
			void SessionFromProcessId()
			{
				DWORD sessionId = -1;
				if(ProcessIdToSessionId(GetCurrentProcessId(), &sessionId))
					Session(sessionId);
			}
		};

		class LoginResponseMessage : public LoginRequestMessage
		{
			public:
				LoginResponseMessage()
				{
					Type(LoginResponse);
				}

				std::wstring Message() { return m_message; }
				void		 Message(std::wstring const& v) { m_message = v; }

				bool         Result() { return m_result; }
				void		 Result(bool v) { m_result = v; }

				virtual void FromDynamicMessage(pGina::Messaging::Message * msg)
				{
					LoginRequestMessage::FromDynamicMessage(msg);

					if(msg->Exists<std::wstring>(L"Message"))
						Message(msg->Property<std::wstring>(L"Message"));

					if(msg->Exists<bool>(L"Result"))
						Result(msg->Property<bool>(L"Result"));
				}

				virtual pGina::Messaging::Message * ToDynamicMessage()
				{				
					pGina::Messaging::Message * msg = LoginRequestMessage::ToDynamicMessage();				
					msg->Property<std::wstring>(L"Message", Message(), pGina::Messaging::String);
					msg->Property<bool>(L"Result", Result(), pGina::Messaging::Boolean);					
					return msg;
				}

			private:
				bool m_result;
				std::wstring m_message;
		};

		/* Request for text to be placed within a label in the UI. */
		class DynamicLabelRequestMessage : public MessageBase
		{
		public:
			DynamicLabelRequestMessage()
			{
				Type(DynLabelRequest);
			}

			DynamicLabelRequestMessage( std::wstring const& name )
			{
				Type(DynLabelRequest);
				m_name = name;
			}

			std::wstring const& Name() { return m_name; }
			void				Name(std::wstring const& v) { m_name = v; }

			virtual void FromDynamicMessage(pGina::Messaging::Message * msg)
			{
				MessageBase::FromDynamicMessage(msg);

				if(msg->Exists<std::wstring>(L"Name"))
					Name(msg->Property<std::wstring>(L"Name"));
			}

			virtual pGina::Messaging::Message * ToDynamicMessage()
			{				
				pGina::Messaging::Message * msg = MessageBase::ToDynamicMessage();				
				msg->Property<std::wstring>(L"Name", Name(), pGina::Messaging::String);
				return msg;
			}

		private:
			std::wstring m_name;
		};

		/* Response containing text for a label in the UI. */
		class DynamicLabelResponseMessage : public DynamicLabelRequestMessage
		{
		public:
			DynamicLabelResponseMessage()
			{
				Type(DynLabelResponse);
			}

			std::wstring const& Text() { return m_text; }
			void				Text(std::wstring const& v) { m_text = v; }

			virtual void FromDynamicMessage(pGina::Messaging::Message * msg)
			{
				DynamicLabelRequestMessage::FromDynamicMessage(msg);

				if(msg->Exists<std::wstring>(L"Text"))
					Text(msg->Property<std::wstring>(L"Text"));
			}

			virtual pGina::Messaging::Message * ToDynamicMessage()
			{				
				pGina::Messaging::Message * msg = DynamicLabelRequestMessage::ToDynamicMessage();				
				msg->Property<std::wstring>(L"Text", Text(), pGina::Messaging::String);
				return msg;
			}

		private:
			std::wstring m_text;
		};


		/* Request for user information based on session ID. */
		class UserInformationRequestMessage : public MessageBase
		{
		public:
			UserInformationRequestMessage() 
			{
				Type(UserInfoRequest);
				SessionID(-1);
			}

			UserInformationRequestMessage( int sess_id )
			{
				Type(UserInfoRequest);
				m_sessid = sess_id;
			}

			int const& SessionID() { return m_sessid; }
			void SessionID(int v) { m_sessid = v; }

			virtual void FromDynamicMessage(pGina::Messaging::Message * msg)
			{
				MessageBase::FromDynamicMessage(msg);

				if(msg->Exists<int>(L"SessionID"))
					SessionID(msg->Property<int>(L"SessionID"));
			}

			virtual pGina::Messaging::Message * ToDynamicMessage()
			{				
				pGina::Messaging::Message * msg = MessageBase::ToDynamicMessage();				
				msg->Property<int>(L"SessionID", SessionID(), pGina::Messaging::Integer);
				return msg;
			}

		private:
			int m_sessid;
		};

		/* Response containing user information. */
		class UserInformationResponseMessage : public UserInformationRequestMessage
		{
		public:
			UserInformationResponseMessage()
			{
				Type(UserInfoResponse);
			}

			std::wstring const& OriginalUsername() { return m_orig_uname; }
			void	OriginalUsername(std::wstring const& v) { m_orig_uname = v; }

			std::wstring const& Username() { return m_uname; }
			void	Username(std::wstring const& v) { m_uname = v; }

			std::wstring const& Domain() { return m_domain; }
			void	Domain(std::wstring const& v) { m_domain = v; }

			virtual void FromDynamicMessage(pGina::Messaging::Message * msg)
			{
				UserInformationRequestMessage::FromDynamicMessage(msg);

				if(msg->Exists<std::wstring>(L"OriginalUsername"))
					OriginalUsername(msg->Property<std::wstring>(L"OriginalUsername"));
				if(msg->Exists<std::wstring>(L"Username"))
					Username(msg->Property<std::wstring>(L"Username"));
				if(msg->Exists<std::wstring>(L"Domain"))
					Domain(msg->Property<std::wstring>(L"Domain"));
			}

			virtual pGina::Messaging::Message * ToDynamicMessage()
			{				
				pGina::Messaging::Message * msg = UserInformationRequestMessage::ToDynamicMessage();				
				msg->Property<std::wstring>(L"OriginalUsername", OriginalUsername(), pGina::Messaging::String);
				msg->Property<std::wstring>(L"Username", Username(), pGina::Messaging::String);
				msg->Property<std::wstring>(L"Domain", Domain(), pGina::Messaging::String);
				return msg;
			}

		private:
			std::wstring m_orig_uname;
			std::wstring m_uname;
			std::wstring m_domain;
		};


		class LoginInfoChangeMessage : public LoginRequestMessage
		{
			public:				
				LoginInfoChangeMessage() :
				  m_fromSession(0),
				  m_toSession(0)
				{
					Type(LoginInfoChange);
				}

				LoginInfoChangeMessage(const wchar_t * username, const wchar_t * domain, const wchar_t *password) :
					LoginRequestMessage(username, domain, password, Login),
					m_fromSession(0),
				    m_toSession(0)
				{
					Type(LoginInfoChange);					
				}
				
				int		FromSession() { return m_fromSession; }
				void	FromSession(int v) { m_fromSession = v; }
				int		ToSession() { return m_toSession; }
				void	ToSession(int v) { m_toSession = v; }

				virtual void FromDynamicMessage(pGina::Messaging::Message * msg)
				{
					LoginRequestMessage::FromDynamicMessage(msg);

					if(msg->Exists<int>(L"FromSession"))
						FromSession(msg->Property<int>(L"FromSession"));
					if(msg->Exists<int>(L"ToSession"))
						ToSession(msg->Property<int>(L"ToSession"));
				}

				virtual pGina::Messaging::Message * ToDynamicMessage()
				{				
					pGina::Messaging::Message * msg = LoginRequestMessage::ToDynamicMessage();				
					msg->Property<int>(L"ToSession", ToSession(), pGina::Messaging::Integer);
					msg->Property<int>(L"FromSession", FromSession(), pGina::Messaging::Integer);
					return msg;
				}

			private:
				int m_fromSession;
				int m_toSession;
		};

		class ChangePasswordRequestMessage : public MessageBase
		{
		public:
			ChangePasswordRequestMessage() 
			{
				Type(ChangePasswordRequest);
				Username(L"");
				Domain(L"");
				OldPassword(L"");
				NewPassword(L"");
			}

			ChangePasswordRequestMessage(std::wstring const& username, std::wstring const& domain, 
				std::wstring const& oldPassword, std::wstring const& newPassword)
			{
				Type(ChangePasswordRequest);
				Username(username);
				Domain(domain);
				OldPassword(oldPassword);
				NewPassword(newPassword);
			}

			std::wstring const& Username() { return m_username; }
			void				Username(std::wstring const& v) { m_username = v; }

			std::wstring const& OldPassword() { return m_oldPassword; }
			void			    OldPassword(std::wstring const& v) { m_oldPassword = v; }

			std::wstring const& NewPassword() { return m_newPassword; }
			void			    NewPassword(std::wstring const& v) { m_newPassword = v; }

			std::wstring const& Domain() { return m_domain; }
			void			    Domain(std::wstring const& v) { m_domain = v; }

			virtual void FromDynamicMessage(pGina::Messaging::Message * msg)
			{
				MessageBase::FromDynamicMessage(msg);

				if(msg->Exists<std::wstring>(L"Username"))
					Username(msg->Property<std::wstring>(L"Username"));

				if(msg->Exists<std::wstring>(L"Domain"))
					Domain(msg->Property<std::wstring>(L"Domain"));

				if(msg->Exists<std::wstring>(L"OldPassword"))
					OldPassword(msg->Property<std::wstring>(L"OldPassword"));

				if(msg->Exists<std::wstring>(L"NewPassword"))
					OldPassword(msg->Property<std::wstring>(L"NewPassword"));
			}

			virtual pGina::Messaging::Message * ToDynamicMessage()
			{				
				pGina::Messaging::Message * msg = MessageBase::ToDynamicMessage();				
				msg->Property<std::wstring>(L"Username", Username(), pGina::Messaging::String);
				msg->Property<std::wstring>(L"Domain", Domain(), pGina::Messaging::String);
				msg->Property<std::wstring>(L"OldPassword", OldPassword(), pGina::Messaging::String);
				msg->Property<std::wstring>(L"NewPassword", NewPassword(), pGina::Messaging::String);
				return msg;
			}

		protected:
			std::wstring m_username;
			std::wstring m_domain;
			std::wstring m_oldPassword;
			std::wstring m_newPassword;
		};

		class ChangePasswordResponseMessage : public ChangePasswordRequestMessage
		{
			public:
				ChangePasswordResponseMessage()
				{
					Type(ChangePasswordResponse);
				}

				std::wstring Message() { return m_message; }
				void		 Message(std::wstring const& v) { m_message = v; }

				bool         Result() { return m_result; }
				void		 Result(bool v) { m_result = v; }

				virtual void FromDynamicMessage(pGina::Messaging::Message * msg)
				{
					ChangePasswordRequestMessage::FromDynamicMessage(msg);

					if(msg->Exists<std::wstring>(L"Message"))
						Message(msg->Property<std::wstring>(L"Message"));

					if(msg->Exists<bool>(L"Result"))
						Result(msg->Property<bool>(L"Result"));
				}

				virtual pGina::Messaging::Message * ToDynamicMessage()
				{				
					pGina::Messaging::Message * msg = ChangePasswordRequestMessage::ToDynamicMessage();				
					msg->Property<std::wstring>(L"Message", Message(), pGina::Messaging::String);
					msg->Property<bool>(L"Result", Result(), pGina::Messaging::Boolean);					
					return msg;
				}

			private:
				bool m_result;
				std::wstring m_message;
		};
	}
}