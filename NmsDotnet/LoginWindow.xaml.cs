using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MySql.Data.MySqlClient;
using NmsDotnet.lib;

namespace NmsDotnet
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            IconHelper.RemoveIcon(this);
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            String ip = "192.168.2.66";
            int port = 33306;
            String id = "tnmnms";
            String pw = "tnmtech";
            String DatabaseName = "TNM_NMS";
            String strConn = String.Format("server={0};port={1};uid={2};pwd={3};database={4};charset=utf8mb4;",
                ip,
                port,
                id,
                pw,
                DatabaseName);
            MySqlConnection conn = new MySqlConnection(strConn);
            conn.Open();
            
            //나중에 preparestatement로 변경해야함(보안상의 이유)
            String query = String.Format("SELECT * FROM user WHERE id = '{0}' AND pw = '{1}'", LoginID.Text, LoginPW.Password);
            MySqlCommand cmd = new MySqlCommand(query, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();

            /*
            while(rdr.Read())
            {
                MessageBox.Show(String.Format("{0}-{1}", rdr["id"], rdr["name"]));
            }
            */            
            if ( rdr.Read() )
            {
                NmsMainWindow nmsMainWindow = new NmsMainWindow();
                nmsMainWindow.Show();
                this.Close();
            } else
            {
                MessageBox.Show("아이디와 비밀번호를 확인해주세요");
            }

            conn.Close();            
        }

        private void TransactionExample()
        {
            /*
            MySqlCommand command = conn.CreateCommand();
            MySqlTransaction myTrans = conn.BeginTransaction(); // 트랜잭션 시작

            command.Connection = conn;
            command.Transaction = myTrans;

            command.CommandText = (첫 쿼리문, 유저 이름으로 된 테이블 생성);
            command.ExecuteNonQuery();

            command.CommandText = (두 번 째 쿼리문, 전체 유저 테이블에 신규 유저 정보 저장);
            command.ExecuteNonQuery();

            myTrans.Commit(); // 트랜잭션 끝
            */
        }

        private void LoginPW_KeyDown(object sender, KeyEventArgs e)
        {
            if ( e.Key == Key.Enter)
            {
                BtnLogin_Click(sender, e);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoginID.Focus();
        }
    }
}
