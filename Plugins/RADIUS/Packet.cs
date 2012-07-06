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
        public string sharedKey { get; set; }
        public Code code { get; private set; }
        public byte identifier { get; set; }
        public short length
        {
            get
            {
                return (short)(20 + avpBytes.Length);
            }
        }
        
        //authenticator changes based on the type of packet (authorization = random 16 octets, accounting = md5 of packet contents+sharedkey)
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


        public Packet(Code code, string sharedKey) : this(code, (byte)r.Next((int)Byte.MaxValue + 1), sharedKey) { }

        public Packet(Code code, byte identifier, string sharedKey)
        {
            this.code = code;
            this.identifier = identifier;
            this.sharedKey = sharedKey;

            this.authenticator = new byte[16];

            //Access-Requests use a random authentication code
            if (this.code == Code.Access_Request)
            {   
                //Generate secure bytes for authenticator
                RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
                rngCsp.GetBytes(this.authenticator);
            }
        }

        public Packet(Code code, byte identifier, byte[] auth)
        {   
            this.code = code;
            this.identifier = identifier;
            this.authenticator = auth;
        }

        public Packet(byte[] data)
        {
            code = (Code)data[0];
            identifier = data[1];
            //length = BitConverter.ToInt16(data, 2);
            authenticator = new byte[16];
            System.Buffer.BlockCopy(data, 4, authenticator, 0, 16);

            //AVP Values
            int index = 20; //First 20 bytes belong to the header above
            while (index < data.Length)
            {
                AttributeType atype = (AttributeType)data[index++];
                byte length = data[index++]; //length includes the type and length byte
                byte[] value = new byte[length - 2];
                System.Buffer.BlockCopy(data, index, value, 0, value.Length);
                avp.Add(atype, value);
                index += value.Length;
            }

        }

        //Adds the specified attribute type and corresponding string data to AVP list
        public void addAttribute(AttributeType type, string value)
        {
            if (String.IsNullOrEmpty(value))
                return; //Drop empty string value fields
            addRawAttribute(type, Encoding.UTF8.GetBytes(value));
        }

        //Adds the specified attribute type and corresponding int data to AVP list
        public void addAttribute(AttributeType type, int value)
        {
            addRawAttribute(type, BitConverter.GetBytes(value));
        }

        //Adds the specified attribute type and corresponding byte array data to AVP list
        public void addRawAttribute(AttributeType type, byte[] value)
        {
            //If it's a User_Password, we need to encrypt it
            if (type == AttributeType.User_Password)
                avp.Add(type, PAPPassword(value));
            else
                avp.Add(type, value);
            _avpBytes = null; //avp has been modified
        }

        //Returns a list of Attribute Types this packet contains
        public AttributeType[] getAttributeTypes()
        {
            return avp.Keys.ToArray();
        }

        //Returns the string value of the specified AttributeType
        public string getAttribute(AttributeType type)
        {
            return Encoding.UTF8.GetString(avp[type]);
        }

        //Returns the byte[] value of the specified AttributeType
        public byte[] getRawAttribute(AttributeType type)
        {
            return avp[type];
        }

        //Verifies the response authenticator comes from a legitimate source
        public bool verifyResponseAuthenticator(byte[] requestAuthenticator, string sharedKey)
        {
            //Accounting MD5(respcode+id+length+reqauth+respattr+sharedkey)
            
            //this.authenticator == MD5(Code+ID+Length+RequestAuth+Attributes+Secret)
            byte[] secretKeyBytes = Encoding.UTF8.GetBytes(sharedKey);
            byte[] verificationBytes = new byte[this.length + secretKeyBytes.Length];
            
            //Copy this packet to the buffer
            System.Buffer.BlockCopy(this.toBytes(), 0, verificationBytes, 0, this.length);

            //Copy secret key bytes to end of buffer
            System.Buffer.BlockCopy(secretKeyBytes, 0, verificationBytes, this.length, secretKeyBytes.Length);

            //Replace response authenticator with request authenticator (auth begins at byte 4)
            System.Buffer.BlockCopy(requestAuthenticator, 0, verificationBytes, 4, requestAuthenticator.Length);

            MD5 md5 = MD5.Create();
            byte[] hashedRespCode = md5.ComputeHash(verificationBytes);

            return equalByteArrays(hashedRespCode, this.authenticator);
        }

        //Returns a byte array representing this packet, generating a new authenticator value for accounting requests
        public byte[] toBytes()
        {
            if (this.code == Code.Accounting_Request)
            {
                //Clear out authenticator
                this.authenticator = new byte[16];

                //Create array consisting of packet data + shared key
                byte[] hashedPacket = new byte[this.length + sharedKey.Length];
                byte[] skey = Encoding.UTF8.GetBytes(sharedKey);
                System.Buffer.BlockCopy(this.convertToBytes(), 0, hashedPacket, 0, this.length);
                System.Buffer.BlockCopy(skey, 0, hashedPacket, this.length, skey.Length);
                
                //Set authenticator to MD5 of packet data + shared key
                this.authenticator = MD5.Create().ComputeHash(hashedPacket);
            }

            return convertToBytes();
        }
        

        //Returns a byte array representing this packet
        private byte[] convertToBytes()
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
                bytes[pos++] = (byte)(kvp.Value.Length + 2); //+2 for the type and length bytes
                System.Buffer.BlockCopy(kvp.Value, 0, bytes, pos, kvp.Value.Length);
                pos += kvp.Value.Length;
            }

            return bytes;
        } 

        //Encrypts the User-Password value
        private byte[] PAPPassword(byte[] password)
        {
            /* This implementation could be slightly more efficient.
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
            
            //Password should be broken into 16 octet chunks, with \0 terminating the additional bytes
            int passwordChunks = (password.Length / 16);
            if (password.Length % 16 != 0)
                passwordChunks += 1;

            
            byte[] extendedPassword = new byte[16 * passwordChunks];

            //Fill the last 16 bytes with \0
            for (int k = 16 * (passwordChunks - 1); k < extendedPassword.Length; k++)
                extendedPassword[k] = (byte)'\0';

            //Copy password over
            System.Buffer.BlockCopy(password, 0, extendedPassword, 0, password.Length);

            MD5 md5 = MD5.Create();
            byte[] c = new byte[extendedPassword.Length];

            //Create S+RA
            byte[] sra = new byte[secretKeyBytes.Length + authenticator.Length];
            System.Buffer.BlockCopy(secretKeyBytes, 0, sra, 0, secretKeyBytes.Length);
            System.Buffer.BlockCopy(authenticator, 0, sra, secretKeyBytes.Length, authenticator.Length);

            //b1
            byte[] bx = md5.ComputeHash(sra);

            //Cycle through each chunk
            for (int k = 0; k < passwordChunks; k++ )
            {
                //Compute Cx ^ bx
                for (int j = 0; j < 16; j++)
                    c[16*k + j] = (byte)(extendedPassword[16*k + j] ^ bx[j]);
                
                //S+Cx
                byte[] scx = new byte[secretKeyBytes.Length + 16];
                System.Buffer.BlockCopy(secretKeyBytes, 0, scx, 0, secretKeyBytes.Length); //S
                System.Buffer.BlockCopy(c, 16*k, scx, secretKeyBytes.Length, 16); //S+Cx
                //bx = MD5(S+Cx)
                bx = md5.ComputeHash(scx);

            }

            return c;
        }

        //Compares two byte arrays for equality, true if equal
        private bool equalByteArrays(byte[] arr1, byte[] arr2)
        {
            if (arr1.Length != arr2.Length) 
                return false;
            
            for (int k = 0; k < arr1.Length; k++)
                if (arr1[k] != arr2[k])
                    return false;
            return true;
        }

        public override string ToString()
        {

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

        public enum Acct_AuthenticType { Not_Specified = 0, RADIUS, Local, Remote }
        public enum Acct_Status_TypeType { Start = 1, Stop, Interim_Update }
        public enum Acct_Terminate_CauseType { User_Request = 1, Session_Timeout = 5}
    }
}
