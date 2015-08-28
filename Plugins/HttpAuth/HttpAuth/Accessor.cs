using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Collections;
using System.Diagnostics;
using System.Text.RegularExpressions;

using pGina.Shared.Types;
using log4net;

namespace pGina.Plugin.HttpAuth
{
    public class HttpAccessor
    {
        private static Dictionary<string, UInfo> resps = new Dictionary<string, UInfo>();
        private static ILog m_logger = LogManager.GetLogger("HttpAuthAccessor");
        private static string loginServer;

        static HttpAccessor()
        {
            loginServer = _urlByEnvVar();
            if (loginServer == null) {
                // try to get URL from DNS
                try
                {
                    List<string> entries = _getTxtRecords("pginaloginserver");
                    if (entries.Count > 0)
                    {
                        loginServer = entries[0].ToString();    // gets the first item
                    }
                    else
                    {
                        loginServer = "http://pginaloginserver/login";
                    }
                }
                catch (Exception dnsex)
                {
                    m_logger.ErrorFormat("Response: {0}", dnsex.ToString());
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

        /*
         * Uses http://www.robertsindall.co.uk/blog/getting-dns-txt-record-using-c-sharp/
         * because c# and whole M$osft is crap and has no tools to resolve TXT recs!!!
         */
        private static List<string> _getTxtRecords(string hostname)
        {
            List<string> txtRecords = new List<string>();
            string output;

            var startInfo = new ProcessStartInfo("nslookup");
            startInfo.Arguments = string.Format("-type=TXT {0}", hostname);
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;

            using (var cmd = Process.Start(startInfo))
            {
                output = cmd.StandardOutput.ReadToEnd();
            }

            MatchCollection matches = Regex.Matches(output, "\"([^\"]*)\"", RegexOptions.IgnoreCase);
            foreach (Match match in matches)
            {
                if (match.Success)
                    txtRecords.Add(match.Groups[1].Value);
            }

            return txtRecords;
        }

        public static BooleanResult getResponse(String uname, String pwd)
        {
            using (WebClient cli = new WebClient())
            {
                cli.Headers[HttpRequestHeader.ContentType] = "application/json";
                string data = "{\"username\":\"" + uname + "\",\"password\":\"" + pwd + "\"}";

                try
                {
                    string response = cli.UploadString(loginServer, data);

                    m_logger.InfoFormat("Response: {0}", response);

                    // save it for later use
                    if (resps.ContainsKey(uname))
                    {
                        resps.Remove(uname);
                    }
                    resps.Add(uname, UInfo.parseResponse(response));

                    // Successful authentication
                    return new BooleanResult() { Success = true };
                }
                catch (WebException we)
                {
                    m_logger.ErrorFormat("Bad Response: {0}", we.ToString());
                    string m;

                    // Authentication failure
                    using (HttpWebResponse res = (HttpWebResponse)we.Response)
                    {

                        if (res != null)
                        {
                            using (StreamReader resReader = new StreamReader(res.GetResponseStream()))
                            {
                                string responseBody = resReader.ReadLine();
                                if (responseBody.Length > 0)
                                {
                                    m = responseBody;
                                }
                                else
                                {
                                    m = res.StatusCode + ": " + res.StatusDescription;
                                }
                            }
                        }
                        else
                        {
                            m = we.ToString();
                        }
                    }

                    return new BooleanResult() { Success = false, Message = m };
                }
                catch (Exception e)
                {
                    // very bad scenario
                    return new BooleanResult() { Success = false, Message = e.StackTrace };
                }
            }
        }

        public static BooleanResult getUserInfo(String uname, String pwd, out UInfo uinfo)
        {
            BooleanResult ret = new BooleanResult() { Success = false, Message = "" };

            if (! resps.ContainsKey(uname))
            {
                // wee need to getResponse first
                ret = getResponse(uname, pwd);
            }
            resps.TryGetValue(uname, out uinfo);

            return ret;
        }
    }
}
