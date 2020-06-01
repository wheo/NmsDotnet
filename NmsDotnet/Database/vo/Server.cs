using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace NmsDotNet.Database.vo
{
    class Server
    {
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }

        public static Server server;
        public static Server GetInstance()
        {
            if (server == null)
            {
                server = new Server();
            }
            return server;
        }

        public void AddServer(Server server)
        {
            string query = "INSERT INTO server (id, name) VALUES (uuid(), @name)";
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.getInstance().ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", server.name);
                cmd.Prepare();
                cmd.ExecuteNonQuery();
            }
        }

        public List<Server> GetServerList()
        {   
            DataTable dt = new DataTable();
            string query = "SELECT * FROM server";
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
                id = row.Field<string>("id"),
                name = row.Field<string>("name")
            }).ToList();
        }
    }
}
