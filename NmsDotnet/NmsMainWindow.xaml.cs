using log4net;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using NmsDotnet.lib;
using NmsDotNet.Database.vo;
using NmsDotNet.Service;
using NmsDotNet.vo;
using SnmpSharpNet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace NmsDotNet
{
    /// <summary>
    /// NmsMainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class NmsMainWindow : Window
    {
        private static readonly ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static bool _shouldStop = false;
        private ObservableCollection<Server> _serverList;

        private ObservableCollection<LogItem> _dialogLogList;

        private SoundPlayer _simpleSound = null;

        private LogList _logs;

        private LogItem _currentSelectedItem = null;

        private ObservableCollection<LogItem> _logItem;

        private int _logCount;

        //CancellationTokenSource source;
        public NmsMainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadMibFiles();
            //SetServerIdle();
            GetGroupList();

            SnmpSetServiceTest();

            ServerDispatcherTimer();
            _logCount = 0;
            _logs = new LogList();
            GetLog();

            Task.Run(() => TrapListener());
        }

        private void LoadMibFiles()
        {
            String path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MIB");
            String[] files = Directory.GetFiles(path);
        }

        private void SetServerIdle()
        {
            Server.GetServerLastStatus();
        }

        private void GetGroupList()
        {
            TreeGroup.ItemsSource = Group.GetGroupList();
        }

        private int GetLog()
        {
            _logItem = new ObservableCollection<LogItem>(LogItem.GetInstance().GetLog());
            LvLog.ItemsSource = null;
            LvLog.ItemsSource = _logItem;
            return LogItem.GetInstance().LogItemCount;
        }

        private void ServerDispatcherTimer()
        {
            DispatcherTimer SnmpGetTimer = new DispatcherTimer(DispatcherPriority.Render);
            SnmpGetTimer.Interval = TimeSpan.FromSeconds(5);
            SnmpGetTimer.Tick += new EventHandler(SnmpGetService);
            _serverList = new ObservableCollection<Server>(Server.GetServerList());
            ServerListItem.ItemsSource = _serverList;
            SnmpGetTimer.Start();
        }

        private void SnmpSetServiceTest()
        {
            SnmpService.Set();
        }

        private void SnmpGetService(object sender, EventArgs e)
        {
            var Servers = _serverList;
            bool drawItem = false;
            foreach (Server server in Servers)
            {
                string unitName = null;
                string serviceOID = null;
                try
                {
                    if (SnmpService.Get(server.Ip, out unitName))
                    {
                        //logger.Debug(String.Format("[{0}/{1}] ServerService current status", server.Ip, server.Status));

                        if ("CM5000".Equals(unitName))
                        {
                            serviceOID = SnmpService._CM5000UnitName_oid;
                        }
                        else
                        {
                            serviceOID = SnmpService._DR5000UnitName_oid;
                        }
                        if (!string.IsNullOrEmpty(unitName) && server.Status != "normal")
                        {
                            server.Type = unitName;

                            if (server.ErrorCount == 0)
                            {
                                //server.Status = "normal";
                            }
                            if (!server.IsConnect)
                            {
                                Snmp snmp = new Snmp { IP = server.Ip, Port = "65535", Community = "public", TypeOid = serviceOID, LevelString = "Critical", TypeValue = "end", TranslateValue = "Failed to connection" };
                                LogItem.GetInstance().LoggingDatabase(snmp);
                                //server.IsChange = true; INotify 인터페이스로 대체
                                //logger.Info(String.Format($"[{server.Ip}] ServerService ({server.Status}) status changed")); // INotify 로 이동
                                drawItem = true;
                            }
                        }
                        server.IsConnect = true;
                    }
                    else
                    {
                        if (server.IsConnect)
                        {
                            //server.Type = unitName;
                            //server.Status = "critical";
                            //server.IsChange = true;
                            //logger.Info(String.Format($"[{server.Ip}] ServerService ({server.Status}) status changed")); // INotify 로 이동
                            Debug.WriteLine(server.Type);

                            if (server.Type.Equals(unitName))
                            {
                                serviceOID = SnmpService._CM5000UnitName_oid;
                            }
                            else
                            {
                                serviceOID = SnmpService._DR5000UnitName_oid;
                            }

                            Snmp snmp = new Snmp { IP = server.Ip, Port = "65535", Community = "public", TypeOid = serviceOID, LevelString = "Critical", TypeValue = "begin", TranslateValue = "Failed to connection" };
                            LogItem.GetInstance().LoggingDatabase(snmp);

                            drawItem = true;
                        }
                        server.IsConnect = false;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex.ToString());
                }
            }

            if (drawItem)
            {
                if (ServerListItem.Dispatcher.CheckAccess())
                {
                    ServerListItem.ItemsSource = null;
                    ServerListItem.ItemsSource = Server.GetServerList();
                    //ServerListItem.ItemsSource = _serverList;
                }
                else
                {
                    ServerListItem.Dispatcher.Invoke(() =>
                    {
                        ServerListItem.ItemsSource = null;
                        ServerListItem.ItemsSource = Server.GetServerList();
                        //ServerListItem.ItemsSource = _serverList;
                    });
                }

                if (LvLog.Dispatcher.CheckAccess())
                {
                    _logCount = GetLog();
                }
                else
                {
                    LvLog.Dispatcher.Invoke(() => { _logCount = GetLog(); });
                }
            }
        }

        private void ServerList_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            //MessageBox.Show("Get");
            //Service.SnmpService.GetNext();
            logger.Info(sender);
            MenuItem menuItem = (MenuItem)e.Source;
            ContextMenu menu = (ContextMenu)menuItem.Parent;
            ListView lv = (ListView)menu.PlacementTarget;
            lv.Focus();
        }

        private void ServerList_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            logger.Info(sender);

            MessageBox.Show("left down");
            /*
            Service.Snmp.GetBulk();
            */
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            logger.Info("NMS main windows is closing");
            _shouldStop = true;
        }

        private async void MenuGroupAdd_Click(object sender, RoutedEventArgs e)
        {
            //var result = await DialogHost.Show("DialogGroup");
            this.IsEnabled = false;

            Group group = new Group();
            var result = await DialogHost.Show(group, "DialogGroup");
        }

        private async void MenuGroupEdit_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)e.Source;
            ContextMenu menu = (ContextMenu)menuItem.Parent;
            TreeView tv = (TreeView)menu.PlacementTarget;
            TreeViewItem item = (TreeViewItem)(tv.ItemContainerGenerator.ContainerFromItem(tv.SelectedItem));

            if (item == null)
            {
                MessageBox.Show("그룹을 선택해 주세요", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                this.IsEnabled = false;
                var group = (Group)item.Header;
                var result = await DialogHost.Show(group, "DialogGroup");
            }
        }

        private void ClosingGroupDialogEventHandler(object sender, MaterialDesignThemes.Wpf.DialogClosingEventArgs eventArgs)
        {
            if (this.IsEnabled == false)
            {
                this.IsEnabled = true;
            }
            if ((bool)eventArgs.Parameter == true)
            {
                Group group = (Group)eventArgs.Session.Content;
                Debug.WriteLine(group.Id);
                if (string.IsNullOrEmpty(group.Id))
                {
                    Group.AddGroup(group);
                    TreeGroup.ItemsSource = Group.GetGroupList();
                }
                else
                {
                    Group.EditGroup(group);
                }
            }
        }

        private void MenuGroupDel_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)e.Source;
            ContextMenu menu = (ContextMenu)menuItem.Parent;
            TreeView tv = (TreeView)menu.PlacementTarget;
            if ((Group)tv.SelectedItem == null)
            {
                MessageBox.Show("그룹을 선택해 주세요", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                var group = (Group)tv.SelectedItem;
                if (Group.DeleteGroup(group.Id) > 0)
                {
                    TreeGroup.ItemsSource = Group.GetGroupList();
                    ServerListItem.ItemsSource = Server.GetServerList();
                    //서비스 스레드 종료 꼭 해야함 (서비스 스레드는 1개로 운영)
                }
            }
        }

        private async void MenuServerAdd_Click(object sender, RoutedEventArgs e)
        {
            //clear 먼저
            //DialogServer.IsOpen = true;

            this.IsEnabled = false;

            Server server = new Server();
            server.Groups = (List<Group>)Group.GetGroupList();
            var result = await DialogHost.Show(server, "DialogServer");
        }

        private async void MenuServerEdit_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)e.Source;
            ContextMenu menu = (ContextMenu)menuItem.Parent;
            ListView lv = (ListView)menu.PlacementTarget;
            Server server = (Server)lv.SelectedItem;
            if (server == null)
            {
                MessageBox.Show("서버를 선택해 주세요", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            logger.Debug(server.GroupName);
            logger.Debug(server.Gid);
            /*
            TbServerIp.Text = info.Ip;
            TbServerName.Text = info.Name;
            CbGroup.Text = info.GroupName;
            */
            //DialogServer.IsOpen = true;

            this.IsEnabled = false;
            var result = await DialogHost.Show(server, "DialogServer");
        }

        private void ClosingServerDialog(object sender, MaterialDesignThemes.Wpf.DialogClosingEventArgs eventArgs)
        {
            if (this.IsEnabled == false)
            {
                this.IsEnabled = true;
            }
            if ((bool)eventArgs.Parameter == true)
            {
                Server server = (Server)eventArgs.Session.Content;
                Debug.WriteLine(server.Id);
                if (string.IsNullOrEmpty(server.Id))
                {
                    if (!string.IsNullOrEmpty(server.Gid))
                    {
                        server.AddServer();
                        ServerListItem.ItemsSource = Server.GetServerList();
                        TreeGroup.ItemsSource = Group.GetGroupList();
                    }
                    else
                    {
                        MessageBox.Show("그룹을 설정해야 합니다");
                    }
                }
                else
                {
                    server.EditServer();
                    //바인딩이 지저분해짐 한번에 할 수 있는것을 연구해야함
                    TreeGroup.ItemsSource = Group.GetGroupList();
                    ServerListItem.ItemsSource = Server.GetServerList();
                }
            }
        }

        private void MenuServerDel_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)e.Source;
            ContextMenu menu = (ContextMenu)menuItem.Parent;
            ListView lv = (ListView)menu.PlacementTarget;
            if ((Server)lv.SelectedItem == null)
            {
                MessageBox.Show("그룹을 선택해 주세요", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                var server = (Server)lv.SelectedItem;
                logger.Debug(server.Id);
                if (Server.DeleteServer(server) > 0)
                {
                    logger.Debug(String.Format("[{0}/{1} deleted]", server.Ip, server.Status));

                    TreeGroup.ItemsSource = Group.GetGroupList();
                    ServerListItem.ItemsSource = Server.GetServerList();
                }
            }
        }

        private void BtnServerAdd_Click(object sender, RoutedEventArgs e)
        {
            /*
            // 검증
            if (CbGroup.SelectedItem == null)
            {
                MessageBox.Show("그룹을 선택해 주세요");
            }
            else if (Server.ValidServerIP(TbServerIp.Text))
            {
                MessageBox.Show("중복된 IP는 등록할 수 없습니다");
            }
            else
            {
                var group = (Group)CbGroup.SelectedItem;
                Server server = new Server();
                server.SetServerInfo(TbServerName.Text, TbServerIp.Text, group.Id);
                server.Id = server.AddServer();
                server.Status = "idle";
                ServerListItem.ItemsSource = Server.GetServerList();
                TreeGroup.ItemsSource = Group.GetInstance().GetGroupList();

                //Task.Run(() => ServerService(server));
                logger.Info(String.Format("{0} New Service is created", server.Ip, server.Status, server.Name));
            }
            */
        }

        /*
        private void DialogServer_DialogOpened(object sender, MaterialDesignThemes.Wpf.DialogOpenedEventArgs eventArgs)
        {
            //CbGroup.ItemsSource = Group.GetInstance().GetGroupList();
        }
        */

        private void TrapListener()
        {
            logger.Info("TrapListener is created");

            // Construct a socket and bind it to the trap manager port 162
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.ReceiveTimeout = 100;
            socket.SendTimeout = 100;
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 162);
            EndPoint ep = (EndPoint)ipep;
            socket.Bind(ep);
            // Disable timeout processing. Just block until packet is received
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 100);

            int inlen = -1;
            Debug.WriteLine(string.Format($"Waiting for snmp trap"));
            while (!_shouldStop)
            {
                byte[] indata = new byte[16 * 1024];
                // 16KB receive buffer int inlen = 0;
                IPEndPoint peer = new IPEndPoint(IPAddress.Any, 0);
                EndPoint inep = (EndPoint)peer;
                try
                {
                    inlen = socket.ReceiveFrom(indata, ref inep);
                }
                catch (Exception ex)
                {
                    //Debug.WriteLine("Exception {0}", ex.Message);
                    inlen = -1;
                }
                if (inlen > 0)
                {
                    // Check protocol version int
                    int ver = SnmpPacket.GetProtocolVersion(indata, inlen);
                    if (ver == (int)SnmpVersion.Ver1)
                    {
                        // Parse SNMP Version 1 TRAP packet
                        SnmpV1TrapPacket pkt = new SnmpV1TrapPacket();
                        pkt.decode(indata, inlen);
                        logger.Info(string.Format("** SNMP Version 1 TRAP received from {0}:", inep.ToString()));
                        logger.Info(string.Format("*** Trap generic: {0}", pkt.Pdu.Generic));
                        logger.Info(string.Format("*** Trap specific: {0}", pkt.Pdu.Specific));
                        logger.Info(string.Format("*** Agent address: {0}", pkt.Pdu.AgentAddress.ToString()));
                        logger.Info(string.Format("*** Timestamp: {0}", pkt.Pdu.TimeStamp.ToString()));
                        logger.Info(string.Format("*** VarBind count: {0}", pkt.Pdu.VbList.Count));
                        logger.Info("*** VarBind content:");
                        foreach (Vb v in pkt.Pdu.VbList)
                        {
                            Debug.WriteLine("**** {0} {1}: {2}", v.Oid.ToString(), SnmpConstants.GetTypeName(v.Value.Type), v.Value.ToString());
                        }
                        Debug.WriteLine("** End of SNMP Version 1 TRAP data.");
                    }
                    else
                    {
                        // Parse SNMP Version 2 TRAP packet
                        SnmpV2Packet pkt = new SnmpV2Packet();
                        pkt.decode(indata, inlen);
                        logger.Info(string.Format("** SNMP Version 2 TRAP received from {0}:", inep.ToString()));
                        if ((SnmpSharpNet.PduType)pkt.Pdu.Type != PduType.V2Trap)
                        {
                            Debug.WriteLine("*** NOT an SNMPv2 trap ****");
                        }
                        else
                        {
                            Debug.WriteLine("*** Community: {0}", pkt.Community.ToString());
                            Debug.WriteLine("*** VarBind count: {0}", pkt.Pdu.VbList.Count);
                            Debug.WriteLine("*** VarBind content:");

                            Snmp snmp = new Snmp();

                            foreach (Vb v in pkt.Pdu.VbList)
                            {
                                snmp.Id = v.Oid.ToString();
                                snmp.IP = inep.ToString().Split(':')[0];
                                snmp.Port = inep.ToString().Split(':')[1];
                                snmp.Syntax = SnmpConstants.GetTypeName(v.Value.Type);
                                snmp.Community = pkt.Community.ToString();
                                snmp.Value = v.Value.ToString();
                                snmp.type = "trap";

                                logger.Info("Oid : " + v.Oid.ToString());

                                string value = Snmp.GetNameFromOid(v.Oid.ToString());
                                logger.Info("value : " + value);

                                if (value.LastIndexOf("Level") > 0)
                                {
                                    snmp.LevelString = Snmp.GetLevelString(Convert.ToInt32(v.Value.ToString()));
                                    logger.Info("LevelString : " + snmp.LevelString);
                                }
                                else if (value.LastIndexOf("Type") > 0)
                                {
                                    snmp.TranslateValue = Snmp.GetTranslateValue(value);
                                    snmp.TypeValue = Enum.GetName(typeof(Snmp.TrapType), Convert.ToInt32(v.Value.ToString()));
                                    snmp.TypeOid = v.Oid.ToString();
                                }
                                else if (value.LastIndexOf("Channel") > 0)
                                {
                                    snmp.Channel = Convert.ToInt32(v.Value.ToString()) + 1; //0받으면 채널 1로
                                }
                                else if (value.LastIndexOf("Main") > 0)
                                {
                                    snmp.Main = Enum.GetName(typeof(Snmp.EnumMain), Convert.ToInt32(v.Value.ToString()));
                                }

                                //logger.Info("TranslateValue : " + snmp.TranslateValue);

                                //데이터베이스 테이블을 만들기 위해 등록함(로그는 translate 테이블을 이용하자)
                                Snmp.RegisterSnmpInfo(snmp);

                                logger.Info(String.Format("[{0}] Trap : {1} {2}: {3}", inep.ToString().Split(':')[0], v.Oid.ToString(), SnmpConstants.GetTypeName(v.Value.Type), v.Value.ToString()));
                            }

                            if (Snmp.IsEnableTrap(snmp.TypeOid))
                            {
                                if (!String.IsNullOrEmpty(snmp.LevelString))
                                {
                                    snmp.TrapString = snmp.MakeTrapLogString();
                                    LogItem.GetInstance().LoggingDatabase(snmp);
                                    Server s = null;
                                    foreach (var server in _serverList)
                                    {
                                        if (server.Ip == snmp.IP)
                                        {
                                            s = server;
                                            break;
                                        }
                                    }

                                    if (string.Equals(snmp.TypeValue, "begin"))
                                    {
                                        s.ErrorCount++;
                                    }
                                    else if (string.Equals(snmp.TypeValue, "end"))
                                    {
                                        if (s.ErrorCount > 0)
                                        {
                                            s.ErrorCount--;
                                        }
                                    }

                                    if (!string.Equals(snmp.TypeValue, "log"))
                                    {
                                        if (s.ErrorCount > 0)
                                        {
                                            s.Status = Server.CompareState(s.Status, snmp.LevelString.ToLower());
                                        }
                                        else
                                        {
                                            s.Status = "normal";
                                        }

                                        if (ServerListItem.Dispatcher.CheckAccess())
                                        {
                                            ServerListItem.ItemsSource = null;
                                            ServerListItem.ItemsSource = Server.GetServerList();
                                            //TreeGroup.ItemsSource = null;
                                            TreeGroup.ItemsSource = Group.GetGroupList();
                                            //ServerListItem.ItemsSource = _serverList; //최적화시 _serverList만 관리하고 데이터베이스 Select 는 지양해야함
                                        }
                                        else
                                        {
                                            ServerListItem.Dispatcher.Invoke(() =>
                                            {
                                                ServerListItem.ItemsSource = null;
                                                ServerListItem.ItemsSource = Server.GetServerList();
                                                //TreeGroup.ItemsSource = null;
                                                TreeGroup.ItemsSource = Group.GetGroupList();
                                                //ServerListItem.ItemsSource = _serverList; //최적화시 _serverList만 관리하고 데이터베이스 Select 는 지양해야함
                                            });
                                        }
                                    }

                                    if (LvLog.Dispatcher.CheckAccess())
                                    {
                                        _logCount = GetLog();
                                    }
                                    else
                                    {
                                        LvLog.Dispatcher.Invoke(() => { _logCount = GetLog(); });
                                    }

                                    /*
                                    if (DialogNotification.Dispatcher.CheckAccess())
                                    {
                                        DialogNotification.IsOpen = true;
                                    }
                                    else
                                    {
                                        DialogNotification.Dispatcher.Invoke(() => { DialogNotification.IsOpen = true; });
                                    }*/
                                }
                            }
                            if (_logCount > 0)
                            {
                                Task.Run(() => SoundPlayAsync());
                            }
                            else
                            {
                                SoundStop();
                            }

                            logger.Info("** End of SNMP Version 2 TRAP data.");
                        }
                    }
                }
                else
                {
                    if (inlen == 0)
                    {
                        Debug.WriteLine("Zero length packet received.");
                    }
                }
            }
            logger.Info("TrapListener is done");
        }

        private async Task SoundPlayAsync()
        {
            if (_simpleSound == null)
            {
                _simpleSound = new SoundPlayer(@"Sound\alarm.wav");
                _simpleSound.PlayLooping();
            }
        }

        private void SoundStop()
        {
            if (_simpleSound != null)
            {
                _simpleSound.Stop();
                _simpleSound = null;
            }
        }

        private void ServerList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string uri = "http://";
            ListView lv = sender as ListView;
            Server server = (Server)lv.SelectedItem;
            System.Diagnostics.Process.Start(String.Format($"{uri}{server.Ip}"));
        }

        private void add_testserver_Click(object sender, RoutedEventArgs e)
        {
            string group_id = "3140e0fd-b752-11ea-a91a-0242ac160002"; //고정
            Server server = new Server();
            server.SetServerInfo("테스트서버", "192.168.2.189", group_id);
            server.AddServer();
            ServerListItem.ItemsSource = Server.GetServerList();
            TreeGroup.ItemsSource = Group.GetGroupList();
            //Task.Run(() => ServerService(server)); // 스레드 돌리지 않음
            logger.Info(String.Format("{0} New Service is created", server.Ip));
        }

        //deprecated
        private void lvBtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            LogItem logItem = (LogItem)GetLvItem(e);
            LogItem.ChangeConfirmStatus(logItem.idx);
            logger.Info(String.Format($"{logItem.Ip}, {logItem.Value}, {logItem.idx}"));
            GetLog();
        }

        private object GetLvItem(RoutedEventArgs e)
        {
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            while (!(dep is System.Windows.Controls.ListViewItem))
            {
                try
                {
                    dep = VisualTreeHelper.GetParent(dep);
                }
                catch
                {
                    return null;
                }
            }
            ListViewItem item = (ListViewItem)dep;
            //item.Background = Brushes.Transparent;
            return item.Content;
        }

        private void TreeGroup_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is Group)
            {
                var group = (Group)e.NewValue;
                Debug.WriteLine(group.Name);
            }
        }

        private void TreeGroup_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);

            if (treeViewItem != null)
            {
                treeViewItem.Focus();
                e.Handled = true;
            }
        }

        private TreeViewItem VisualUpwardSearch(DependencyObject source)
        {
            while (source != null && !(source is TreeViewItem))
            {
                source = VisualTreeHelper.GetParent(source);
            }

            return source as TreeViewItem;
        }

        private async void BtnMenuSetting_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;

            GlobalSettings settings = new GlobalSettings();
            settings.SnmpCM5000Settings = (List<SnmpSetting>)Snmp.GetTrapAlarmList("CM5000");
            settings.SnmpDR5000Settings = (List<SnmpSetting>)Snmp.GetTrapAlarmList("DR5000");
            settings.ServerSettings = (List<Server>)Server.GetServerList();
            var result = await DialogHost.Show(settings, "DialogSettingInfo");
        }

        private void ClosingSettingDialogEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            if (this.IsEnabled == false)
            {
                this.IsEnabled = true;
            }
            if ((bool)eventArgs.Parameter == true)
            {
                GlobalSettings settings = (GlobalSettings)eventArgs.Session.Content;
                Snmp.UpdateSnmpMessgeUseage(settings);

                TreeGroup.ItemsSource = Group.GetGroupList();
                ServerListItem.ItemsSource = Server.GetServerList();
            }
        }

        private void HiddenAlarm_Click(object sender, RoutedEventArgs e)
        {
#if false
            LogItem.LogHide();
            LvLog.ItemsSource = LogItem.GetInstance().GetLog();
#endif
            // 알람 끄기로 변경
            SoundStop();
        }

        private async void LogView_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;

            _logs.Logs = new ObservableCollection<LogItem>(LogItem.GetInstance().GetLog("dialog"));
            var result = await DialogHost.Show(_logs, "DialogLogViewInfo");
        }

        private void BtnDialogLogSumit_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender as Button;
            DatePicker DpDayFrom = (DatePicker)FindElemetByName(e, "DpDayFrom");
            DatePicker DpDayTo = (DatePicker)FindElemetByName(e, "DpDayTo");

            if (String.IsNullOrEmpty(DpDayFrom.Text))
            {
                DpDayFrom.Text = DateTime.Now.ToString("yyyy-MM-dd");
            }
            if (String.IsNullOrEmpty(DpDayTo.Text))
            {
                DpDayTo.Text = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
            }
            if (DpDayFrom.Text == DpDayTo.Text)
            {
                DpDayTo.Text = Convert.ToDateTime(DpDayTo.Text).AddDays(1).ToString("yyyy-MM-dd");
            }

            _logs.Logs = new ObservableCollection<LogItem>(LogItem.GetInstance().GetLog("dialog", DpDayFrom.Text, DpDayTo.Text));
            ListView lvDialog = (ListView)FindElemetByName(e, "DialogLvLog");
            lvDialog.ItemsSource = _logs.Logs;
        }

        private void ClosingLogViewDialogEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            if (this.IsEnabled == false)
            {
                this.IsEnabled = true;
            }
            /*
            if ((bool)eventArgs.Parameter == true)
            {
                SnmpSettings settings = (SnmpSettings)eventArgs.Session.Content;
                Snmp.UpdateSnmpMessgeUseage(settings);
            }
            */
        }

        private void ServerEdit_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBoxResult.Yes == MessageBox.Show(String.Format("정보를 수정 하시겠습니까?"), "알림", MessageBoxButton.YesNo, MessageBoxImage.Information))
            {
                Server s = (Server)GetLvItem(e);
                s.EditServer();
            }
        }

        private void BtnExcelDownload_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Csv file (*.csv)|*.csv";
            string downloadsPath = KnownFolders.GetPath(KnownFolder.Downloads);
            saveFileDialog.InitialDirectory = downloadsPath;
            string csvString = "";

            saveFileDialog.FileName = DateTime.Now.ToString("yyyy-MM-dd");
            csvString = LogItem.MakeCsvFile(LogItem.GetInstance().GetLog("dialog"));

            try
            {
                if (String.IsNullOrEmpty(csvString))
                {
                    MessageBox.Show("저장할 항목이 없습니다", "정보", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else if (saveFileDialog.ShowDialog() == true)
                {
                    File.WriteAllText(saveFileDialog.FileName, csvString, System.Text.Encoding.GetEncoding("euc-kr"));
                }
            }
            catch (Exception)
            {
                MessageBox.Show("파일을 저장할 수 없습니다. 열려있는 파일을 닫아주세요", "경고", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void MenuItem_LogHide_Click(object sender, RoutedEventArgs e)
        {
            if (_currentSelectedItem != null)
            {
                LogItem.HideLogAlarm(_currentSelectedItem.idx);
                GetLog();
            }
        }

        private void ListViewItem_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            _currentSelectedItem = (LogItem)GetLvItem(e);
        }

        private DependencyObject FindElemetByName(RoutedEventArgs e, string Name)
        {
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            DependencyObject ret = null;
            while (ret == null)
            {
                dep = VisualTreeHelper.GetParent(dep);

                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dep); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(dep, i);
                    ret = FindChild<DependencyObject>(child, Name);
                    if (ret != null)
                    {
                        break;
                    }
                }
            }
            return ret;
        }

        public static T FindChild<T>(DependencyObject parent, string childName)
        where T : DependencyObject
        {
            // Confirm parent and childName are valid.
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child.
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }
    }
}