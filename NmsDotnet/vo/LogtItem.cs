using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NmsDotNet.vo
{    
    class LogItem
    {
        public String Server { get; set; }
        public String Info { get; set; }
        public String Desc { get; set; }

        public static List<LogItem> instance;

        public static List<LogItem> GetInstance()
        {
            if ( instance == null)
            {
                instance = new List<LogItem>();
            }
            return instance;
        }
    }
}
