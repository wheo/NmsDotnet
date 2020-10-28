using log4net;
using MySql.Data.MySqlClient;
using NmsDotnet.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace NmsDotnet.Database.vo
{
    /// <summary>
    /// Trap을 수신할 때 정보를 데이터베이스에 기록
    /// </summary>
    ///
    public class SnmpSetting
    {
        public string Name { get; set; }
        public bool IsEnable { get; set; }
        public int Idx { get; set; }
    }

    public class GlobalSettings
    {
        public List<SnmpSetting> SnmpCM5000Settings { get; set; }
        public List<SnmpSetting> SnmpDR5000Settings { get; set; }
        public List<Server> ServerSettings { get; set; }
    }

    public class Snmp
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
        public EnumLevel Level { get; set; }

        public string _LocalIP { get; set; }

        public Snmp()
        {
            _LocalIP = Util.GetLocalIpAddress();
        }

        public enum EnumLevel
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
            string logString = "";
            if (Channel > 0)
            {
                string.Format($"{TranslateValue} ({TypeValue}) (Channel : {Channel}");
            }
            else
            {
                string.Format($"{TranslateValue} ({TypeValue})");
            }
            logger.Info(string.Format($"logString : {logString}"));
            return logString;
        }

        public static bool IsEnableTrap(string compareID)
        {
            logger.Debug(string.Format($"compareID : {compareID}"));
            string value = null;
            string query = String.Format(@"SELECT S.id, T.translate FROM translate T
INNER JOIN snmp S ON S.name = T.name
WHERE T.is_enable = 'N' ");
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.getInstance().ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    value = rdr["id"].ToString();
                    int idx = compareID.LastIndexOf('.');
                    string searchID = compareID.Substring(0, idx);
                    if (value.Contains(searchID))
                    {
                        logger.Debug(string.Format($"value : {value}, searchID : {searchID}"));
                        return false;
                    }
                }
                rdr.Close();
            }

            return true;
        }

        public static string GetLevelString(int level)
        {
            logger.Info("GetLevelString : " + level);
            return Enum.GetName(typeof(EnumLevel), level);
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

        public static IEnumerable<SnmpSetting> GetTrapAlarmList(string type)
        {
            DataTable dt = new DataTable();
            string query = String.Format($"SELECT * FROM translate WHERE type = '{type}'");
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.getInstance().ConnectionString))
            {
                conn.Open();
                MySqlDataAdapter adpt = new MySqlDataAdapter(query, conn);
                adpt.Fill(dt);
            }

            List<SnmpSetting> settings = dt.AsEnumerable().Select(row => new SnmpSetting
            {
                Name = row.Field<string>("translate"),
                Idx = row.Field<int>("idx"),
                IsEnable = row["is_enable"].ToString() == "Y" ? true : false
            }).ToList();

            return settings;
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

        public static Server GetServerInfo(Snmp snmp)
        {
            Server server = null;
            string query = string.Format($"SELECT * FROM server WHERE ip = '{snmp.IP}'");
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.getInstance().ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    server = new Server
                    {
                        Id = rdr["id"].ToString(),
                        Ip = rdr["ip"].ToString(),
                        Name = rdr["name"].ToString(),
                        Status = rdr["status"].ToString()
                    };
                }
            }

            return server;
        }

        public static void UpdateSnmpMessgeUseage(GlobalSettings settings)
        {
            foreach (var item in settings.SnmpCM5000Settings)
            {
                int ret = 0;
                string query = "UPDATE translate set is_enable = @is_enable WHERE idx = @idx";
                using (MySqlConnection conn = new MySqlConnection(DatabaseManager.getInstance().ConnectionString))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@idx", item.Idx);
                    cmd.Parameters.AddWithValue("@is_enable", item.IsEnable == true ? "Y" : "N");
                    cmd.Prepare();
                    ret = cmd.ExecuteNonQuery();
                }
            }

            foreach (var item in settings.SnmpDR5000Settings)
            {
                int ret = 0;
                string query = "UPDATE translate set is_enable = @is_enable WHERE idx = @idx";
                using (MySqlConnection conn = new MySqlConnection(DatabaseManager.getInstance().ConnectionString))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@idx", item.Idx);
                    cmd.Parameters.AddWithValue("@is_enable", item.IsEnable == true ? "Y" : "N");
                    cmd.Prepare();
                    ret = cmd.ExecuteNonQuery();
                }
            }
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