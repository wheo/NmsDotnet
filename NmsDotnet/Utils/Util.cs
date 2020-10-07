using System;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace NmsDotNet.Utils
{
    internal class Util
    {
        public static String GetLocalIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No networks adapters with an IPv4 address in the system!");
        }

        public static String GetCurrnetDatetime()
        {
            return DateTime.Now.ToString("yyyy-mm-dd hh:mm:ss");
        }

        private static readonly Regex _regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text

        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }
    }
}