using MySql.Data.MySqlClient;
using NmsDotnet.Database;
using NmsDotnet.Database.vo;
using NmsDotnet.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NmsDotnet.vo
{
    public class LogItem
    {
        public string StartAt { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        public string EndAt { get; set; }
        public string Ip { get; set; }
        public string Name { get; set; }
        public string Oid { get; set; }
        public bool IsConnection { get; set; } = true;
        public string _Level { get; set; }
        public int LevelPriority { get; set; }
        public string TypeValue { get; set; }

        public string Level
        {
            get
            {
                return _Level;
            }
            set
            {
                _Level = value;
                if (value.Equals("Critical"))
                {
                    Color = "Red";
                    LevelPriority = (int)Server.EnumStatus.Critical;
                }
                else if (value.Equals("Warning"))
                {
                    Color = "#FF8000";
                    LevelPriority = (int)Server.EnumStatus.Warning;
                }
                else if (value.Equals("Information"))
                {
                    Color = "Blue";
                    LevelPriority = (int)Server.EnumStatus.Information;
                }
                else
                {
                    LevelPriority = (int)Server.EnumStatus.Normal;
                    Color = "Green";
                }
            }
        }

        public string Color { get; set; }
        public string Value { get; set; }

        //deprecated
        //public string IsConfirm { get; set; }

        static public string _LocalIp { get; } = Util.GetLocalIpAddress();

        public int idx { get; set; }

        //public static LogItem instance;

        //public int LogItemCount = 0;

        //public static DataTable dt;

        /*
         * //public static List<LogItem> listItem;
        public static List<LogItem> GetListItem()
        {
            if (listItem == null)
            {
                listItem = new List<LogItem>();
            }
            return listItem;
        }
        */

        /*

        public static LogItem GetInstance()
        {
            if (instance == null)
            {
                instance = new LogItem();
            }
            if (dt == null)
            {
                dt = new DataTable();
            }
            return instance;
        }
        */

        public LogItem()
        {
        }

        public static int LoggingDatabase(Snmp trap)
        {
            int ret = 0;
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.getInstance().ConnectionString))
            {
                if (trap.TypeValue == "begin")
                {
                    string query = string.Format(@"INSERT INTO log (client_ip, ip, port, community, level, oid, value, snmp_type_value)
VALUES (@client_ip, @ip, @port, @community, @level, @oid, @value, @snmp_type_value) ON DUPLICATE KEY UPDATE client_ip = @client_ip");
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@client_ip", trap._LocalIP);
                    cmd.Parameters.AddWithValue("@ip", trap.IP);
                    cmd.Parameters.AddWithValue("@port", trap.Port);
                    cmd.Parameters.AddWithValue("@community", trap.Community);
                    cmd.Parameters.AddWithValue("@level", trap.LevelString);
                    cmd.Parameters.AddWithValue("@oid", trap.Oid);
                    if (String.IsNullOrEmpty(trap.TrapString))
                    {
                        cmd.Parameters.AddWithValue("@value", trap.TranslateValue);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@value", trap.TrapString);
                    }
                    cmd.Parameters.AddWithValue("@snmp_type_value", trap.TypeValue);

                    cmd.Prepare();
                    ret = cmd.ExecuteNonQuery();
                }
                else if (trap.TypeValue == "end")
                {
                    conn.Open();
                    string query = "UPDATE log set end_at = current_timestamp() WHERE ip = @ip AND oid = @oid AND snmp_type_value = 'begin'";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@ip", trap.IP);
                    cmd.Parameters.AddWithValue("@oid", trap.Oid);

                    cmd.Prepare();
                    ret = cmd.ExecuteNonQuery();
                }
            }

            return ret;
        }

        public static int ChangeConfirmStatus(int idx)
        {
            int ret = 0;

            string query = "UPDATE log set is_display = 'Y' WHERE idx = @idx";
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.getInstance().ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@idx", idx);
                cmd.Prepare();
                ret = cmd.ExecuteNonQuery();
            }
            return ret;
        }

        public static int LogHide()
        {
            int ret = 0;

            string query = "UPDATE log set is_display = 'N'";
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.getInstance().ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Prepare();
                ret = cmd.ExecuteNonQuery();
            }
            return ret;
        }

        public static List<LogItem> GetLog()
        {
            return GetLog("");
        }

        public static List<LogItem> GetLog(string dayFrom = null, string dayTo = null)
        {
            string option_query = null;
            string date_query = null;
            string order_query = "ASC";
            string is_active = "AND end_at is NULL";

            if (!string.IsNullOrEmpty(dayFrom) && !string.IsNullOrEmpty(dayTo))
            {
                date_query = string.Format($" AND L.start_at BETWEEN '{dayFrom}' AND '{dayTo}'");
                order_query = "DESC";
                is_active = "";
            }

            DataTable dt = new DataTable();
            string query = String.Format(@"SELECT DATE_FORMAT(L.start_at, '%Y-%m-%d %H:%i:%s') as start_at
, IFNULL(DATE_FORMAT(L.end_at, '%Y-%m-%d %H:%i:%s'), '') as end_at
, L.ip as ip
, L.oid as oid
, S.name AS name
, L.level as level
, IF(L.level = 'Critical', 'Red', IF(L.level = 'Warning', '#FF8000', IF(L.level = 'Information', 'Blue', 'Black'))) AS color
, L.value as value
, L.snmp_type_value as type_value
, L.idx as idx
FROM log L
LEFT JOIN server S ON S.ip = L.ip
WHERE 1=1
AND client_ip = '{0}'
AND snmp_type_value = 'begin'
{1}
{2}
ORDER BY L.start_at {3}", _LocalIp, is_active, date_query, order_query);
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.getInstance().ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Prepare();
                MySqlDataAdapter adpt = new MySqlDataAdapter(query, conn);
                dt.Clear();
                adpt.Fill(dt);
            }

            return dt.AsEnumerable().Select(row => new LogItem
            {
                idx = row.Field<int>("idx"),
                StartAt = row.Field<string>("start_at"),
                EndAt = row.Field<string>("end_at"),
                Oid = row.Field<string>("oid"),
                Ip = row.Field<string>("ip"),
                Name = row.Field<string>("name"),
                Level = row.Field<string>("level"),
                Color = row.Field<string>("color"),
                Value = row.Field<string>("value"),
                TypeValue = row.Field<string>("type_value")
            }).ToList();
        }

        public static int HideLogAlarm(int idx)
        {
            int ret = 0;
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.getInstance().ConnectionString))
            {
                // end_at 시간을 줘야하는지 고민해야함
                conn.Open();
                string query = "UPDATE log set end_at = current_timestamp() WHERE idx = @idx AND snmp_type_value = 'begin'";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@idx", idx);

                cmd.Prepare();
                ret = cmd.ExecuteNonQuery();
            }

            return ret;
        }

        public static string MakeCsvFile(List<LogItem> asc)
        {
            String csvBuff = "";

            StringBuilder sb = new StringBuilder();

            if (asc.Count > 0)
            {
                csvBuff = csvBuff + "StartAt,";
                csvBuff = csvBuff + "EndAt,";
                csvBuff = csvBuff + "Name,";
                csvBuff = csvBuff + "Ip,";
                csvBuff = csvBuff + "Level,";
                csvBuff = csvBuff + "Value,";
                csvBuff = csvBuff.Substring(0, csvBuff.Length - 1);
                csvBuff += System.Environment.NewLine;

                sb.Append(csvBuff);
                csvBuff = "";

                foreach (var item in asc)
                {
                    csvBuff = csvBuff + item.StartAt + ",";
                    csvBuff = csvBuff + item.EndAt + ",";
                    csvBuff = csvBuff + item.Name + ",";
                    csvBuff = csvBuff + item.Ip + ",";
                    csvBuff = csvBuff + item.Level + ",";
                    csvBuff = csvBuff + item.Value + ",";
                    csvBuff = csvBuff.Substring(0, csvBuff.Length - 1);
                    csvBuff += System.Environment.NewLine;
                    sb.Append(csvBuff);
                    csvBuff = "";
                }
            }

            csvBuff = sb.ToString();
            return csvBuff;
        }
    }
}