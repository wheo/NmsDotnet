using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using NAudio.Wave;
using System.IO;

namespace NmsDotnet.Utils
{
    public class Util
    {
        public static void ConvertMp3ToWav(string path)
        {
            //string path = Directory.GetCurrentDirectory() + @"\alarmSound\music.mp3";
            using (Mp3FileReader mp3 = new Mp3FileReader(path))
            {
                using (WaveStream pcm = WaveFormatConversionStream.CreatePcmStream(mp3))
                {
                    WaveFileWriter.CreateWaveFile(Path.ChangeExtension(path, "wav"), pcm);
                }
            }
        }

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

        public static string UTF8_TO_EUCKR(string s)
        {
            System.Text.Encoding euckr = System.Text.Encoding.GetEncoding(51949);
            byte[] euckrBytes = euckr.GetBytes(s);

            string urlEncodingText = "";
            foreach (byte b in euckrBytes)
            {
                string addText = Convert.ToString(b, 16);
                urlEncodingText = urlEncodingText + "%" + addText;
            }
            return Convert.ToString(urlEncodingText);
        }
    }
}