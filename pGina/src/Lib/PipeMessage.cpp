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
		Message * Message::Demarshal(pGina::Memory::Buffer &buffer)
		{
			// Cannot be empty
			if(buffer.Raw() == 0 || buffer.Length() == 0)
				return 0;

			/*			
            using (BinaryReader br = new BinaryReader(new MemoryStream(data, false)))
            {
                // For now we support only the one message version
                byte messageFormatVersion = br.ReadByte();
                if (messageFormatVersion != CurrentMessageFormatVersion)
                    throw new InvalidDataException(string.Format("Message format: {0} is not support (library version: {1})", messageFormatVersion, CurrentMessageFormatVersion));

                // Create an expando object to represent this message
                dynamic message = new ExpandoObject();
                IDictionary<String, Object> messageDict = ((IDictionary<String, Object>)message);

                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    string propertyName = br.ReadString();
                    DataType propertyType = (DataType)br.ReadByte();

                    switch (propertyType)
                    {
                        case DataType.Boolean:
                            messageDict.Add(propertyName, br.ReadBoolean());
                            break;
                        case DataType.Byte:
                            messageDict.Add(propertyName, br.ReadByte());
                            break;
                        case DataType.Integer:
                            messageDict.Add(propertyName, br.ReadInt32());
                            break;
                        case DataType.String:
                            messageDict.Add(propertyName, br.ReadString());
                            break;
                        case DataType.EmptyString:
                            messageDict.Add(propertyName, (string) null);
                            break;
                        default:
                            throw new InvalidDataException(string.Format("Message includes unknown data type: {0} for property: {1}", propertyType, propertyName));
                    }
                }

                return message;
            }
			*/
			return 0;
		}
		
		/* static */
		pGina::Memory::Buffer *  Message::Marshal(Message *)
		{
			return 0;
		}
	}
}