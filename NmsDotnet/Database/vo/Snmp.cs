using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using NmsDotNet.Database;
using log4net;

namespace NmsDotNet.Database.vo
{
    /// <summary>
    /// Trap을 수신할 때 정보를 데이터베이스에 기록
    /// </summary>

    internal class Snmp
    {
        private static readonly ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public string Id { get; set; }

        public string type { get; set; }
        public string IP { get; set; }
        public string Syntax { get; set; }
        public string Port { get; set; }
        public string Community { get; set; }
        public string Value { get; set; }
        public string LevelString { get; set; }
        public string TypeValue { get; set; }
        public string TypeOid { get; set; }
        public bool Enable { get; set; }
        public string Desc { get; set; }
        public int Channel { get; set; }
        public int Index { get; set; }
        public string Main { get; set; }
        public string TranslateValue { get; set; }
        public string TrapString { get; set; }
        public TrapType Type { get; set; }

        public enum Level
        {
            Disabled = 1,
            Information = 2,
            Warning = 3,
            Critical = 4
        }

        public enum TrapType
        {
            begin = 1,
            end = 2,
            log = 3
        }

        public enum EnumMain
        {
            Main = 1,
            Backup = 2
        }

        public string MakeTrapLogString()
        {
            string logString = string.Format($"{TranslateValue} ({Main}) (Channel : {Channel}) (Index : {Index}) ({TypeValue})");
            logger.Info("logString : " + logString);
            return logString;
        }

        public static string GetLevelString(int level)
        {
            logger.Info("GetLevelString : " + level);
            return Enum.GetName(typeof(Level), level);
        }

        public static string GetNameFromOid(string oid)
        {
            string value = null;
            string query = String.Format($"SELECT name FROM snmp WHERE id = '{oid}'");
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.getInstance().ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    value = rdr["name"].ToString();
                }
                rdr.Close();
            }
            if (value == null)
            {
                value = oid;
            }
            return value;
        }

        public static string GetTranslateValue(string name)
        {
            string value = null;
            string query = String.Format($"SELECT translate FROM translate WHERE name = '{name}'");
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.getInstance().ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    value = rdr["translate"].ToString();
                }
                rdr.Close();
            }
            if (value == null)
            {
                value = "no value";
            }
            return value;
        }

        public static void RegisterSnmpInfo(Snmp snmp)
        {
            string query = String.Format(@"INSERT INTO snmp (id, ip, syntax, community, type) VALUES (@id, @ip, @syntax, @community, @type) ON DUPLICATE KEY UPDATE edit_time = CURRENT_TIMESTAMP(), ip = @ip, syntax = @syntax, community = @community, type = @type");
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.getInstance().ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", snmp.Id);
                cmd.Parameters.AddWithValue("@ip", snmp.IP);
                cmd.Parameters.AddWithValue("@syntax", snmp.Syntax);
                cmd.Parameters.AddWithValue("@community", snmp.Community);
                cmd.Parameters.AddWithValue("@type", snmp.type);
                cmd.Prepare();
                cmd.ExecuteNonQuery();
            }
        }
    }
}