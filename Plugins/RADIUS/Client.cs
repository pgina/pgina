using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace pGina.Plugin.RADIUS
{
    class RADIUSClient
    {
        public string server { get; set; }
        public int port { get; set; }
        public string sharedKey { get; set; }
        public int timeout { get; set; }

        public bool Authenticate(string username, string password)
        {
            UdpClient client = new UdpClient(server, port);
            client.Client.SendTimeout = timeout;
            client.Client.ReceiveTimeout = timeout;
            

            Packet authPacket = new Packet(Packet.Code.Access_Request);
            authPacket.sharedKey = sharedKey;
            authPacket.addAttribute(Packet.AttributeType.User_Name, username);
            authPacket.addAttribute(Packet.AttributeType.User_Password, password);
            authPacket.addRawAttribute(Packet.AttributeType.NAS_IP_Address, new byte[]{192, 168, 1, 4});//getIPAddress());
            //Add NAS_Identifier iff requested

            byte[] pBytes = authPacket.toBytes();


            Console.WriteLine("Auth Packet: {0}", authPacket);
            
            client.Send(authPacket.toBytes(), authPacket.length);


            //Listen for response, since the server has been specified, we don't need to re-specify server
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] respBytes = client.Receive(ref RemoteIpEndPoint);
            Packet responsePacket = new Packet(respBytes);
            //Verify packet response is good

            Console.WriteLine("\nReceived Packet: {0}", responsePacket);

            client.Close();

            return responsePacket.code == Packet.Code.Access_Accept;
        }


        //Returns the current IPv4 address
        private byte[] getIPAddress()
        {
            IPAddress[] ipList = Dns.GetHostAddresses("");
            // Grab the first IPv4 address in the list
            foreach (IPAddress addr in ipList)
            {
                if (addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return addr.GetAddressBytes();
                    
                }
            }
            return null;
        }

    }

    
}
