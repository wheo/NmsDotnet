using log4net;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using NmsDotnet.Database.vo;

using Newtonsoft.Json;
using System;
using System.Diagnostics;
using NmsDotnet.config;
using NmsDotnet.Utils;

namespace NmsDotnet.Database.vo
{
    public class Group
    {
        //private ObservableCollection<Server> DataSource = new ObservableCollection<Server>();

        private static readonly ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [JsonProperty("gid")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        // List를 ObservableCollection 으로 만들어주기때문에 List로 할당해도 될듯?(내생각) ???
        [JsonProperty("server")]
        public ObservableCollection<Server> Server { get; set; }

        public static bool ImportGroup(ObservableCollection<Group> groups)
        {
            /* 1.transation
             * 2. Delete server table
             * 3. insert new group info
             * 4. commit
             */

            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.GetInstance().ConnectionString))
            {
                conn.Open();
                int ret = 0;
                MySqlTransaction trans = conn.BeginTransaction();
                try
                {
                    string query = String.Format("DELETE FROM grp");
                    MySqlCommand cmd = conn.CreateCommand();
                    cmd.CommandText = query;
                    cmd.Prepare();
                    ret = cmd.ExecuteNonQuery();

                    query = "INSERT INTO grp (id, name) VALUES (@id, @name)";

                    cmd.CommandText = query;
                    foreach (Group g in groups)
                    {
                        cmd.Parameters.AddWithValue("@id", g.Id);
                        cmd.Parameters.AddWithValue("@name", g.Name);
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                    }
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    logger.Error(ex.ToString());
                    trans.Rollback();
                    return false;
                }
            }

            return true;
        }

        public string AddGroup()
        {
            /*
            string id = null;
            string query = "SELECT uuid() as id";

            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.getInstance().ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    id = rdr["id"].ToString();
                }
                rdr.Close();
            }
            grp.Id = id;

            int ret = 0;
            query = "INSERT INTO grp (id, name) VALUES (@id, @name)";
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.getInstance().ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", grp.Id);
                cmd.Parameters.AddWithValue("@name", grp.Name);
                cmd.Prepare();
                ret = cmd.ExecuteNonQuery();
            }
            */

            this.Id = Guid.NewGuid().ToString();
            string jsonBody = JsonConvert.SerializeObject(this);
            string uri = string.Format($"{HostManager.getInstance().uri}/api/v1/group");
            string response = Http.Put(uri, jsonBody);
            return this.Id;
        }

        public int EditGroup()
        {
            int ret = 0;
            /*
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
            */

            string jsonBody = JsonConvert.SerializeObject(this);
            string uri = string.Format($"{HostManager.getInstance().uri}/api/v1/group");
            string response = Http.Post(uri, jsonBody);
            ret = 1;
            return ret;
        }

        public int DeleteGroup()
        {
            int ret = 0;
            /*
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
            */

            string jsonBody = JsonConvert.SerializeObject(this);
            string uri = string.Format($"{HostManager.getInstance().uri}/api/v1/group");
            string response = Http.Delete(uri, jsonBody);
            ret = 1;
            return ret;
        }

        public static List<Group> GetGroupList()
        {
            string uri = string.Format($"{HostManager.getInstance().uri}/api/v1/group");

            string response = Http.Get(uri, null);
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            //DataTable dt = (DataTable)JsonConvert.DeserializeObject<DataTable>(response, settings);
            List<Group> groups = JsonConvert.DeserializeObject<List<Group>>(response);

            foreach (Group g in groups)
            {
                if (g.Server == null)
                {
                    g.Server = new ObservableCollection<Server>();
                }
            }

            foreach (Server s in NmsInfo.GetInstance().serverList)
            {
                foreach (Group g in groups)
                {
                    if (s.Gid == g.Id)
                    {
                        Server temp = (Server)g.Server.Where(x => x.Gid == g.Id).FirstOrDefault();
                        g.Server.Remove(temp);
                        g.Server.Add(s);
                        break;
                    }
                }
            }

            return groups;
#if false
            DataTable dt = new DataTable();
            string query = "SELECT G.* FROM grp G ORDER BY G.create_time";
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.getInstance().ConnectionString))
            {
                conn.Open();
                /*
                MySqlCommand cmd = new MySqlCommand(query, conn)
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
                if (g.Servers == null)
                {
                    g.Servers = new ObservableCollection<Server>();
                }
            }

            foreach (Server s in NmsInfo.GetInstance().serverList)
            {
                foreach (Group g in groups)
                {
                    if (s.gid == g.Id)
                    {
                        g.Servers.Add(s);
                    }
                }
            }

            return groups;
#endif
        }
    }
}