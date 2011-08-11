using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Dynamic;

namespace Abstractions.Pipes
{
    public static class PipeMessage
    {        
        // Every messages starts with a single byte version marker, which can be used
        //  to facilitate support for older clients if needed.  This value indicates
        //  the most recent version supported by this library.
        public const byte CurrentMessageFormatVersion = 0x01;
        
        private enum DataType
        {
            Byte            = 0x00,
            Integer         = 0x01,
            Boolean         = 0x02,
            String          = 0x03,            
        }
        
        public static dynamic Demarshal(byte[] data)
        {
            // Cannot be empty
            if (data.Length == 0)
                throw new InvalidDataException(string.Format("Empty message provided, invalid format."));
            
            using (BinaryReader br = new BinaryReader(new MemoryStream(data, false)))
            {
                // For now we support only the one message version
                byte messageFormatVersion = br.ReadByte();
                if (messageFormatVersion != CurrentMessageFormatVersion)
                    throw new InvalidDataException(string.Format("Message format: {0} is not support (library version: {1})", messageFormatVersion, CurrentMessageFormatVersion));

                // Create an expando object to represent this message
                dynamic message = new ExpandoObject();
                IDictionary<String, Object> messageDict = ((IDictionary<String, Object>)message);

                string propertyName = br.ReadString();
                DataType propertyType = (DataType) br.ReadByte();

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
                    default:
                        throw new InvalidDataException(string.Format("Message includes unknown data type: {0} for property: {1}", propertyType, propertyName));
                }

                return message;
            }
        }

        public static byte[] Marshal(dynamic message)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                BinaryWriter writer = new BinaryWriter(memory);
                
                // Write the version
                writer.Write(CurrentMessageFormatVersion);
    
                // Now we just iterate properties and write them out
                foreach (KeyValuePair<String, Object> property in (IDictionary<String, Object>) message)
                {                    
                    System.Type propType = property.GetType();

                    writer.Write(property.Key);

                    if (propType == typeof(int))
                    {
                        writer.Write((byte) DataType.Integer);                        
                        writer.Write((int)property.Value);
                    }
                    else if (propType == typeof(byte))
                    {                        
                        writer.Write((byte)DataType.Byte);
                        writer.Write((byte)property.Value);
                    }
                    else if (propType == typeof(bool))
                    {
                        writer.Write((byte)DataType.Boolean);
                        writer.Write((bool)property.Value);
                    }
                    else if(propType == typeof(Enum))
                    {
                        writer.Write((byte)DataType.Byte);
                        writer.Write((byte)property.Value);
                    }
                    else
                    {
                        // Not one of the former, you're string value it is then!
                        writer.Write((byte)DataType.String);
                        writer.Write(property.Value.ToString());
                    }
                }

                // Provide our caller a copy in byte[] format
                return memory.ToArray();
            }            
        }        
    }
}
