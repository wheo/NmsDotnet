using log4net;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;

namespace NmsDotNet.Database.vo
{
    public class Group
    {
        private ObservableCollection<Server> DataSource = new ObservableCollection<Server>();

        private static readonly ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public string Id { get; set; }
        public string Name { get; set; }

        public List<Server> Servers { get; set; }

        //public static Group group;

        public static int AddGroup(Group grp)
        {
            int ret = 0;
            string query = "INSERT INTO grp (id, name) VALUES (uuid(), @name)";
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.getInstance().ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@name", grp.Name);
                cmd.Prepare();
                ret = cmd.ExecuteNonQuery();
            }
            return ret;
        }

        public static int EditGroup(Group grp)
        {
            int ret = 0;
            string query = "UPDATE grp set name = @name WHERE id = @id";
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.getInstance().ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", grp.Id);
                cmd.Parameters.AddWithValue("@name", grp.Name);
                cmd.Prepare();
                ret = cmd.ExecuteNonQuery();
            }
            return ret;
        }

        public static int DeleteGroup(string id)
        {
            int ret = 0;
            string query = "DELETE FROM grp WHERE id = @id";
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.getInstance().ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Prepare();
                ret = cmd.ExecuteNonQuery();
            }
            return ret;
        }

        public static IEnumerable<Group> GetGroupList()
        {
            DataTable dt = new DataTable();
            string query = "SELECT G.* FROM grp G ORDER BY G.create_time";
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.getInstance().ConnectionString))
            {
                conn.Open();
                /*
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Prepare();
                */
                MySqlDataAdapter adpt = new MySqlDataAdapter(query, conn);
                adpt.Fill(dt);
            }

            List<Group> groups = dt.AsEnumerable().Select(row => new Group
            {
                Id = row.Field<string>("id"),
                Name = row.Field<string>("name")
            }).ToList();

            foreach (Group g in groups)
            {
                g.Servers = Server.GetServerListByGroup(g.Id);
            }

            return groups;
        }
    }
}