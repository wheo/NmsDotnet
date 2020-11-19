﻿using log4net;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using NmsDotnet.Database.vo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;

namespace NmsDotnet.Database.vo
{
    public class Server : INotifyPropertyChanged
    {
        private static readonly ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [JsonIgnore]
        private string _Status;

        [JsonIgnore]
        private string _Type;

        [JsonIgnore]
        private string _Color;

        public string Id { get; set; }
        public string Ip { get; set; }

        [JsonIgnore]
        public int _Location { get; set; }

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

        public string Gid { get; set; }

        [JsonIgnore]
        public ObservableCollection<Group> Groups { get; set; }

        public string GroupName { get; set; }

        [JsonIgnore]
        public DateTime Uptime { get; set; }

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
                _ServicePid = value;
                OnPropertyChanged(new PropertyChangedEventArgs("ServicePid"));
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
                _VideoOutputId = value;
                OnPropertyChanged(new PropertyChangedEventArgs("VideoOutputId"));
            }
        }

        public string ModelName
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
                    else
                    {
                        HeaderType = value[0];
                    }
                }
                _Type = value;
                OnPropertyChanged(new PropertyChangedEventArgs("HeaderType"));
            }
        }

        [JsonIgnore]
        public char HeaderType { get; set; }

        //public ObservableCollection<Group> Groups { get; set; }

        //public List<Group> Groups { get; set; }
        [JsonIgnore]
        public int ErrorCount { get; set; }

        [JsonIgnore]
        public string Color
        {
            get { return _Color; }
            set
            {
                _Color = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Color"));
            }
        }

        [JsonIgnore]
        public string Message { get; set; }

        [JsonIgnore]
        public string Status
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
                            this.Color = "Green";
                            this.Message = "Normal status";
                        }
                        else if (value.ToLower().Equals("critical"))
                        {
                            this.Color = "Red";
                        }
                        else if (value.ToLower().Equals("warning"))
                        {
                            this.Color = "#FF8000";
                        }
                        else if (value.ToLower().Equals("information"))
                        {
                            this.Color = "Blue";
                        }
                        else
                        {
                            this.Color = "White";
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
        public int ConnectionErrorCount { get; set; }

        public Server()
        {
            _Status = "";
            IsConnect = (int)EnumIsConnect.Init;
            ConnectionErrorCount = 0;
        }

        public int GetNewLocation()
        {
            Location = NmsInfo.GetInstance().serverList.Max(x => x.Location) + 1;
            return Location;
        }

        public void Clear()
        {
            this.Ip = null;
            this.Ip = null;
            this.Location = 0;
            this.Color = null;
            this.Gid = null;
            this.GroupName = null;
            this.Groups = null;
            this.VideoOutputId = 0;
            this.ServicePid = 0;
            this.ModelName = null;
            this.Message = null;
            this.IsConnect = EnumIsConnect.Init;
            this.HeaderType = '\0';
            this.Status = null;
            this.UnitName = null;
            this.Version = null;
            this.ErrorCount = 0;
            this.ConnectionErrorCount = 0;
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
                    UpdateServerStatus(this);
                }
            }
        }

        public static int GetServerLastStatus()
        {
            int ret = 0;
            string query = "UPDATE server set status = @status";
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.getInstance().ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@status", "idle");
                cmd.Prepare();
                ret = cmd.ExecuteNonQuery();
            }
            return ret;
        }

        // deprecated
        /*
        public void SetServerInfo(string name, string ip, string gid)
        {
            UnitName = name;
            Ip = ip;
            Gid = gid;
        }
        */

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

        public static IEnumerable<Server> GetServerSettings()
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
                UnitName = row.Field<string>("name"),
                Ip = row.Field<string>("ip"),
                GroupName = row.Field<string>("grp_name"),
            }).ToList();
        }

        public string AddServer()
        {
            string id = null;

            if (String.IsNullOrEmpty(UnitName))
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

            query = "INSERT INTO server (id, ip, name, gid) VALUES (@id, @ip, @name, @gid)";
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.getInstance().ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@name", this.UnitName);
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
                cmd.Parameters.AddWithValue("@name", this.UnitName);
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
            string query = "UPDATE server set status = @status, type = @type, name = @name, location = @location, error_count = @error_count, connection_error_count = @connection_error_count WHERE id = @id";
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.getInstance().ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", server.Id);
                cmd.Parameters.AddWithValue("@status", server.Status);
                cmd.Parameters.AddWithValue("@name", server.UnitName);
                cmd.Parameters.AddWithValue("@location", server.Location);
                cmd.Parameters.AddWithValue("@type", server.ModelName);
                cmd.Parameters.AddWithValue("@error_count", server.ErrorCount);
                cmd.Parameters.AddWithValue("@connection_error_count", server.ConnectionErrorCount);
                cmd.Prepare();
                ret = cmd.ExecuteNonQuery();
            }
            return ret;
        }

        public static int UpdateServerInformation(Server server)
        {
            int ret = 0;
            string query = "UPDATE server set version = @version WHERE id = @id";
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.getInstance().ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", server.Id);
                cmd.Parameters.AddWithValue("@version", server.Version);
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

        public static bool ImportServer(ObservableCollection<Server> servers)
        {
            /* 1.transation
             * 2. Delete server table
             * 3. insert new server info
             * 4. commit
             */

            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.getInstance().ConnectionString))
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
            DataTable dt = new DataTable();
            string query = @"SELECT S.*
, IF(S.status = 'Critical', 'Red', IF(S.status = '#FF8000', 'Yellow', IF(S.status = 'Information', 'Blue', 'Green'))) AS color
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
                Id = row.Field<string>("id"),
                Gid = row.Field<string>("gid"),
                UnitName = row.Field<string>("name"),
                Ip = row.Field<string>("ip"),
                GroupName = row.Field<string>("grp_name"),
                ModelName = row.Field<string>("type"),
                Location = row.Field<int>("location"),
                ErrorCount = row.Field<int>("error_count"),
                ConnectionErrorCount = row.Field<int>("connection_error_count"),
                Color = row.Field<string>("color"),
                Status = row.Field<string>("status")
            }).ToList();
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