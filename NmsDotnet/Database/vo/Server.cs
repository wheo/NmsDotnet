using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Relational;

namespace NmsDotNet.Database.vo
{
    public class Server : INotifyPropertyChanged
    {
        private string _Status;
        public string Id { get; set; }
        public string Ip { get; set; }
        public string Name { get; set; }
        public string Gid { get; set; }
        public string GroupName { get; set; }
        public string Type { get; set; }
        public List<Group> Groups { get; set; }

        public enum EnumStatus
        {
            Idle = 0,
            Normal = 1,
            Disabled = 2,
            Information = 3,
            Warning = 4,
            Critical = 5
        }

        public string Status
        {
            get { return _Status; }
            set
            {
                _Status = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Status"));
            }
        }

        public string Image { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
                if (e.PropertyName.Equals("Status"))
                {
                    UpdateServerStatus(this);
                }
            }
        }

        public void SetServerInfo(string name, string ip, string gid)
        {
            Name = name;
            Ip = ip;
            Gid = gid;
        }

        public static string CompareState(string a, string b)
        {
            a = char.ToUpper(a[0]) + a.Substring(1);
            b = char.ToUpper(b[0]) + b.Substring(1);
            EnumStatus e1 = (EnumStatus)Enum.Parse(typeof(EnumStatus), a);
            EnumStatus e2 = (EnumStatus)Enum.Parse(typeof(EnumStatus), b);
            string str = null;

            if (e1 > e2)
            {
                Debug.WriteLine(e1);
                Debug.WriteLine(e2);
                str = Enum.GetName(typeof(EnumStatus), e1);
            }
            else
            {
                Debug.WriteLine(e1);
                Debug.WriteLine(e2);
                str = Enum.GetName(typeof(EnumStatus), e2);
            }

            return str.ToLower();
        }

        public string AddServer()
        {
            string id = null;

            if (String.IsNullOrEmpty(Name))
            {
            }
            else if (String.IsNullOrEmpty(Ip))
            {
            }
            else if (String.IsNullOrEmpty(Gid))
            {
            }
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

            query = "INSERT INTO server (id, ip, name, gid) VALUES (uuid(), @ip, @name, @gid)";
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.getInstance().ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                //cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@name", this.Name);
                cmd.Parameters.AddWithValue("@ip", this.Ip);
                cmd.Parameters.AddWithValue("@gid", this.Gid);
                cmd.Prepare();
                cmd.ExecuteNonQuery();
            }
            return id;
        }

        public int EditServer()
        {
            int ret = 0;
            string query = "UPDATE server set ip = @ip, name = @name, gid = @gid WHERE id = @id";
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.getInstance().ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", this.Id);
                cmd.Parameters.AddWithValue("@ip", this.Ip);
                cmd.Parameters.AddWithValue("@name", this.Name);
                cmd.Parameters.AddWithValue("@gid", this.Gid);
                cmd.Prepare();
                ret = cmd.ExecuteNonQuery();
            }
            return ret;
        }

        public static int DeleteServer(Server server)
        {
            int ret = 0;
            string query = "DELETE FROM server WHERE id = @id";
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.getInstance().ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", server.Id);
                cmd.Prepare();
                ret = cmd.ExecuteNonQuery();
            }
            return ret;
        }

        public static int UpdateServerStatus(Server server)
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

        public static bool ValidServerIP(string ip)
        {
            bool ret = false;
            string query = String.Format($"SELECT ip FROM server WHERE ip = '{ip}'");
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.getInstance().ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    ret = true;
                }
                rdr.Close();
            }
            return ret;
        }

        public static List<Server> GetServerList()
        {
            List<Group> g = (List<Group>)Group.GetGroupList();

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
                Image = row.Field<string>("path"),
                Groups = g
            }).ToList();
        }

        public static List<Server> GetServerListByGroup(string gid)
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