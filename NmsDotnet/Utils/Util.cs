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

        public static bool IpValidCheck(string Ip)
        {
            // ip 값이 null이면 실패 반환
            if (Ip == null)
                return false;

            // ip 길이가 15자 넘거나 7보다 작으면 실패를 반환
            if (Ip.Length > 15 || Ip.Length < 7)
                return false;

            string[] number = Ip.Split('.');
            foreach (string s in number)
            {
                int n = Convert.ToInt32(s);
                if (n < 0x00 || n > 0xFF)
                {
                    return false;
                }
            }

            // 숫자 갯수
            int nNumCount = 0;

            // '.' 갯수
            int nDotCount = 0;

            for (int i = 0; i < Ip.Length; i++)
            {
                if (Ip[i] < '0' || Ip[i] > '9')
                {
                    if ('.' == Ip[i])
                    {
                        ++nDotCount;
                        nNumCount = 0;
                    }
                    else
                        return false;
                }
                else
                {
                    // 4자리가 넘으면 실패 반환
                    if (++nNumCount > 3)
                        return false;
                }
            }
            // '.' 3개 아니여도 실패 반환
            if (nDotCount != 3 || nNumCount == 0)
                return false;

            return true;
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