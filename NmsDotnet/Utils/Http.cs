using log4net;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NmsDotnet.Utils
{
    internal static class Http
    {
        private static readonly ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static string Get(string uri, NameValueCollection nv)
        {
            string responseText = string.Empty;

            if (nv != null)
            {
                string q = String.Join("&",
                 nv.AllKeys.Select(a => a + "=" + HttpUtility.UrlEncode(nv[a])));
                uri = string.Format($"{uri}?{q}");
            }

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "GET";
            request.Timeout = 5 * 1000; // 5초
            //request.Headers.Add("Authorization", "BASIC SGVsbG8="); // 헤더 추가 방법

            using (HttpWebResponse resp = (HttpWebResponse)request.GetResponse())
            {
                HttpStatusCode status = resp.StatusCode;
                Console.WriteLine(status);  // 정상이면 "OK"

                Stream respStream = resp.GetResponseStream();
                using (StreamReader sr = new StreamReader(respStream))
                {
                    responseText = sr.ReadToEnd();
                }
            }

            return responseText;
        }

        public static string Post(string uri, string jsonBody)
        {
            // Here we create the request and write the POST data to it.
            var request = (HttpWebRequest)HttpWebRequest.Create(uri);
            request.ContentType = "application/json";

            request.Method = "POST";
            request.Timeout = 1000;

            try
            {
                using (var writer = new StreamWriter(request.GetRequestStream()))
                {
                    writer.Write(jsonBody);
                }

                string response = string.Empty;
                using (WebResponse res = request.GetResponse())
                {
                    Stream respStream = res.GetResponseStream();
                    using (StreamReader sr = new StreamReader(respStream))
                    {
                        response = sr.ReadToEnd();
                    }
                }

                logger.Info(jsonBody);
                return response;
            }
            catch (WebException wex)
            {
                logger.Error(wex.ToString());
                logger.Error(uri);
                logger.Error(jsonBody);
                return null;
            }
        }

        public static string Delete(string uri, string jsonBody)
        {
            // Here we create the request and write the POST data to it.
            var request = (HttpWebRequest)HttpWebRequest.Create(uri);
            request.ContentType = "application/json";

            request.Method = "DELETE";
            request.Timeout = 1000;

            try
            {
                using (var writer = new StreamWriter(request.GetRequestStream()))
                {
                    writer.Write(jsonBody);
                }

                string response = string.Empty;
                using (WebResponse res = request.GetResponse())
                {
                    Stream respStream = res.GetResponseStream();
                    using (StreamReader sr = new StreamReader(respStream))
                    {
                        response = sr.ReadToEnd();
                    }
                }

                logger.Info(jsonBody);
                return response;
            }
            catch (WebException wex)
            {
                logger.Error(wex.ToString());
                logger.Error(uri);
                logger.Error(jsonBody);
                return null;
            }
        }

        public static string Put(string uri, string jsonBody)
        {
            // Here we create the request and write the POST data to it.
            var request = (HttpWebRequest)HttpWebRequest.Create(uri);
            request.ContentType = "application/json";

            request.Method = "PUT";
            request.Timeout = 1000;

            try
            {
                using (var writer = new StreamWriter(request.GetRequestStream()))
                {
                    writer.Write(jsonBody);
                }

                string response = string.Empty;
                using (WebResponse res = request.GetResponse())
                {
                    Stream respStream = res.GetResponseStream();
                    using (StreamReader sr = new StreamReader(respStream))
                    {
                        response = sr.ReadToEnd();
                    }
                }

                logger.Info(jsonBody);
                return response;
            }
            catch (WebException wex)
            {
                logger.Error(wex.ToString());
                logger.Error(uri);
                logger.Error(jsonBody);
                return null;
            }
        }

        public static async Task TitanApiPostAsync(string uri, string jsonBody)
        {
            Task.Run(() => TitanApiPost(uri, jsonBody));
        }

        public static void TitanApiPost(string uri, string jsonBody)
        {
            // Here we create the request and write the POST data to it.
            var request = (HttpWebRequest)HttpWebRequest.Create(uri);
            request.Headers.Add("authorization", "Basic QXBpOlRpdGFuQVBJQEF0M21l");
            request.ContentType = "application/json";

            request.Method = "POST";
            request.Timeout = 1000;
            WebResponse response;

            try
            {
                using (var writer = new StreamWriter(request.GetRequestStream()))
                {
                    writer.Write(jsonBody);
                }

                response = request.GetResponse();
                var webStream = response.GetResponseStream();
                var reader = new StreamReader(webStream);
                logger.Info(jsonBody);
                logger.Info(reader.ReadToEnd());
            }
            catch (WebException wex)
            {
                logger.Error(wex.ToString());
                logger.Error(uri);
                logger.Error(jsonBody);
            }
        }
    }
}