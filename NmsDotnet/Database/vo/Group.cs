using log4net;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using NmsDotnet.Database.vo;

using NmsDotnet.Database.vo;

namespace NmsDotnet.Database.vo
{
    public class Group
    {
        //private ObservableCollection<Server> DataSource = new ObservableCollection<Server>();

        private static readonly ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public string Id { get; set; }
        public string Name { get; set; }

        // List를 ObservableCollection 으로 만들어주기때문에 List로 할당해도 될듯?(내생각)
        public ObservableCollection<Server> Servers { get; set; }

        //public ObservableCollection<Server> Servers { get; set; }

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

        public static List<Group> GetGroupList()
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

            //g.Servers = Server.GetServerListByGroup(g.Id);
            foreach (Server s in NmsInfo.GetInstance().serverList)
            {
                foreach (Group g in groups)
                {
                    if (g.Servers == null)
                    {
                        g.Servers = new ObservableCollection<Server>();
                    }
                    if (s.Gid == g.Id)
                    {
                        g.Servers.Add(s);
                    }
                }
            }

            return groups;
        }
    }
}