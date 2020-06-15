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

    class Snmp
    {        
        public string Id { get; set; }        
        public string Syntax { get; set; }

        public string type { get; set; }
        public string IP { get; set; }
        public string Port { get; set; }
        public string Community { get; set; }
        public string Value { get; set; }
        public bool Enable { get; set; }
        public string Desc { get; set; }

        public static Snmp trap;
        public static Snmp GetInstance()
        {
            if (trap == null)
            {
                trap = new Snmp();
            }
            return trap;
        }
        public void RegisterSnmpInfo(Snmp snmp)
        {
            string query = String.Format(@"INSERT INTO snmp (id, ip, syntax, community, type, value) VALUES (@id, @ip, @syntax, @community, @type, @value) ON DUPLICATE KEY UPDATE edit_time = CURRENT_TIMESTAMP(), ip = @ip, syntax = @syntax, community = @community, type = @type, value = @value");
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.getInstance().ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", snmp.Id);
                cmd.Parameters.AddWithValue("@ip", snmp.IP);
                cmd.Parameters.AddWithValue("@syntax", snmp.Syntax);
                cmd.Parameters.AddWithValue("@community", snmp.Community);
                cmd.Parameters.AddWithValue("@type", snmp.type);
                cmd.Parameters.AddWithValue("@value", snmp.Value);
                cmd.Prepare();
                cmd.ExecuteNonQuery();
            }
        }
    }
}