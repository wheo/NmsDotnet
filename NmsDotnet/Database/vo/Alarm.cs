using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NmsDotnet.Database.vo
{
    public class Alarm
    {
        public Alarm()
        {
        }

        public int Id { get; set; }
        public string Level { get; set; }
        public string Path { get; set; }

        public static List<Alarm> GetAlarmInfo()
        {
            DataTable dt = new DataTable();
            string query = @"SELECT A.* FROM alarm A";
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
                Path = row.Field<string>("path")
            }).ToList();
        }

        public int UpdateAlarmInfo()
        {
            int ret = 0;
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.getInstance().ConnectionString))
            {
                string query = string.Format(@"INSERT INTO alarm (level, path) VALUES (@level, @path) ON DUPLICATE KEY UPDATE path = @path");
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@level", this.Level);
                cmd.Parameters.AddWithValue("@path", this.Path);
                cmd.Prepare();
                ret = cmd.ExecuteNonQuery();
            }

            return ret;
        }
    }
}