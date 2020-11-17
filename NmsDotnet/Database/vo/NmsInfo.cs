using Newtonsoft.Json;
using NmsDotnet.Database.vo;
using NmsDotnet.vo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NmsDotnet.Database.vo
{
    internal class NmsInfo
    {
        private NmsInfo()
        {
        }

        public ObservableCollection<Server> serverList;
        public ObservableCollection<Group> groupList;

        [JsonIgnore]
        public ObservableCollection<LogItem> activeLog;

        [JsonIgnore]
        public ObservableCollection<LogItem> logHistory;

        [JsonIgnore]
        public List<Alarm> alarmInfo;

        public ObservableCollection<Server> RealServerList()
        {
            // oc deep copy
            ObservableCollection<Server> oc = new ObservableCollection<Server>(serverList);

            for (int i = 0; i < oc.Count; i++)
            {
                if (string.IsNullOrEmpty(oc[i].Id))
                {
                    oc.Remove(oc[i]);
                    --i;
                }
            }
            return oc;
        }

        private static NmsInfo instance = null;

        public static NmsInfo GetInstance()
        {
            if (instance == null)
            {
                instance = new NmsInfo();
            }
            return instance;
        }
    }
}