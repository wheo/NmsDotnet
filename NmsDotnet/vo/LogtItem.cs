using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using NmsDotnet.Database.vo;
using NmsDotNet.Database;

namespace NmsDotNet.vo
{    
    class LogItem : INotifyPropertyChanged
    {
        public String Server { get; set; }
        public String Info { get; set; }
        public String Desc { get; set; }

        public string Gid { get; set; }
        public string Name { get; set; }

        public string Ip { get; set; }

        public static LogItem instance;

        public static List<LogItem> listItem;

        public event PropertyChangedEventHandler PropertyChanged;

        public static DataTable dt;        

        public static List<LogItem> GetListItem()
        {
            if ( listItem == null)
            {
                listItem = new List<LogItem>();
            }
            return listItem;
        }

        public static LogItem GetInstance()
        {
            if ( instance == null)
            {
                instance = new LogItem();
            }
            if ( dt == null)
            {
                dt = new DataTable();
            }
            return instance;
        }       

        public int LoggingDatabase(Snmp trap)
        {
            string query = String.Format(@"INSERT INTO log (oid, ip, port, type, community, value) VALUES (@oid, @ip, @port, @type, @community, @value)");
            int ret = 0;
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.getInstance().ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@oid", trap.Id);
                cmd.Parameters.AddWithValue("@ip", trap.IP);
                cmd.Parameters.AddWithValue("@port", trap.Port);
                cmd.Parameters.AddWithValue("@type", trap.Syntax);
                cmd.Parameters.AddWithValue("@community", trap.Community);
                cmd.Parameters.AddWithValue("@value", trap.Value);
                cmd.Prepare();
                ret = cmd.ExecuteNonQuery();
            }

            return ret;
        }

        public DataTable GetLog()
        {            
            string query = $"SELECT DATE_FORMAT(L.create_time, '%Y-%m-%d %H:%i:%s') as time" +
                $", L.ip as ip" +                    
                $", L.oid as oid" +
                $", L.level as level" +
                $", L.type as type" +
                $", L.community as community" +
                $", L.value as value" +
                $", S.name as name" +
                $" FROM log L" +
                $" LEFT JOIN snmp S ON S.id = L.oid" +
                $" ORDER BY L.create_time DESC" +
                $" LIMIT 0,10";
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.getInstance().ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Prepare();
                MySqlDataAdapter adpt = new MySqlDataAdapter(query, conn);
                adpt.Fill(dt);
            }

            return dt;
        }
    }
}
