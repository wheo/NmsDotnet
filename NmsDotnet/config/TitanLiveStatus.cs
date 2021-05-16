using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NmsDotnet.config
{
    public class TitanLiveStatus
    {
        public string State { get; set; }

        public string IP { get; set; }

        public List<String> UIDList { get; set; }

        [JsonIgnore]
        public DateTime currentTime;

        public TitanLiveStatus()
        {
            if (UIDList == null)
            {
                UIDList = new List<String>();
            }
        }

        public void Clear()
        {
            UIDList.Clear();
            UIDList = new List<String>();
        }
    }
}