using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
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
using log4net;
using MySql.Data.MySqlClient;
using NmsDotnet.config;
using NmsDotnet.Database.vo;
using NmsDotnet.lib;
using NmsDotnet.Database;

namespace NmsDotnet
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LoginWindow : Window
    {
        private readonly ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        JsonConfig jsonConfig;
        public LoginWindow()
        {
            InitializeComponent();
            LoadConfig();
        }

        private bool LoadConfig()
        {
            // config.json 읽기
            String jsonString;
            try
            {
                jsonConfig = JsonConfig.getInstance();
                if (!File.Exists(jsonConfig.configFileName))
                {
                    MessageBox.Show("config.json 파일이 없습니다.\n환경설정 파일을 읽지 못했습니다.\n기본값으로 설정합니다.", "경고", MessageBoxButton.OK);
                    //default value
                    jsonConfig.ip = "192.168.2.66";
                    jsonConfig.port = 33306;
                    jsonConfig.id = "tnmnms";
                    jsonConfig.pw = "tnmtech";
                    jsonConfig.DatabaseName = "TNM_NMS";

                    jsonString = JsonSerializer.Serialize(jsonConfig);
                    File.WriteAllText(jsonConfig.configFileName, jsonString);
                }
                
                jsonString = File.ReadAllText(jsonConfig.configFileName);
                jsonConfig = JsonSerializer.Deserialize<JsonConfig>(jsonString);
                
                DatabaseManager.getInstance().SetConnectionString(jsonConfig.ip, jsonConfig.port, jsonConfig.id, jsonConfig.pw, jsonConfig.DatabaseName);
            }
            catch (FileLoadException e)
            {
                MessageBox.Show(e.ToString() + "\n\n프로그램을 종료합니다.", "경고", MessageBoxButton.OK);
            }

            return true;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            IconHelper.RemoveIcon(this);
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {            
            if ( Login.GetInstance().LoginCheck(LoginID.Text, LoginPW.Password))
            {
                NmsMainWindow nmsMainWindow = new NmsMainWindow();
                nmsMainWindow.Show();
                this.Close();
            } else
            {
                MessageBox.Show("아이디와 비밀번호를 확인해주세요");
                logger.Info(String.Format("login failed, ({0})", LoginID.Text));
            }
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
