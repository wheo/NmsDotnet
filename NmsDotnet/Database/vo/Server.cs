using System;
using System.Collections.Generic;
using System.Data;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Relational;

namespace NmsDotNet.Database.vo
{
    internal class Server
    {
        public string Id { get; set; }
        public string Ip { get; set; }
        public string Name { get; set; }
        public string Gid { get; set; }
        public String GroupName { get; set; }
        public string Type { get; set; }

        public string Status { get; set; }

        public string Image { get; set; }

        private Server()
        {
        }

        public static Server server;

        public static Server GetInstance()
        {
            if (server == null)
            {
                server = new Server();
            }
            return server;
        }

        public void SetServerInfo(string name, string ip, string gid)
        {
            Name = name;
            Ip = ip;
            Gid = gid;
        }

        public void AddServer()
        {
            if (String.IsNullOrEmpty(Name))
            {
            }
            else if (String.IsNullOrEmpty(Ip))
            {
            }
            else if (String.IsNullOrEmpty(Type))
            {
            }
            else if (String.IsNullOrEmpty(Gid))
            {
            }

            string query = "INSERT INTO server (id, ip, name, type, gid) VALUES (uuid(), @ip, @name, @type, @gid)";
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.getInstance().ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@name", server.Name);
                cmd.Parameters.AddWithValue("@ip", server.Ip);
                cmd.Parameters.AddWithValue("@type", server.Type);
                cmd.Parameters.AddWithValue("@gid", server.Gid);
                cmd.Prepare();
                cmd.ExecuteNonQuery();
            }
        }

        public int UpdateServerStatus(Server server)
        {
            int ret = 0;
            string query = "UPDATE server set status = @status WHERE id = @id";
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.getInstance().ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", server.Id);
                cmd.Parameters.AddWithValue("@status", server.Status);
                cmd.Prepare();
                ret = cmd.ExecuteNonQuery();
            }
            return ret;
        }

        public List<Server> GetServerList()
        {
            DataTable dt = new DataTable();
            string query = @"SELECT S.*, G.name as grp_name, A.path FROM server S LEFT JOIN asset A ON S.status = A.id LEFT JOIN grp G ON G.id = S.gid ORDER BY G.name, S.create_time";
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.getInstance().ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Prepare();
                MySqlDataAdapter adpt = new MySqlDataAdapter(query, conn);
                adpt.Fill(dt);
            }

            return dt.AsEnumerable().Select(row => new Server
            {
                Id = row.Field<string>("id"),
                Gid = row.Field<string>("gid"),
                Name = row.Field<string>("name"),
                Ip = row.Field<string>("ip"),
                GroupName = row.Field<string>("grp_name"),
                Status = row.Field<string>("status"),
                Image = row.Field<string>("path")
            }).ToList();
        }

        public List<Server> GetServerListByGroup(string gid)
        {
            DataTable dt = new DataTable();
            string query = String.Format($"SELECT S.*, G.name as grp_name, A.path FROM server S LEFT JOIN asset A ON S.status = A.id LEFT JOIN grp G ON G.id = S.gid WHERE S.gid = '{gid}'");
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.getInstance().ConnectionString))
            {
                conn.Open();
                MySqlDataAdapter adpt = new MySqlDataAdapter(query, conn);
                adpt.Fill(dt);
            }

            return dt.AsEnumerable().Select(row => new Server
            {
                Id = row.Field<string>("id"),
                Gid = row.Field<string>("gid"),
                Name = row.Field<string>("name"),
                GroupName = row.Field<string>("grp_name"),
                Status = row.Field<string>("status"),
                Image = row.Field<string>("path")
            }).ToList();
        }
    }
}