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

        public static bool Get(string uri)
        {
            return true;
        }

        public static async Task PostAsync(string uri, string jsonBody)
        {
            Task.Run(() => Post(uri, jsonBody));
        }

        public static void Post(string uri, string jsonBody)
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