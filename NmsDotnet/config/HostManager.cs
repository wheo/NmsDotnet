using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NmsDotnet.config
{
    public class HostManager
    {
        public string uri { get; set; }
        public string ip { get; set; }
        public string port { get; set; }
        public static HostManager instance;

        public static HostManager getInstance()
        {
            if (instance == null)
            {
                instance = new HostManager();
            }
            return instance;
        }
    }
}