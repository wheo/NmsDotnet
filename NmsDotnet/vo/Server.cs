using log4net;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using NmsDotnet.config;
using NmsDotnet.Database.vo;
using NmsDotnet.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;

namespace NmsDotnet.Database.vo
{
    public class Server : INotifyPropertyChanged
    {
        private static readonly ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Server ShallowCopy()
        {
            return (Server)this.MemberwiseClone();
        }

        public void PutInfo(Server s)
        {
            this.Ip = s.Ip;
            this.UnitName = s.UnitName;
            this.status = s.status;
            this.Color = s.Color;
            this.Location = s.Location;
            this.Type = s.Type;
            this.Uptime = s.Uptime;
            this.Version = s.Version;
        }

        [JsonIgnore]
        public Server Undo { get; set; }

        [JsonIgnore]
        private string _Status;

        [JsonIgnore]
        private string _Type;

        [JsonIgnore]
        private string _Color;

        [JsonIgnore]
        public string Uptime { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("ip")]
        public string Ip { get; set; }

        [JsonIgnore]
        public int _Location { get; set; }

        [JsonProperty("location")]
        public int Location
        {
            get { return _Location; }
            set
            {
                if (_Location != value)
                {
                    _Location = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("Location"));
                }
            }
        }

        [JsonIgnore]
        public string _UnitName { get; set; }

        [JsonProperty("name")]
        public string UnitName
        {
            get
            { return _UnitName; }
            set
            {
                if (_UnitName != value)
                {
                    _UnitName = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("UnitName"));
                }
            }
        }

        [JsonProperty("gid")]
        public string Gid { get; set; }

        [JsonIgnore]
        public ObservableCollection<Group> Groups { get; set; }

        [JsonProperty("grp_name")]
        public string Grp_name { get; set; }

        [JsonIgnore]
        public string Version { get; set; }

        [JsonIgnore]
        public int _ServicePid { get; set; }

        [JsonIgnore]
        public int ServicePid
        {
            get
            {
                return _ServicePid;
            }
            set
            {
                if (_ServicePid != value)
                {
                    _ServicePid = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("ServicePid"));
                }
            }
        }

        [JsonIgnore]
        public int _VideoOutputId { get; set; }

        [JsonIgnore]
        public int VideoOutputId
        {
            get
            {
                return _VideoOutputId;
            }
            set
            {
                if (_VideoOutputId != value)
                {
                    _VideoOutputId = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("VideoOutputId"));
                }
            }
        }

        [JsonProperty("type")]
        public string Type
        {
            get { return _Type; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    HeaderType = '\0';
                }
                else
                {
                    if (value[0] == 'C')
                    {
                        HeaderType = 'E'; //CM5000 to 'E'ncoder
                    }
                    else if (value[0] == 'D')
                    {
                        HeaderType = 'D'; //DR5000 to 'D'ecoder
                    }
                    else if (value[0] == 'T')
                    {
                        HeaderType = 'U'; // Titan Live to 'U'HD encoder
                    }
                }
                if (_Type != value)
                {
                    _Type = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("HeaderType"));
                }
            }
        }

        [JsonIgnore]
        public char HeaderType { get; set; }

        //public ObservableCollection<Group> Groups { get; set; }

        //public List<Group> Groups { get; set; }
        [JsonProperty("error_count")]
        public int ErrorCount { get; set; }

        [JsonProperty("color")]
        public string Color
        {
            get { return _Color; }
            set
            {
                if (_Color != value)
                {
                    _Color = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("Color"));
                }
            }
        }

        [JsonIgnore]
        public string Message { get; set; }

        [JsonIgnore]
        public string status
        {
            get { return _Status; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    if (_Status != value)
                    {
                        logger.Info(String.Format($"[{Ip}] Server ({_Status}) => ({value}) changed"));

                        if (value.ToLower().Equals("normal"))
                        {
                            this.Color = "#00FF7F";
                            this.Message = "Normal status";
                        }
                        else if (value.ToLower().Equals("critical"))
                        {
                            this.Color = "#EE0000";
                        }
                        else if (value.ToLower().Equals("warning"))
                        {
                            this.Color = "#FF8000";
                        }
                        else if (value.ToLower().Equals("information"))
                        {
                            this.Color = "#0000FF";
                        }
                        else
                        {
                            this.Color = "#CCECFF";
                        }
                        _Status = value;
                        OnPropertyChanged(new PropertyChangedEventArgs("Status"));
                    }
                }
                else
                {
                    _Status = null;
                }
            }
        }

        [JsonIgnore]
        public List<MenuItem> MenuItems { get; set; }

        public enum EnumStatus
        {
            Normal = 0,
            Disabled = 2,
            Information = 3,
            Warning = 4,
            Critical = 5
        }

        [JsonIgnore]
        public EnumIsConnect _IsConnect { get; set; }

        [JsonIgnore]
        public EnumIsConnect IsConnect
        {
            get { return _IsConnect; }
            set
            {
                _IsConnect = value;
                //logger.Info(string.Format($"[{Ip}] connect status is {c.ToString()}"));
            }
        }

        public enum EnumIsConnect
        {
            Init = 0,
            Connect = 1,
            Disconnect = 2
        }

        [JsonIgnore]
        public int connection_error_count { get; set; }

        public Server()
        {
            _Status = "";
            IsConnect = (int)EnumIsConnect.Init;
            connection_error_count = 0;
        }

        public int GetNewLocation()
        {
            try
            {
                Location = NmsInfo.GetInstance().serverList.Max(x => x.Location) + 1;
            }
            catch
            {
                Location = 0;
            }
            return Location;
        }

        public void Clear()
        {
            this.Ip = null;
            this.Ip = null;
            this.Location = 0;
            this.Color = null;
            this.Gid = null;
            this.Grp_name = null;
            this.Groups = null;
            this.VideoOutputId = 0;
            this.ServicePid = 0;
            this.Type = null;
            this.Message = null;
            this.IsConnect = EnumIsConnect.Init;
            this.HeaderType = '\0';
            this.status = null;
            this.UnitName = null;
            this.Version = null;
            this.ErrorCount = 0;
            this.connection_error_count = 0;
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
                if (e.PropertyName.Equals("Status") ||
                    e.PropertyName.Equals("Name") ||
                        e.PropertyName.Equals("Location"))
                {
                    UpdateServerStatus();
                }
            }
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
            /*
            string id = null;
            if (String.IsNullOrEmpty(UnitName))
            {
            }
            else if (String.IsNullOrEmpty(Ip))
            {
            }
            else if (String.IsNullOrEmpty(gid))
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

            query = "INSERT INTO server (id, ip, name, gid) VALUES (@id, @ip, @name, @gid)";
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.getInstance().ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@name", this.UnitName);
                cmd.Parameters.AddWithValue("@ip", this.Ip);
                cmd.Parameters.AddWithValue("@gid", this.gid);
                cmd.Prepare();
                cmd.ExecuteNonQuery();
            }
            return id;
            */
            //this.Id = Guid.NewGuid().ToString();
            string jsonBody = JsonConvert.SerializeObject(this);
            string uri = string.Format($"{HostManager.getInstance().uri}/api/v1/server");
            string response = Http.Put(uri, jsonBody);
            return this.Id;
        }

        public int EditServer()
        {
            int ret = 1;
            /*
            string query = "UPDATE server set ip = @ip, name = @name, gid = @gid WHERE id = @id";
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.getInstance().ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", this.Id);
                cmd.Parameters.AddWithValue("@ip", this.Ip);
                cmd.Parameters.AddWithValue("@name", this.UnitName);
                cmd.Parameters.AddWithValue("@gid", this.gid);
                cmd.Prepare();
                ret = cmd.ExecuteNonQuery();
            }
            */
            string jsonBody = JsonConvert.SerializeObject(this);
            string uri = string.Format($"{HostManager.getInstance().uri}/api/v1/server");
            string response = Http.Post(uri, jsonBody);
            ret = 1;
            return ret;
        }

        public static int DeleteServer(Server server)
        {
            int ret = 0;
            /*
            string query = "DELETE FROM server WHERE id = @id";
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.getInstance().ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", server.Id);
                cmd.Prepare();
                ret = cmd.ExecuteNonQuery();
            }
            */

            string jsonBody = JsonConvert.SerializeObject(server);
            string uri = string.Format($"{HostManager.getInstance().uri}/api/v1/server");
            string response = Http.Delete(uri, jsonBody);
            logger.Info(response);
            ret = 1;
            return ret;
        }

        public int UpdateServerStatus()
        {
            int ret = 0;
            /*
            string query = "UPDATE server set status = @status, type = @type, name = @name, location = @location, error_count = @error_count, connection_error_count = @connection_error_count WHERE id = @id";
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.getInstance().ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", server.id);
                cmd.Parameters.AddWithValue("@status", server.status);
                cmd.Parameters.AddWithValue("@name", server.name);
                cmd.Parameters.AddWithValue("@location", server.location);
                cmd.Parameters.AddWithValue("@type", server.type);
                cmd.Parameters.AddWithValue("@error_count", server.ErrorCount);
                cmd.Parameters.AddWithValue("@connection_error_count", server.connection_error_count);
                cmd.Prepare();
                ret = cmd.ExecuteNonQuery();
            }
            */
            string jsonBody = JsonConvert.SerializeObject(this);
            string uri = string.Format($"{HostManager.getInstance().uri}/api/v1/server");
            string response = Http.Post(uri, jsonBody);
            return ret;
        }

        //deprecated
        /*
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
        */

        public static bool ImportServer(ObservableCollection<Server> servers)
        {
            /* 1.transation
             * 2. Delete server table
             * 3. insert new server info
             * 4. commit
             */

            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.GetInstance().ConnectionString))
            {
                int ret = 0;
                conn.Open();
                MySqlTransaction trans = conn.BeginTransaction();
                try
                {
                    string query = String.Format($"DELETE FROM server");
                    MySqlCommand cmd = conn.CreateCommand();
                    cmd.CommandText = query;
                    cmd.Prepare();
                    ret = cmd.ExecuteNonQuery();

                    query = "INSERT INTO server (id, ip, name, gid, location) VALUES (@id, @ip, @name, @gid, @location)";
                    cmd.CommandText = query;
                    foreach (Server s in servers)
                    {
                        if (!string.IsNullOrEmpty(s.Id))
                        {
                            cmd.Parameters.AddWithValue("@id", s.Id);
                            cmd.Parameters.AddWithValue("@name", s.UnitName);
                            cmd.Parameters.AddWithValue("@ip", s.Ip);
                            cmd.Parameters.AddWithValue("@gid", s.Gid);
                            cmd.Parameters.AddWithValue("@location", s.Location);
                            cmd.Prepare();
                            cmd.ExecuteNonQuery();
                            cmd.Parameters.Clear();
                        }
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

        public static List<Server> GetServerList()
        {
            try
            {
                string uri = string.Format($"{HostManager.getInstance().uri}/api/v1/server");

                string response = Http.Get(uri, null);
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };

                //DataTable dt = (DataTable)JsonConvert.DeserializeObject<DataTable>(response, settings);
                return JsonConvert.DeserializeObject<List<Server>>(response);
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                return null;
            }
            /*
            DataTable dt = new DataTable();
            string query = @"SELECT S.*
, IF(S.status = 'Critical', 'Red', IF(S.status = 'Warning', '#FF8000', IF(S.status = 'Information', 'Blue', 'Green'))) AS color
, G.name as grp_name
, A.path FROM server S
LEFT JOIN asset A ON S.status = A.id
LEFT JOIN grp G ON G.id = S.gid
ORDER BY S.location ASC";
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
                gid = row.Field<string>("gid"),
                name = row.Field<string>("name"),
                ip = row.Field<string>("ip"),
                grp_name = row.Field<string>("grp_name"),
                type = row.Field<string>("type"),
                location = row.Field<int>("location"),
                //ErrorCount = row.Field<int>("error_count"),
                ErrorCount = 0,
                connection_error_count = row.Field<int>("connection_error_count"),
                color = row.Field<string>("color"),
                status = row.Field<string>("status")
            }).ToList();
            */
        }

        //deprecated 2020-10-29
        /*
        public static List<Server> GetServerListByGroup(string gid)
        {
            DataTable dt = new DataTable();
            string query = String.Format($"SELECT S.*" +
                $", IF(S.status = 'critical', 'Red', IF(S.status = 'warning', '#FF8000', IF(S.status = 'information', 'Blue', 'Green'))) AS color" +
                $", G.name as grp_name" +
                $", A.path FROM server S" +
                $" LEFT JOIN asset A ON S.status = A.id" +
                $" LEFT JOIN grp G ON G.id = S.gid" +
                $" WHERE S.gid = '{gid}'");
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
                Image = row.Field<string>("path"),
                Color = row.Field<string>("color")
            }).ToList();
        }
        */
    }
}