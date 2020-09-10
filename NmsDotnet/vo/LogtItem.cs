using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using NmsDotNet.Database;
using NmsDotNet.Database.vo;

namespace NmsDotNet.vo
{
    public class LogList
    {
        public List<LogItem> Logs { get; set; }
    }

    public class LogItem
    {
        public string StartAt { get; set; }
        public string EndAt { get; set; }
        public string Ip { get; set; }
        public string Name { get; set; }
        public string Level { get; set; }
        public string Color { get; set; }
        public string Value { get; set; }
        public string TypeValue { get; set; }
        public string IsConfirm { get; set; }

        public int idx { get; set; }

        public static LogItem instance;

        public int LogItemCount = 0;

        public static List<LogItem> listItem;

        public static DataTable dt;

        public static List<LogItem> GetListItem()
        {
            if (listItem == null)
            {
                listItem = new List<LogItem>();
            }
            return listItem;
        }

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

        public int LoggingDatabase(Snmp trap)
        {
            int ret = 0;
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.getInstance().ConnectionString))
            {
                string is_display = trap.TypeValue == "begin" ? "Y" : "N";
                string query = string.Format(@"INSERT INTO log (ip, port, community, level, oid, value, snmp_type_value, is_display) VALUES (@ip, @port, @community, @level, @oid, @value, @snmp_type_value, @is_display)");
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ip", trap.IP);
                cmd.Parameters.AddWithValue("@port", trap.Port);
                cmd.Parameters.AddWithValue("@community", trap.Community);
                cmd.Parameters.AddWithValue("@level", trap.LevelString);
                cmd.Parameters.AddWithValue("@oid", trap.TypeOid);
                cmd.Parameters.AddWithValue("@is_display", is_display);
                if (!String.IsNullOrEmpty(trap.TranslateValue))
                {
                    cmd.Parameters.AddWithValue("@value", trap.TrapString);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@value", trap.TranslateValue);
                }
                cmd.Parameters.AddWithValue("@snmp_type_value", trap.TypeValue);

                cmd.Prepare();
                ret = cmd.ExecuteNonQuery();

                if (trap.TypeValue == "end")
                {
                    query = "UPDATE log set end_at = current_timestamp(), is_display = 'N' WHERE ip = @ip AND oid = @oid AND snmp_type_value = 'begin'";
                    cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@ip", trap.IP);
                    cmd.Parameters.AddWithValue("@oid", trap.TypeOid);

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

        public List<LogItem> GetLog()
        {
            return GetLog("");
        }

        public List<LogItem> GetLog(string type)
        {
            string option_query = null;

            if (!type.Equals("dialog"))
            {
                option_query = " AND L.is_display = 'Y'";
            }

            DataTable dt = new DataTable();
            string query = String.Format(@"SELECT DATE_FORMAT(L.start_at, '%Y-%m-%d %H:%i:%s') as start_at
, IFNULL(DATE_FORMAT(L.end_at, '%Y-%m-%d %H:%i:%s'), '') as end_at
, L.ip as ip
, S.name AS name
, L.level as level
, IF(L.level = 'Critical', 'Red', IF(L.level = 'Warning', 'Yellow', IF(L.level = 'Information', 'Blue', 'Black'))) AS color
, L.value as value
, L.snmp_type_value as type_value
, L.idx as idx
FROM log L
LEFT JOIN server S ON S.ip = L.ip
WHERE 1=1
{0}
AND snmp_type_value = 'begin'
ORDER BY L.start_at DESC", option_query);
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.getInstance().ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Prepare();
                MySqlDataAdapter adpt = new MySqlDataAdapter(query, conn);
                dt.Clear();
                adpt.Fill(dt);
            }
            LogItemCount = dt.Rows.Count;
            return dt.AsEnumerable().Select(row => new LogItem
            {
                idx = row.Field<int>("idx"),
                StartAt = row.Field<string>("start_at"),
                EndAt = row.Field<string>("end_at"),
                Ip = row.Field<string>("ip"),
                Name = row.Field<string>("name"),
                Level = row.Field<string>("level"),
                Color = row.Field<string>("color"),
                Value = row.Field<string>("value"),
                TypeValue = row.Field<string>("type_value")
            }).ToList();
        }
    }
}