using log4net;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NmsDotnet.config;
using NmsDotnet.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NmsDotnet.Database.vo
{
    public class Alarm : INotifyPropertyChanged
    {
        private static readonly ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Alarm()
        {
        }

        public int id { get; set; }
        public string level { get; set; }
        public string _path { get; set; }

        public string path
        {
            get { return _path; }
            set
            {
                if (_path != value)
                {
                    _path = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("Path"));
                }
            }
        }

        public string ip { get; set; }

        public Server.EnumStatus Uid { get; set; }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
                if (e.PropertyName.Equals("Path"))

                {
                    UpdateAlarmInfo(this);
                }
            }
        }

        public static List<Alarm> GetAlarmInfo()
        {
            /*
            string query = string.Format($"SELECT A.* FROM alarm A WHERE A.ip = '{Utils.Util.GetLocalIpAddress()}'");
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.getInstance().ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Prepare();
                MySqlDataAdapter adpt = new MySqlDataAdapter(query, conn);
                adpt.Fill(dt);
            }
            */
            NameValueCollection nv = new NameValueCollection();
            nv.Add("ip", Util.GetLocalIpAddress());
            string uri = string.Format($"{HostManager.getInstance().uri}/api/v1/setting/alarm");

            string response = Http.Get(uri, nv);
            //DataTable dt = (DataTable)JsonConvert.DeserializeObject(response, (typeof(DataTable)));
            //JObject applyJObj = JObject.Parse(response);
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            //DataTable dt = (DataTable)JsonConvert.DeserializeObject<DataTable>(response, settings);
            return JsonConvert.DeserializeObject<List<Alarm>>(response);
            /*
            return dt.AsEnumerable().Select(row => new Alarm
            {
                Id = row.Field<int>("id"),
                Level = row.Field<string>("level"),
                Path = row.Field<string>("path"),
                Ip = row.Field<string>("ip")
            }).ToList();
            */
        }

        public static int UpdateAlarmInfo(Alarm alarm)
        {
            int ret = 0;
            alarm.ip = Utils.Util.GetLocalIpAddress();
            /*
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.getInstance().ConnectionString))
            {
                string query = string.Format(@"INSERT INTO alarm (level, path, ip) VALUES (@level, @path, @ip) ON DUPLICATE KEY UPDATE path = @path, Ip = @ip");
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@level", alarm.level);
                cmd.Parameters.AddWithValue("@path", alarm.path);
                cmd.Parameters.AddWithValue("@ip", alarm.ip);
                cmd.Prepare();
                ret = cmd.ExecuteNonQuery();
            }
            */
            string jsonBody = JsonConvert.SerializeObject(alarm);
            string uri = string.Format($"{HostManager.getInstance().uri}/api/v1/setting/alarm");
            Http.Post(uri, jsonBody);

            return ret;
        }
    }
}