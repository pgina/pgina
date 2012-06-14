using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace pGina.Plugin.RADIUS
{
    //Assumes 8 bits = 1 byte
    class Packet
    {

        public enum Code
        {
            Access_Request = (byte)1, Access_Accept, Access_Reject, Accounting_Request, Accounting_Response
        };

        public enum AttributeType
        {
            User_Name = (byte)1, User_Password, CHAP_Password, NAS_IP_Address, NAS_Port, 
            Service_Type, Framed_Protocol, Framed_IP_Address, Framed_IP_Netmask, Framed_Routing,
            Filter_Id, Framed_MTU, Framed_Compression, Login_IP_Host, Login_Service,
            Login_TCP_Port, UNASSIGNED_17, Reply_Message, Callback_Number, Callback_Id,
            UNASSIGNED_21, Framed_Route, Framed_IPX_Network, State, Class,
            Vendor_Specific, Session_Timeout, Idle_Timeout, Termination_Action, Called_Station_Id,
            Calling_Station_Id, NAS_Identifier, Proxy_State, Login_LAT_Service, Login_LAT_Node,
            Login_LAT_Group, Framed_AppleTalk_Link, Framed_AppleTalk_Network, Framed_AppletTalk_Zone, Acct_Status_Type,
            Acct_Delay_Time, Acct_Input_Octets, Acct_Output_Octets, Acct_Session_Id, Acct_Authentic,
            Acct_Session_Time, Acct_Input_Packets, Acct_Output_Packets, Acct_Terminate_Cause, Acct_Multi_Session_Id,
            Acct_Link_Count, UNASSIGNED_52, UNASSIGNED_53, UNASSIGNED_54, UNASSIGNED_55,
            UNASSIGNED_56, UNASSIGNED_57, UNASSIGNED_58, UNASSIGNED_59, CHAP_Challenge, 
            NAS_Port_Type, Port_Limit, Login_LAT_Port
        };

        public string sharedKey { get; set; }
        public Code code { get; private set; }
        public byte identifier { get; private set; } 
        public short length {
            get
            {
                return (short)(20 + avpBytes.Length);
            }
        }
        public byte[] authenticator { get; private set; }

        
        private Dictionary<AttributeType, byte[]> avp = new Dictionary<AttributeType, byte[]>();
        private byte[] _avpBytes = null; //Byte representation of dictionary; required to calculate length
        private byte[] avpBytes //Returns a current byte[] representation of AVP dictionary
        {
            get
            {
                byte[] b = _avpBytes ?? avpToByteArray();
                _avpBytes = b;
                return b;
            }
        }

        private static Random r = new Random();


        public Packet(Code code) : this(code, (byte) r.Next((int)Byte.MaxValue + 1)){ }

        public Packet(Code code, byte identifier)
        {
            this.code = code;
            this.identifier = identifier;

            this.authenticator = new byte[16];
            
            //Generate secure bytes for authenticator
            RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
            rngCsp.GetBytes(this.authenticator);
        }

        public Packet(Code code, byte identifier, byte[] auth)
        {
            this.code = code;
            this.identifier = identifier;
            this.authenticator = auth;
        }

        public Packet(byte[] data)
        {
            code = (Code) data[0];
            identifier = data[1];
            //length = BitConverter.ToInt16(data, 2);
            authenticator = new byte[16];
            System.Buffer.BlockCopy(data, 4, authenticator, 0, 16);

            //AVP Values
            int index = 20; //First 20 bytes belong to the header above
            while(index < data.Length){
                AttributeType atype = (AttributeType)data[index++];
                byte length = data[index++]; //length includes the type and length byte
                byte[] value = new byte[length-2];
                System.Buffer.BlockCopy(data, index, value, 0, value.Length);
                avp.Add(atype, value);
                index += value.Length;
            }

        }

        public void addAttribute(AttributeType type, string value)
        {
            addRawAttribute(type, Encoding.UTF8.GetBytes(value));
        }

        public void addRawAttribute(AttributeType type, byte[] value)
        {
            //If it's a User_Password, we need to encrypt it
            if (type == AttributeType.User_Password)
                avp.Add(type, PAPPassword(value));
            else
                avp.Add(type, value);
            _avpBytes = null; //avp has been modified
        }

        public AttributeType[] getAttributeTypes()
        {
            return avp.Keys.ToArray();
        }
        
        public string getAttribute(AttributeType type)
        {
            return Encoding.UTF8.GetString(avp[type]);
        }

        public byte[] getRawAttribute(AttributeType type)
        {
            return avp[type];
        }

        //Returns a byte array representing this packet
        public byte[] toBytes()
        {
            //1 byte for code, 1 byte for identifier, 2 bytes for length, 16 bytes for auth + AVP
            byte[] avpBytes = this.avpBytes;
            byte[] packetBytes = new byte[20 + avpBytes.Length];
            
            byte[] lengthBytes = BitConverter.GetBytes(length);

            packetBytes[0] = (byte)code;
            packetBytes[1] = identifier;
            packetBytes[2] = lengthBytes[1]; //Endian stupidness
            packetBytes[3] = lengthBytes[0];

            System.Buffer.BlockCopy(authenticator, 0, packetBytes, 4, authenticator.Length);

            System.Buffer.BlockCopy(avpBytes, 0, packetBytes, 20, avpBytes.Length);

            return packetBytes;
        }

        //Returns a byte array representing the AVP
        private byte[] avpToByteArray()
        {
            //Figure out total length of data
            int avpLength = 0;
            foreach (byte[] data in avp.Values)
            {
                avpLength += data.Length;
            }

            //Total size is 2 bytes per entry + the length of all the data
            byte[] bytes = new byte[2 * avp.Count() + avpLength];

            //Copy data in kvp to array as bytes
            int pos = 0;
            foreach (KeyValuePair<AttributeType, byte[]> kvp in avp)
            {
                //First byte = type, secondbyte = length of data, than data
                bytes[pos++] = (byte)kvp.Key;
                bytes[pos++] = (byte)(kvp.Value.Length+2); //+2 for the type and length bytes
                System.Buffer.BlockCopy(kvp.Value, 0, bytes, pos, kvp.Value.Length);
                pos += kvp.Value.Length;
            }

            return bytes;
        }

        //Encrypts the User-Password value
        private byte[] PAPPassword(byte[] password)
        {
            /* This is messy as hell and needs to be cleaned up.
             Call the shared secret S and the pseudo-random 128-bit Request
             Authenticator RA.  Break the password into 16-octet chunks p1, p2,
             etc.  with the last one padded at the end with nulls to a 16-octet
             boundary.  Call the ciphertext blocks c(1), c(2), etc.  We'll need
             intermediate values b1, b2, etc.

             b1 = MD5(S + RA)       c(1) = p1 xor b1
             b2 = MD5(S + c(1))     c(2) = p2 xor b2
                    .                       .
                    .                       .
                    .                       .
             bi = MD5(S + c(i-1))   c(i) = pi xor bi

            The String will contain c(1)+c(2)+...+c(i) where + denotes
            concatenation.
            */
            
            byte[] secretKeyBytes = Encoding.UTF8.GetBytes(sharedKey); //Secret key as bytes
            byte[] passBytes = password; //Encoding.UTF8.GetBytes(password); //Password as bytes
            
            int passwordChunks = (passBytes.Length / 16) + 1;
            byte[] extendedPassBytes = new byte[16 * passwordChunks];

            //Fill the last 16 bytes with \0
            for (int k = 16*(passwordChunks-1); k < extendedPassBytes.Length; k++)
                extendedPassBytes[k] = (byte)'\0';

            Console.WriteLine("Password Bytes: {0}, Encrypted bytes: {1}", password.Length, extendedPassBytes.Length);

            //Copy pass over to null-filled bytes
            System.Buffer.BlockCopy(passBytes, 0, extendedPassBytes, 0, passBytes.Length);

            //Will store the encrypted password here
            byte[] encryptedPass = new byte[16 * passwordChunks];

            //Create S+RA
            byte[] bx = new byte[secretKeyBytes.Length + authenticator.Length];
            System.Buffer.BlockCopy(secretKeyBytes, 0, bx, 0, secretKeyBytes.Length);
            System.Buffer.BlockCopy(authenticator, 0, bx, secretKeyBytes.Length, authenticator.Length);

            MD5 md5 = MD5.Create();

            //Cycle through each 16 byte chunk
            for (int k = 0; k < extendedPassBytes.Length; k += 16)
            {
                //Compute the MD5 hash for b1 ... bx
                bx = md5.ComputeHash(bx);
                                
                //Computer passSeg XOR bx, storing in encryptedPass
                for (int j = 0; j < 16; j++)
                {
                    encryptedPass[k + j] = (byte)(extendedPassBytes[k+j] ^ bx[j]);
                }

                //Create S+Cx (16 bytes for each one)
                bx = new byte[32];
                System.Buffer.BlockCopy(secretKeyBytes, 0, bx, 0, secretKeyBytes.Length);
                System.Buffer.BlockCopy(encryptedPass, k, bx, secretKeyBytes.Length, 16);

            }

            return encryptedPass;
        }
    
        
        public override string ToString(){

            string init = String.Format("Code: {0}\tID: {1}\tLength: {2}\nRequest Authenticator: {3}\n", code, identifier, length, authenticator);
            StringBuilder builder = new StringBuilder(init);

            if (avp.Count > 0)
            {
                builder.Append("Attributes:\n");
                foreach (KeyValuePair<AttributeType, byte[]> kvp in avp)
                {
                    if (kvp.Key == AttributeType.NAS_IP_Address)
                        builder.AppendFormat("\t{0} : Length: {1}\t{2}.{3}.{4}.{5}\n", kvp.Key, kvp.Value.Length + 2, kvp.Value[0], kvp.Value[1], kvp.Value[2], kvp.Value[3]);
                    else if (kvp.Key == AttributeType.User_Password)
                        builder.AppendFormat("\t{0} : Length: {1} \tEncrypted to {2} bytes.\n", kvp.Key, kvp.Value.Length + 2, kvp.Value.Length);
                    else
                        builder.AppendFormat("\t{0}: Length: {1} \t{2}\n", kvp.Key, kvp.Value.Length + 2, Encoding.UTF8.GetString(kvp.Value));
                }
            }
            else
            {
                builder.Append("No attributes.");
            }

            return builder.ToString();
        }
    }
}
