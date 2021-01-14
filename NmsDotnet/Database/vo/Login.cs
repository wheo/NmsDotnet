using NmsDotnet.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace NmsDotnet.Database.vo
{
    public class Login
    {
        private Login()
        {
        }

        public string id { get; set; }
        public string pw { get; set; }

        public static Login login;

        public static Login GetInstance()
        {
            if (login == null)
            {
                login = new Login();
            }
            return login;
        }

        public bool LoginCheck(string LoginID, string LoginPW)
        {
            String query = "SELECT * FROM user WHERE id = @id AND pw = @pw";
            try
            {
                using (MySqlConnection conn = new MySqlConnection(DatabaseManager.getInstance().ConnectionString))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", LoginID);
                    cmd.Parameters.AddWithValue("@pw", LoginPW);
                    cmd.Prepare();
                    MySqlDataReader rdr = cmd.ExecuteReader();

                    if (rdr.Read())
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (MySqlException ex)
            {
                throw new ArgumentException("Database Connection Error");
            }
        }
    }
}