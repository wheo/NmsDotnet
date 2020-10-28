using NmsDotnet.Database.vo;
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