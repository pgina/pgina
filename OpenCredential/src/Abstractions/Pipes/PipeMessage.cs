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
            EmptyString     = 0x04,
        }

        public static IDictionary<string, object> Demarshal(byte[] data)
        {
            // Cannot be empty
            if (data.Length == 0)
                throw new InvalidDataException(string.Format("Empty message provided, invalid format."));
            
            using (BinaryReader br = new BinaryReader(new MemoryStream(data, false), Encoding.Unicode))
            {
                // For now we support only the one message version
                byte messageFormatVersion = br.ReadByte();
                if (messageFormatVersion != CurrentMessageFormatVersion)
                    throw new InvalidDataException(string.Format("Message format: {0} is not support (library version: {1})", messageFormatVersion, CurrentMessageFormatVersion));

                // Create a dict for the message contents                
                Dictionary<string, object> messageDict = new Dictionary<string, object>();

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

                return messageDict;
            }
        }

        public static byte[] Marshal(IDictionary<string, object> message)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                BinaryWriter writer = new BinaryWriter(memory, Encoding.Unicode);
                
                // Write the version
                writer.Write(CurrentMessageFormatVersion);
    
                // Now we just iterate properties and write them out
                foreach (KeyValuePair<string, object> property in message)
                {
                    writer.Write(property.Key);

                    // Null values are treated as empty strings
                    if (property.Value == null)
                    {
                        writer.Write((byte)DataType.EmptyString);
                        continue;
                    }

                    System.Type propType = property.Value.GetType();                    
                    
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
