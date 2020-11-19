using log4net;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
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

        public int Id { get; set; }
        public string Level { get; set; }
        public string _Path { get; set; }

        public string Path
        {
            get { return _Path; }
            set
            {
                if (_Path != value)
                {
                    _Path = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("Path"));
                }
            }
        }

        public string Ip { get; set; }

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
            DataTable dt = new DataTable();
            string query = string.Format($"SELECT A.* FROM alarm A WHERE A.ip = '{Utils.Util.GetLocalIpAddress()}'");
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.getInstance().ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Prepare();
                MySqlDataAdapter adpt = new MySqlDataAdapter(query, conn);
                adpt.Fill(dt);
            }

            return dt.AsEnumerable().Select(row => new Alarm
            {
                Id = row.Field<int>("id"),
                Level = row.Field<string>("level"),
                Path = row.Field<string>("path"),
                Ip = row.Field<string>("ip")
            }).ToList();
        }

        public static int UpdateAlarmInfo(Alarm alarm)
        {
            int ret = 0;
            alarm.Ip = Utils.Util.GetLocalIpAddress();
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.getInstance().ConnectionString))
            {
                string query = string.Format(@"INSERT INTO alarm (level, path, ip) VALUES (@level, @path, @ip) ON DUPLICATE KEY UPDATE path = @path, Ip = @ip");
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@level", alarm.Level);
                cmd.Parameters.AddWithValue("@path", alarm.Path);
                cmd.Parameters.AddWithValue("@ip", alarm.Ip);
                cmd.Prepare();
                ret = cmd.ExecuteNonQuery();
            }

            return ret;
        }
    }
}