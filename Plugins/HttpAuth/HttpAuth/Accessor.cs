using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using pGina.Shared.Types;
using System.IO;

namespace pGina.Plugin.HttpAuth
{
    public class HttpAccessor
    {
        static Dictionary<string, string> resps = new Dictionary<string, string>();

        public static BooleanResult getResponse(String uname, String pwd)
        {
            var cli = new WebClient();
            cli.Headers[HttpRequestHeader.ContentType] = "application/json";
            var data = "{\"uname\":\"" + uname + "\",\"pwd\":\"" + pwd + "\"}";

            try
            {
                string response = cli.UploadString("http://pginaloginserver/login", data);

                // save it for later use
                resps.Add(uname, response);

                // Successful authentication
                return new BooleanResult() { Success = true };
            }
            catch (WebException we)
            {
                // Authentication failure
                HttpWebResponse res = (HttpWebResponse)we.Response;
                var resReader = new StreamReader(res.GetResponseStream());
                var m = resReader.ReadLine();

                if (m.Length == 0)
                {
                    m = res.StatusCode + ": " + res.StatusDescription;
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
}
