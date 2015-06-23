using System;
using System.Collections.Generic;
using System.Net;
using pGina.Shared.Types;
using System.IO;
using log4net;

namespace pGina.Plugin.HttpAuth
{
    public class HttpAccessor
    {
        static Dictionary<string, UInfo> resps = new Dictionary<string, UInfo>();
        private static ILog s_logger = LogManager.GetLogger("HttpAuthAccessor");    
        private static string loginServer;

        static HttpAccessor()
        {
            loginServer = _urlByEnvVar();
            if (loginServer == null) {
                // try to get URL from DNS
                try
                {
                    IPHostEntry entries = Dns.GetHostEntry("pginaloginserver");
                    if (entries.AddressList.Length > 0)
                    {
                        loginServer = entries.AddressList[0].ToString();
                    }
                    else
                    {
                        loginServer = "http://pginaloginserver/login";
                    }
                }
                catch (Exception dnsex)
                {
                    s_logger.ErrorFormat("Response: {0}", dnsex.ToString());
                    loginServer = "http://pginaloginserver/login";
                }
            }
        }

        private static string _urlByEnvVar() {
            try
            {
                return Environment.GetEnvironmentVariable("PGINALOGINSERVER");
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static BooleanResult getResponse(String uname, String pwd)
        {
            var cli = new WebClient();
            cli.Headers[HttpRequestHeader.ContentType] = "application/json";
            var data = "{\"uname\":\"" + uname + "\",\"pwd\":\"" + pwd + "\"}";

            try
            {
                string response = cli.UploadString(loginServer, data);

                s_logger.InfoFormat("Response: {0}", response);

                // save it for later use
                if(resps.ContainsKey(uname))
                {
                    resps.Remove(uname);
                }
                resps.Add(uname, UInfo.parseResponse(response));

                // Successful authentication
                return new BooleanResult() { Success = true };
            }
            catch (WebException we)
            {
                s_logger.ErrorFormat("Bad Response: {0}", we.ToString());

                // Authentication failure
                HttpWebResponse res = (HttpWebResponse)we.Response;
                string m;

                if (res != null)
                {
                    var resReader = new StreamReader(res.GetResponseStream());
                    var responseBody = resReader.ReadLine();
                    if (responseBody.Length > 0)
                    {
                        m = responseBody;
                    }
                    else
                    {
                        m = res.StatusCode + ": " + res.StatusDescription;
                    }
                }
                else
                {
                    m = we.ToString();
                }

                return new BooleanResult() { Success = false, Message = m };
            }
            catch (Exception e)
            {
                // very bad scenario
                return new BooleanResult() { Success = false, Message = e.StackTrace };
            }
        }

        public static void getUserInfo(String uname, String pwd, out UInfo uinfo)
        {
            if (! resps.ContainsKey(uname))
            {
                // wee need to getResponse first
                BooleanResult res = getResponse(uname, pwd);
                if (! res.Success)
                {
                    throw new Exception(res.Message);
                }
            }
            resps.TryGetValue(uname, out uinfo);
        }
        
    }
}
