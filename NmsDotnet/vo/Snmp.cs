using log4net;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using NmsDotnet.config;
using NmsDotnet.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data;
using System.Linq;

namespace NmsDotnet.Database.vo
{
    public class ComboBoxPairs
    {
        public string _Key { get; set; }
        public string _Value { get; set; }

        public ComboBoxPairs()
        {
        }

        public ComboBoxPairs(string _key, string _value)
        {
            _Key = _key;
            _Value = _value;
        }
    }

    /// <summary>
    /// Trap을 수신할 때 정보를 데이터베이스에 기록
    /// </summary>
    ///
    public class SnmpSetting
    {
        [JsonProperty("translate")]
        public string Name { get; set; }

        [JsonConverter(typeof(MyJsonConveter))]
        [JsonProperty("is_enable")]
        public bool IsEnable { get; set; }

        [JsonConverter(typeof(MyJsonConveter))]
        [JsonProperty("is_visible")]
        public bool IsVisible { get; set; }

        [JsonProperty("idx")]
        public int Idx { get; set; }

        [JsonProperty("level")]
        public string LevelString { get; set; }

        [JsonIgnore]
        public ComboBoxPairs _Level { get; set; }

        [JsonIgnore]
        public ComboBoxPairs Level
        {
            get
            {
                return this._Level;
            }
            set
            {
                LevelString = value._Key;
                this._Level = value;
            }
        }

        public List<ComboBoxPairs> LevelItem { get; set; }
    }

    public class KeyValue
    {
        [JsonProperty("k")]
        public string k { get; set; }

        [JsonProperty("v")]
        public string v { get; set; }
    }

    public class GlobalSettings
    {
        public List<SnmpSetting> SnmpCM5000Settings { get; set; }
        public List<SnmpSetting> SnmpDR5000Settings { get; set; }
        public ObservableCollection<Alarm> AlarmSettings { get; set; }

        public string SnmpPort { get; set; }

        public string GetSnmpPort()
        {
            NameValueCollection nv = new NameValueCollection();
            nv.Add("request", "snmp_port");
            string uri = string.Format($"{HostManager.getInstance().uri}/api/v1/setting");
            string response = Http.Get(uri, nv);
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            KeyValue kv = JsonConvert.DeserializeObject<KeyValue>(response);
            return kv.v;
        }

        public void SetSnmpPort(string value)
        {
            NameValueCollection nv = new NameValueCollection();
            nv.Add("k", "snmp_port");
            nv.Add("v", value);

            string uri = string.Format($"{HostManager.getInstance().uri}/api/v1/setting");
            string response = Http.Put(uri, nv);

            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
        }

        public string GetPollingSec()
        {
            NameValueCollection nv = new NameValueCollection();
            nv.Add("request", "polling_sec");
            string uri = string.Format($"{HostManager.getInstance().uri}/api/v1/setting");
            string response = Http.Get(uri, nv);
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            KeyValue kv = JsonConvert.DeserializeObject<KeyValue>(response);
            return kv.v;
        }

        public void SetPollingSec(string value)
        {
            NameValueCollection nv = new NameValueCollection();
            nv.Add("k", "polling_sec");
            nv.Add("v", value);

            string uri = string.Format($"{HostManager.getInstance().uri}/api/v1/setting");
            string response = Http.Put(uri, nv);

            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
        }

        public string PollingSec { get; set; }
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
        public string Color { get; set; }
        public string TypeValue { get; set; }
        public string Oid { get; set; }
        public bool Enable { get; set; }
        public string Desc { get; set; }
        public int Channel { get; set; }
        public int Index { get; set; }
        public string Main { get; set; }
        public string TranslateValue { get; set; }
        public bool IsTypeTrap { get; set; } = false;
        public string TrapString { get; set; }
        public string TitanUID { get; set; }
        public string TitanName { get; set; }
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
                logString = string.Format($"{TranslateValue} ({TypeValue}) (Channel : {Channel})");
            }
            else
            {
                //logString = string.Format($"{TranslateValue} ({TypeValue})");
            }
            logger.Info(string.Format($"logString : {logString}"));
            return logString;
        }

        public static int GetSnmpPort()
        {
            int value = 0;
            string query = String.Format($"SELECT v FROM setting WHERE k = 'snmp_port'");
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.GetInstance().ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    value = Convert.ToInt32(rdr["v"].ToString());
                }
                rdr.Close();
            }
            return value;
        }

        public static bool IsEnableTrap(string compareID)
        {
            logger.Debug(string.Format($"compareID : {compareID}"));
            string value = null;
            string query = String.Format(@"SELECT S.id, T.translate FROM translate T
INNER JOIN snmp S ON S.name = T.name
WHERE T.is_enable = 'N'
AND T.is_visible = 'Y'");
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.GetInstance().ConnectionString))
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

        public static string GetLevelString(int level, string oid)
        {
            string levelstring = GetAlternateLevelString(oid.Substring(0, oid.Length - 1));
            if (!string.IsNullOrEmpty(levelstring))
            {
                return levelstring;
            }
            else
            {
                return Enum.GetName(typeof(EnumLevel), level);
            }
        }

        public static string GetAlternateLevelString(string oid)
        {
            string value = null;
            string query = String.Format($"SELECT S.id, T.name, T.level FROM snmp S INNER JOIN translate T ON T.name = S.name WHERE S.id like '{oid}%'");
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.GetInstance().ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    value = rdr["level"].ToString();
                }
                rdr.Close();
            }
            return value;
        }

        public static string GetNameFromOid(string oid)
        {
            string value = null;
            string query = String.Format($"SELECT name FROM snmp WHERE id = '{oid}'");
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.GetInstance().ConnectionString))
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
            /*
            DataTable dt = new DataTable();
            string query = String.Format($"SELECT * FROM translate WHERE type = '{type}' AND is_visible = 'Y'");
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.getInstance().ConnectionString))
            {
                conn.Open();
                MySqlDataAdapter adpt = new MySqlDataAdapter(query, conn);
                adpt.Fill(dt);
            }
            */

            string uri = string.Format($"{HostManager.getInstance().uri}/api/v1/setting/trap?type={type}");

            string response = Http.Get(uri, null);
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            List<SnmpSetting> set = JsonConvert.DeserializeObject<List<SnmpSetting>>(response);

            List<ComboBoxPairs> cbxLevel = new List<ComboBoxPairs>();
            cbxLevel.Add(new ComboBoxPairs { _Key = "", _Value = "Passthrough" });
            cbxLevel.Add(new ComboBoxPairs { _Key = "Critical", _Value = "Critical" });
            cbxLevel.Add(new ComboBoxPairs { _Key = "Warning", _Value = "Warning" });
            cbxLevel.Add(new ComboBoxPairs { _Key = "Information", _Value = "Information" });

            foreach (SnmpSetting s in set)
            {
                s.Level = new ComboBoxPairs { _Key = s.LevelString, _Value = s.LevelString };
                s.LevelItem = cbxLevel;
            }

            return set;

            /*
            List<SnmpSetting> settings = dt.AsEnumerable().Select(row => new SnmpSetting
            {
                Name = row.Field<string>("translate"),
                Idx = row.Field<int>("idx"),
                IsEnable = row["is_enable"].ToString() == "Y" ? true : false,
                Level = new ComboBoxPairs { _Key = row["level"].ToString(), _Value = row["level"].ToString() },
                LevelItem = cbxLevel
            }).ToList();

            return settings;
            */
        }

        public static string GetTranslateValue(string name)
        {
            string value = null;
            string query = String.Format($"SELECT translate FROM translate WHERE name = '{name}'");
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.GetInstance().ConnectionString))
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
                value = "";
            }
            return value;
        }

        public static void UpdateSnmpMessgeUseage(GlobalSettings settings)
        {
            foreach (var item in settings.SnmpCM5000Settings)
            {
                /*
                int ret = 0;
                string query = "UPDATE translate set is_enable = @is_enable, level = @level WHERE idx = @idx";
                using (MySqlConnection conn = new MySqlConnection(DatabaseManager.getInstance().ConnectionString))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@idx", item.Idx);
                    cmd.Parameters.AddWithValue("@is_enable", item.IsEnable == true ? "Y" : "N");
                    cmd.Parameters.AddWithValue("@level", item.Level._Key);
                    cmd.Prepare();
                    ret = cmd.ExecuteNonQuery();
                }
                */

                string jsonBody = JsonConvert.SerializeObject(item, Formatting.Indented, new MyJsonConveter());
                string uri = string.Format($"{HostManager.getInstance().uri}/api/v1/setting/trap");
                string response = Http.Post(uri, jsonBody);
            }

            foreach (var item in settings.SnmpDR5000Settings)
            {
                /*
                int ret = 0;
                string query = "UPDATE translate set is_enable = @is_enable, level = @level WHERE idx = @idx";
                using (MySqlConnection conn = new MySqlConnection(DatabaseManager.getInstance().ConnectionString))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@idx", item.Idx);
                    cmd.Parameters.AddWithValue("@is_enable", item.IsEnable == true ? "Y" : "N");
                    cmd.Parameters.AddWithValue("@level", item.Level._Key);
                    cmd.Prepare();
                    ret = cmd.ExecuteNonQuery();
                }
                */
                string jsonBody = JsonConvert.SerializeObject(item);
                string uri = string.Format($"{HostManager.getInstance().uri}/api/v1/setting/trap");
                string response = Http.Post(uri, jsonBody);
            }
        }

        public static void RegisterSnmpInfo(Snmp snmp)
        {
            string query = String.Format(@"INSERT INTO snmp (id, ip, syntax, community, type) VALUES (@id, @ip, @syntax, @community, @type) ON DUPLICATE KEY UPDATE edit_time = CURRENT_TIMESTAMP(), ip = @ip, syntax = @syntax, community = @community, type = @type");
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.GetInstance().ConnectionString))
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