#include "PipeMessage.h"

namespace pGina
{
	namespace Pipes
	{
		Message::Message()
		{
		}

		Message::~Message()
		{
			for(PropertyMap::iterator itr = m_properties.begin(); itr != m_properties.end(); ++itr)
			{
				// Release the property object we own
				delete itr->second;
			}

			m_properties.clear();
		}
					
		int  Message::Integer(std::wstring const& propertyName)
		{
			return 0;
		}

		void Message::Integer(std::wstring const& propertyName, int value)
		{
		}

		std::wstring Message::String(std::wstring const& propertyName)
		{
			return L"";
		}

		void Message::String(std::wstring const& propertyName, std::wstring const& value)
		{
		}

		unsigned char Message::Byte(std::wstring const& propertyName)
		{
			return 0x00;
		}

		void          Message::Byte(std::wstring const& propertyName, unsigned char value)
		{
		}

		bool		  Message::Bool(std::wstring const& propertyName)
		{
			return false;
		}

		void		  Message::Bool(std::wstring const& propertyName, bool value)
		{
		}

		/* static */
		Message * Message::Demarshal(const char * buffer, int length)
		{
			return 0;
		}
		
		/* static */
		void      Message::Marshal(Message *, char **buffer, int& length)
		{
		}
	}
}