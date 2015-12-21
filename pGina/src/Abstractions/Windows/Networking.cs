using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Abstractions.Logging;
using System.Net;
using System.Net.Sockets;
using System.Net.Mail;
using System.Diagnostics;
using System.IO;
using log4net;
using log4net.Appender;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace Abstractions.Windows
{
    public static class Networking
    {
        /// <summary>
        /// get UTC ntp time
        /// based on http://stackoverflow.com/questions/1193955/how-to-query-an-ntp-server-using-c
        /// </summary>
        /// <param name="FQDN">server address</param>
        /// <returns>DateTime, on error DateTime.MinValue</returns>
        public static DateTime GetNetworkTime(string[] FQDN)
        {
            DateTime RetVal = DateTime.MinValue;

            //time server
            for (uint x = 0; x < FQDN.Length; x++)
            {
                // NTP message size - 16 bytes of the digest (RFC 2030)
                Byte[] ntpData = new byte[48];

                //Setting the Leap Indicator, Version Number and Mode values
                ntpData[0] = 0x1B; //LI = 0 (no warning), VN = 3 (IPv4 only), Mode = 3 (Client Mode)

                try
                {
                    IPAddress[] addresses = Dns.GetHostEntry(FQDN[x]).AddressList;

                    //The UDP port number assigned to NTP is 123
                    IPEndPoint ipEndPoint = new IPEndPoint(addresses[0], 123);
                    //NTP uses UDP
                    Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    socket.ReceiveTimeout = 3 * 1000;
                    socket.SendTimeout = 3 * 1000;

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
                    LibraryLogging.Error("get ntp {0} error:{1}", FQDN[x], ex);
                }
            }

            return RetVal;
        }

        // stackoverflow.com/a/3294698/162671
        internal static UInt32 SwapEndianness(UInt64 x)
        {
            return (UInt32)(((x & 0x000000ff) << 24) +
                           ((x & 0x0000ff00) << 8) +
                           ((x & 0x00ff0000) >> 8) +
                           ((x & 0xff000000) >> 24));
        }

        /// <summary>
        /// does send a mail including the last 60 system-Event and application-Event lines
        /// plus the last 175 pgina logfile lines
        /// </summary>
        /// <param name="mailAddress"></param>
        /// <param name="smtpAddress"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="ssl"></param>
        /// <returns></returns>
        public static Boolean email(string[] mailAddress, string[] smtpAddress, string username, string password, string subject, string body, bool ssl)
        {
            Boolean ret = false;

            #region input checks
            if (mailAddress.Length == 0)
            {
                LibraryLogging.Error("can't send email: mailAddress.Length == 0");
                return false;
            }
            else
            {
                bool atleastonemail = false;
                foreach (string str in mailAddress)
                {
                    if (!String.IsNullOrEmpty(str))
                    {
                        atleastonemail = true;
                        break;
                    }
                }
                if (!atleastonemail)
                {
                    LibraryLogging.Error("can't send email: mailAddress array is empty");
                    return false;
                }
            }
            if (smtpAddress.Length == 0)
            {
                LibraryLogging.Error("can't send email: smtpAddress.Length == 0");
                return false;
            }
            else
            {
                bool atleastoneserver = false;
                foreach (string str in smtpAddress)
                {
                    if (!String.IsNullOrEmpty(str))
                    {
                        atleastoneserver = true;
                        break;
                    }
                }
                if (!atleastoneserver)
                {
                    LibraryLogging.Error("can't send email: smtpAddress array is empty");
                    return false;
                }
            }
            if (String.IsNullOrEmpty(subject))
            {
                LibraryLogging.Error("can't send email: subject is empty");
            }
            #endregion

            try
            {
                using (EventLog systemLog = new EventLog("System"))
                {
                    body += "\n\n====================Eventlog System====================\n";
                    for (int x = systemLog.Entries.Count - 60; x < systemLog.Entries.Count; x++)
                    {
                        body += String.Format("{0:yyyy-MM-dd HH:mm:ss} {1} {2} {3}\n", systemLog.Entries[x].TimeGenerated, systemLog.Entries[x].EntryType, (UInt16)systemLog.Entries[x].InstanceId, systemLog.Entries[x].Message);
                    }
                }
                using (EventLog application = new EventLog("Application"))
                {
                    body += "\n\n====================Eventlog Application===============\n";
                    for (int x = application.Entries.Count - 60; x < application.Entries.Count; x++)
                    {
                        body += String.Format("{0:yyyy-MM-dd HH:mm:ss} {1} {2} {3}\n", application.Entries[x].TimeGenerated, application.Entries[x].EntryType, (UInt16)application.Entries[x].InstanceId, application.Entries[x].Message);
                    }
                }
            }
            catch { }

            ServicePointManager.ServerCertificateValidationCallback = delegate(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };

            for (uint x = 0; x < smtpAddress.Length; x++)
            {
                string smtp = smtpAddress[x].Split(new char[] { ':' }).First();
                int port = 465;
                if (String.Compare(smtp, smtpAddress[x].Split(new char[] { ':' }).Last()) != 0)
                {
                    try
                    {
                        port = Convert.ToInt32(smtpAddress[x].Split(new char[] { ':' }).Last());
                    }
                    catch (Exception ex)
                    {
                        LibraryLogging.Warn("unable to retrieve smtp port from {0} Error:{1}", smtpAddress[x], ex.Message);
                        continue;
                    }
                }
                using (SmtpClient client = new SmtpClient(smtp, port))
                {
                    client.EnableSsl = ssl;
                    client.Timeout = Convert.ToInt32(new TimeSpan(0, 0, 30).TotalMilliseconds);
                    if (!String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(password))
                    {
                        client.Credentials = new NetworkCredential(username, password);
                    }

                    for (uint y = 0; y < mailAddress.Length; y++)
                    {
                        if (mailAddress[y] == null)
                            continue;
                        try
                        {
                            // get the logfile
                            string logfile = null;
                            logfile = LogManager.GetRepository().GetAppenders().OfType<FileAppender>().Where(fa => fa.Name == "bigfile").Single().File;
                            if (!String.IsNullOrEmpty(logfile))
                            {
                                using (StreamReader log = new StreamReader(logfile, true))
                                {
                                    // read the last 50kbytes of the log
                                    if (log.BaseStream.Length > 50 * 1024) //50kbytes
                                        log.BaseStream.Seek(50 * 1024 * -1, SeekOrigin.End);

                                    string[] lastlines = log.ReadToEnd().Split('\n');
                                    int line_count = 0;
                                    if (lastlines.Length > 175)
                                        line_count = lastlines.Length - 176;
                                    body += "\n\n====================Pgina log==========================\n";
                                    for (; line_count < lastlines.Length; line_count++)
                                    {
                                        body += lastlines[line_count] + '\n';
                                    }
                                }
                            }

                            using (MailMessage message = new MailMessage(mailAddress[y], mailAddress[y], subject, body))
                            {
                                client.Send(message);
                            }
                            mailAddress[y] = null;
                        }
                        catch (Exception ex)
                        {
                            LibraryLogging.Warn("Failed to send message \"{0}\" to:{1} port:{2} Error:{3}", subject, smtp, port, ex.Message);
                        }
                    }
                }

                if (mailAddress.All(k => string.IsNullOrEmpty(k)))
                {
                    ret = true;
                    break;
                }
            }

            return ret;
        }

        public static Boolean sendMail(Dictionary<string, string> settings, string username, string password, string subject, string body)
        {
            string mailAddress = settings["notify_email"];
            string smtpAddress = settings["notify_smtp"];
            string user = settings["notify_user"];
            string pass = settings["notify_pass"];
            bool ssl = Convert.ToBoolean(settings["notify_cred"]);
            bool cred = Convert.ToBoolean(settings["notify_ssl"]);

            if (cred)
            {
                // use login credential first
                if (!Abstractions.Windows.Networking.email(mailAddress.Split(' '), smtpAddress.Split(' '), username, password, subject, body, ssl))
                {
                    if (Abstractions.Windows.Networking.email(mailAddress.Split(' '), smtpAddress.Split(' '), user, pass, subject, body, ssl))
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }
            else
            {
                if (Abstractions.Windows.Networking.email(mailAddress.Split(' '), smtpAddress.Split(' '), user, pass, subject, body, ssl))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
