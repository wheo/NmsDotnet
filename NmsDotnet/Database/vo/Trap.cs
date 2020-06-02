using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using NmsDotNet.Database;

namespace NmsDotnet.Database.vo
{
    /// <summary>
    /// Trap을 수신할 때 정보를 데이터베이스에 기록
    /// </summary>

    class Trap
    {        
        public string Id { get; set; }        
        public string Type { get; set; }
        public string IP { get; set; }
        public string Port { get; set; }
        public string Community { get; set; }
        public string Value { get; set; }
        public bool Enable { get; set; }
        public string Desc { get; set; }

        public static Trap trap;
        public static Trap GetInstance()
        {
            if (trap == null)
            {
                trap = new Trap();
            }
            return trap;
        }
        public void RegisterTrapInfo(Trap trap)
        {
            string query = String.Format(@"INSERT INTO trap (id, ip, type, community) VALUES (@id, @ip, @type, @community) ON DUPLICATE KEY UPDATE edit_time = CURRENT_TIMESTAMP(), ip = @ip, type = @type, community = @community");
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.getInstance().ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", trap.Id);
                cmd.Parameters.AddWithValue("@ip", trap.IP);
                cmd.Parameters.AddWithValue("@type", trap.Type);
                cmd.Parameters.AddWithValue("@community", trap.Community);
                cmd.Prepare();
                cmd.ExecuteNonQuery();
            }
        }
    }
}