/*
	Copyright (c) 2012, pGina Team
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
using System.Net;
using System.Net.Sockets;
using log4net;

//based on http://stackoverflow.com/questions/1193955/how-to-query-an-ntp-server-using-c
namespace pGina.Plugin.pgSMB
{
    class ntp
    {
        private static ILog m_logger = LogManager.GetLogger("pgSMB[ntp]");

        public static DateTime GetNetworkTime(string FQDN)
        {
            DateTime RetVal = DateTime.Now;

            //time server
            string[] ntpServer = FQDN.Split(new char[] { ' ' });
            for (uint x = 0; x < ntpServer.Length; x++)
            {
                m_logger.InfoFormat("get ntp time from {0}", ntpServer[x]);

                // NTP message size - 16 bytes of the digest (RFC 2030)
                Byte[] ntpData = new byte[48];

                //Setting the Leap Indicator, Version Number and Mode values
                ntpData[0] = 0x1B; //LI = 0 (no warning), VN = 3 (IPv4 only), Mode = 3 (Client Mode)

                try
                {
                    IPAddress[] addresses = Dns.GetHostEntry(ntpServer[x]).AddressList;

                    //The UDP port number assigned to NTP is 123
                    IPEndPoint ipEndPoint = new IPEndPoint(addresses[0], 123);
                    //NTP uses UDP
                    Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                    socket.Connect(ipEndPoint);

                    socket.Send(ntpData);
                    socket.Receive(ntpData);
                    socket.Close();

                    //Offset to get to the "Transmit Timestamp" field (time at which the reply
                    //departed the server for the client, in 64-bit timestamp format."
                    const byte serverReplyTime = 40;

                    //Get the seconds part
                    UInt64 intPart = BitConverter.ToUInt32(ntpData, serverReplyTime);

                    //Get the seconds fraction
                    UInt64 fractPart = BitConverter.ToUInt32(ntpData, serverReplyTime + 4);

                    //Convert From big-endian to little-endian
                    intPart = SwapEndianness(intPart);
                    fractPart = SwapEndianness(fractPart);

                    UInt64 milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);

                    //**UTC** time
                    RetVal = (new DateTime(1900, 1, 1)).AddMilliseconds((Int64)milliseconds);

                    break;
                }
                catch (Exception ex)
                {
                    m_logger.ErrorFormat("get ntp error: {0}\n{1}\n{2}\n{3}", ex.Message, ex.Source, ex.StackTrace, ex.TargetSite);
                }
            }

            return RetVal;
        }

        // stackoverflow.com/a/3294698/162671
        private static UInt32 SwapEndianness(UInt64 x)
        {
            return (UInt32)(((x & 0x000000ff) << 24) +
                           ((x & 0x0000ff00) << 8) +
                           ((x & 0x00ff0000) >> 8) +
                           ((x & 0xff000000) >> 24));
        }
    }
}
