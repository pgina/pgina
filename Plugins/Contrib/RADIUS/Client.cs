using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

using log4net;

/* This class conforms to the following RFCs:
 * RFC 2865 - Remote Authentication Dial In User Service (RADIUS) - http://tools.ietf.org/html/rfc2865
 * RFC 2866 - RADIUS Accounting - http://tools.ietf.org/html/rfc2866
 */

namespace pGina.Plugin.RADIUS
{
    class RADIUSClient
    {
        public string[] servers { get; set; }
        public int authenticationPort { get; set; }
        public int accountingPort { get; set; }
        public string sharedKey { get; set; } //Private shared key for server
        public int timeout { get; set; } //timeout in ms
        public int maxRetries { get; set; } //Number of times to retry sending packet
        
        public string sessionId { get; set; } //SessionId is required for accounting
        public Packet lastReceievedPacket { get; private set; } //Last packet received from server
        public bool authenticated { get; private set; } //Whether username was successfully authenticated

        
        public byte[] NAS_IP_Address { get; set; } 
        public string NAS_Identifier { get; set; }

        //identifier refers to the identifier number, unique for each new packet
        private byte _id;
        public byte identifier
        {
            get{ return _id++; }
            set { _id = value; }
        }

        public string ipAddressRegex { get; set; }
        
        private static Random r = new Random();
        private ILog m_logger = LogManager.GetLogger("RADIUSPlugin");


        public RADIUSClient(string[] servers, int authport, int acctingport, string sharedKey, string NAS_Id) :
            this(servers, authport, acctingport, sharedKey, timeout: 3000, retry: 3, sessionId:null, NAS_IP_Address:null, NAS_Identifier: NAS_Id)
        {
        }

        public RADIUSClient(string[] servers, int authport, int acctingport, string sharedKey, string sessionId, string NAS_Id) :
            this(servers, authport, acctingport, sharedKey, timeout: 3000, retry: 3, sessionId: sessionId, NAS_IP_Address: null, NAS_Identifier: NAS_Id)
        {
        }

        public RADIUSClient(string[] servers, int authport, int acctingport, string sharedKey,
            int timeout, int retry, string sessionId, byte[] NAS_IP_Address, string NAS_Identifier)
        {
            this.servers = servers;
            this.authenticationPort = authport;
            this.accountingPort = acctingport;
            this.sharedKey = sharedKey;
            this.timeout = timeout;
            this.maxRetries = retry;
            this.identifier = (byte)r.Next(Byte.MaxValue + 1);
            this.sessionId = sessionId;
            this.authenticated = false;

            this.NAS_IP_Address = NAS_IP_Address;
            this.NAS_Identifier = NAS_Identifier;
        }

        //Connects to the RADIUS server and attempts to authenticate the specified user info
        //Sets username value and authenticated members IFF successful
        public bool Authenticate(string username, string password)
        {
            Packet authPacket = new Packet(Packet.Code.Access_Request, identifier, sharedKey);
            authPacket.sharedKey = sharedKey;
            
            authPacket.addAttribute(Packet.AttributeType.User_Name, username);
                        
            authPacket.addAttribute(Packet.AttributeType.User_Password, password);
            if(!String.IsNullOrEmpty(sessionId))
                authPacket.addAttribute(Packet.AttributeType.Acct_Session_Id, sessionId);

            if (NAS_Identifier == null && NAS_IP_Address == null)
                throw new RADIUSException("A NAS_Identifier or NAS_IP_Address (or both) must be supplied.");
            if(NAS_IP_Address != null)
                authPacket.addRawAttribute(Packet.AttributeType.NAS_IP_Address, NAS_IP_Address);
            if (NAS_Identifier != null)
                authPacket.addAttribute(Packet.AttributeType.NAS_Identifier, NAS_Identifier);

            m_logger.DebugFormat("Attempting to send {0} for user {1}", authPacket.code, username);

            for (int retryCt = 0; retryCt <= maxRetries; retryCt++)
            {
                foreach (string server in servers)
                {
                    UdpClient client = new UdpClient(server, authenticationPort);
                    client.Client.SendTimeout = timeout;
                    client.Client.ReceiveTimeout = timeout;

                    try
                    {
                        client.Send(authPacket.toBytes(), authPacket.length);

                        //Listen for response, since the server has been specified, we don't need to re-specify server
                        IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                        byte[] respBytes = client.Receive(ref RemoteIpEndPoint);
                        Packet responsePacket = new Packet(respBytes);

                        //Verify packet authenticator is correct
                        if (!responsePacket.verifyResponseAuthenticator(authPacket.authenticator, sharedKey))
                            throw new RADIUSException(String.Format("Received response to authentication with code: {0}, but an incorrect response authenticator was supplied.", responsePacket.code));

                        lastReceievedPacket = responsePacket;

                        client.Close();

                        m_logger.DebugFormat("Received authentication response: {0} for user {1}", responsePacket.code, username);

                        if (responsePacket.code == Packet.Code.Access_Accept)
                        {
                            this.authenticated = true;
                            return true;
                        }

                        else
                            return false;
                    }

                    //SocketException is thrown if the  server does not respond by end of timeout
                    catch (SocketException se)
                    {
                        m_logger.DebugFormat("Authentication attempt {0}/{1} using {2} failed. Reason: {3}", retryCt + 1, maxRetries + 1, server, se.Message);
                    }
                    catch (Exception e)
                    {
                        throw new RADIUSException("Unexpected error while trying to authenticate.", e);
                    }

                }
            }
            throw new RADIUSException(String.Format("No response from server(s) after {0} tries.", maxRetries + 1));
        }

        //Sends a start accounting request to the RADIUS server, returns true on acknowledge of request
        public bool startAccounting(string username, Packet.Acct_AuthenticType authType)
        {
            //Create accounting request packet
            Packet accountingRequest = new Packet(Packet.Code.Accounting_Request, identifier, sharedKey);
            accountingRequest.addAttribute(Packet.AttributeType.User_Name, username);
            accountingRequest.addAttribute(Packet.AttributeType.Acct_Status_Type, (int)Packet.Acct_Status_TypeType.Start);
            if (String.IsNullOrEmpty(sessionId))
                throw new RADIUSException("Session ID must be present for accounting.");
            accountingRequest.addAttribute(Packet.AttributeType.Acct_Session_Id, sessionId);
            
            if (NAS_Identifier == null && NAS_IP_Address == null)
                throw new RADIUSException("A NAS_Identifier or NAS_IP_Address (or both) must be supplied.");
            if (NAS_IP_Address != null)
                accountingRequest.addRawAttribute(Packet.AttributeType.NAS_IP_Address, NAS_IP_Address);
            if (NAS_Identifier != null)
                accountingRequest.addAttribute(Packet.AttributeType.NAS_Identifier, NAS_Identifier);

            if (authType != Packet.Acct_AuthenticType.Not_Specified)
                accountingRequest.addAttribute(Packet.AttributeType.Acct_Authentic, (int)authType);

            m_logger.DebugFormat("Attempting to send {0} for user {1}", accountingRequest.code, username);

            for (int retryCt = 0; retryCt <= maxRetries; retryCt++)
            {
                foreach (string server in servers)
                {
                    //Accounting request packet created, sending data...
                    UdpClient client = new UdpClient(server, accountingPort);
                    client.Client.SendTimeout = timeout;
                    client.Client.ReceiveTimeout = timeout;

                    try
                    {
                        client.Send(accountingRequest.toBytes(), accountingRequest.length);

                        //Listen for response, since the server has been specified, we don't need to re-specify server
                        IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                        byte[] respBytes = client.Receive(ref RemoteIpEndPoint);
                        Packet responsePacket = new Packet(respBytes);

                        //Verify packet response is good, authenticator should be MD5(Code+ID+Length+RequestAuth+Attributes+Secret)
                        if (!responsePacket.verifyResponseAuthenticator(accountingRequest.authenticator, sharedKey))
                            throw new RADIUSException(String.Format("Received response to accounting request with code: {0}, but an incorrect response authenticator was supplied.", responsePacket.code));

                        lastReceievedPacket = responsePacket;

                        client.Close();

                        m_logger.DebugFormat("Received accounting response: {0} for user {1}", responsePacket.code, username);

                        return responsePacket.code == Packet.Code.Accounting_Response;
                    }

                    //SocketException is thrown if the  server does not respond by end of timeout
                    catch (SocketException se)
                    {
                        m_logger.DebugFormat("Accounting start attempt {0}/{1} using {2} failed. Reason: {3}", retryCt + 1, maxRetries + 1, server, se.Message);
                    }
                    catch (Exception e)
                    {
                        throw new RADIUSException("Unexpected error while trying start accounting.", e);
                    }
                }
            }
            throw new RADIUSException(String.Format("No response from server(s) after {0} tries.", maxRetries + 1));
        }

        public bool stopAccounting(string username, Packet.Acct_Terminate_CauseType? terminateCause)
        {
            Packet accountingRequest = new Packet(Packet.Code.Accounting_Request, identifier, sharedKey);
            accountingRequest.addAttribute(Packet.AttributeType.User_Name, username);
            if(String.IsNullOrEmpty(sessionId))
                throw new RADIUSException("Session ID must be present for accounting.");
            accountingRequest.addAttribute(Packet.AttributeType.Acct_Session_Id, sessionId);
            accountingRequest.addAttribute(Packet.AttributeType.Acct_Status_Type, (int)Packet.Acct_Status_TypeType.Stop);
            if(terminateCause != null)
                accountingRequest.addAttribute(Packet.AttributeType.Acct_Terminate_Cause, (int) Packet.Acct_Terminate_CauseType.User_Request);

            m_logger.DebugFormat("Attempting to send {0} for user {1}", accountingRequest.code, username);

            for (int retryCt = 0; retryCt <= maxRetries; retryCt++)
            {
                foreach (string server in servers)
                {
                    //Accounting request packet created, sending data...
                    UdpClient client = new UdpClient(server, accountingPort);
                    client.Client.SendTimeout = timeout;
                    client.Client.ReceiveTimeout = timeout;

                    try
                    {
                        client.Send(accountingRequest.toBytes(), accountingRequest.length);

                        //Listen for response, since the server has been specified, we don't need to re-specify server
                        IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                        byte[] respBytes = client.Receive(ref RemoteIpEndPoint);
                        Packet responsePacket = new Packet(respBytes);

                        //Verify packet response is good, authenticator should be MD5(Code+ID+Length+RequestAuth+Attributes+Secret)
                        if (!responsePacket.verifyResponseAuthenticator(accountingRequest.authenticator, sharedKey))
                            throw new RADIUSException(String.Format("Received response to accounting request with code: {0}, but an incorrect response authenticator was supplied.", responsePacket.code));

                        lastReceievedPacket = responsePacket;

                        client.Close();

                        m_logger.DebugFormat("Received accounting response: {0} for user {1}", responsePacket.code, username);

                        return responsePacket.code == Packet.Code.Accounting_Response;
                        //SocketException is thrown if the  server does not respond by end of timeout
                    }
                    catch (SocketException se)
                    {
                        m_logger.DebugFormat("Accounting stop attempt {0}/{1} using {2} failed. Reason: {3}", retryCt + 1, maxRetries + 1, server, se.Message);
                    }
                    catch (Exception e)
                    {
                        throw new RADIUSException("Unexpected error while trying stop accounting.", e);
                    }
                }
            }
            throw new RADIUSException(String.Format("No response from server(s) after {0} tries.", maxRetries + 1));
        }
    }

    class RADIUSException : Exception {
        public RADIUSException(string msg) : base(msg) { }
        public RADIUSException(string msg, Exception innerException) : base(msg, innerException) { }
    }
}
