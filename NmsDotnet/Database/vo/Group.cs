using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using NmsDotNet.Database;
using Org.BouncyCastle.Crypto.Engines;

namespace NmsDotNet.Database.vo
{
    internal class Group
    {
        private ObservableCollection<Server> DataSource = new ObservableCollection<Server>();

        private Group()
        {
        }

        public string Id { get; set; }
        public string Name { get; set; }

        public static Group group;

        public static Group GetInstance()
        {
            if (group == null)
            {
                group = new Group();
            }
            return group;
        }

        public int AddGroup(string name)
        {
            int ret = 0;
            string query = "INSERT INTO grp (id, name) VALUES (uuid(), @name)";
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.getInstance().ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Prepare();
                ret = cmd.ExecuteNonQuery();
            }
            return ret;
        }

        public List<Group> GetGroupList()
        {
            DataTable dt = new DataTable();
            string query = "SELECT * FROM grp";
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.getInstance().ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Prepare();
                MySqlDataAdapter adpt = new MySqlDataAdapter(query, conn);
                adpt.Fill(dt);
            }

            return dt.AsEnumerable().Select(row => new Group
            {
                Id = row.Field<string>("id"),
                Name = row.Field<string>("name")
            }).ToList();
        }
    }
}