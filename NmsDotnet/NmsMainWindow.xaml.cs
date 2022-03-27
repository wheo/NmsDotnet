using log4net;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using NmsDotnet.Database.vo;
using NmsDotnet.lib;

using NmsDotnet.Service;
using NmsDotnet.vo;
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
using WPF.JoshSmith.ServiceProviders.UI;
using System.Text.Json;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NAudio.Wave;
using System.Windows.Media.Imaging;
using System.Drawing;
using Image = System.Windows.Controls.Image;
using NmsDotnet.Utils;
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using System.Windows.Data;
using NmsDotnet.config;

namespace NmsDotnet
{
    /// <summary>
    /// NmsMainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class NmsMainWindow : Window
    {
        private static readonly ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static bool _shouldStop = false;

        private SoundPlayer _soundPlayer = null;

        private LogItem _currentSelectedItem = null;

        private DispatcherTimer _snmpGetTimer = null;

        private System.Threading.Timer _snmpGetThreadingTimer = null;

        private ListViewDragDropManager<Server> dragMgr;

        private const int MAX_SERVER = 1200;

        private int _SnmpPort = 162;

        private List<TitanLiveStatus> _tlss;

        private object tlslock = new object();

        public NmsMainWindow(string userid, string ip, int width, int height)
        {
            InitializeComponent();
            ToolTipGlobalOption();
            logger.Info("NMS Main is Starting");
            //this.Width = width;
            //this.Height = height;
            string[] host = ip.Split(':');

            HostManager.getInstance().uri = string.Format($"http://{ip}");
            HostManager.getInstance().ip = host[0];
            if (host.Count() > 1)
            {
                HostManager.getInstance().port = host[1];
            }
            else
            {
                HostManager.getInstance().port = "80";
            }

            logger.Info(string.Format($"({userid}) logged in"));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //GetInitSetting();
            DragNDropSetting();
            GetAlarmInfo();
            LoadMibFiles();
            ServerDispatcherTimer();

            //SnmpSetServiceTest(); //test 완료
            LogInit();

            _tlss = new List<TitanLiveStatus>();
            /*
            if (_tls.UIDList.Count > 12)
            {
                string uri = string.Format($"http://{snmp.IP}/api/v1/servicesmngt/services/state");
                string jsonbody = JsonConvert.SerializeObject(_tls, Formatting.Indented);
                logger.Info(jsonbody);
                await Http.PostAsync(uri, jsonbody);
                _tls.UIDList.Clear();
                logger.Info("Titan Uid List is cleared");
            }
            */

            //Task.Run(() => TrapListenerAsync());
            //Task.Run(() => TitanManagerAsync());
        }

        private async Task TitanManagerAsync()
        {
            logger.Info("TitanManager is created");
            TimeSpan ts = TimeSpan.FromSeconds(2);
            while (!_shouldStop)
            {
                foreach (TitanLiveStatus tls in _tlss)
                {
                    if (tls.UIDList.Count > 0)
                    {
                        lock (tlslock)
                        {
                            string uri = string.Format($"http://{tls.IP}/api/v1/servicesmngt/services/state");
                            string jsonbody = JsonConvert.SerializeObject(tls, Formatting.Indented);
                            logger.Info(jsonbody);
                            Http.TitanApiPostAsync(uri, jsonbody);
                            tls.Clear();
                            logger.Info("Titan Uid List is cleared");
                        }
                    }
                }

                await Task.Delay(ts);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            logger.Info("NMS Main windows is closing");
            _shouldStop = true;
        }

        public static BitmapSource CreateImage(string text, double width, double heigth)
        {
            // create WPF control
            var size = new System.Windows.Size(width, heigth);

            var stackPanel = new StackPanel();

            var header = new TextBlock();
            header.Text = "Header";
            header.FontWeight = FontWeights.Bold;

            var content = new TextBlock();
            content.TextWrapping = TextWrapping.Wrap;
            content.Text = text;

            stackPanel.Children.Add(header);
            stackPanel.Children.Add(content);

            // process layouting
            stackPanel.Measure(size);
            stackPanel.Arrange(new Rect(size));

            // Render control to an image
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)stackPanel.ActualWidth, (int)stackPanel.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(stackPanel);
            return rtb;
        }

        private void ToolTipGlobalOption()
        {
            // tooltip이 사라지지않게 하는 설정
            //ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(Int32.MaxValue));
        }

        private void GetInitSetting()
        {
            _SnmpPort = Snmp.GetSnmpPort();
        }

        private void GetAlarmInfo()
        {
            NmsInfo.GetInstance().alarmInfo = new ObservableCollection<Alarm>();

            Alarm ac = new Alarm { level = "Critical", Uid = Server.EnumStatus.Critical };
            Alarm aw = new Alarm { level = "Warning", Uid = Server.EnumStatus.Warning };
            Alarm ai = new Alarm { level = "Information", Uid = Server.EnumStatus.Information };

            ObservableCollection<Alarm> remoteData = new ObservableCollection<Alarm>(Alarm.GetAlarmInfo());
            IEnumerable<Alarm> query = remoteData.Where(alarm => alarm.level.Equals("Critical"));

            if (query.Count() > 0)
            {
                foreach (Alarm a in query)
                {
                    a.Uid = Server.EnumStatus.Critical;
                    NmsInfo.GetInstance().alarmInfo.Add(a);
                }
            }
            else
            {
                NmsInfo.GetInstance().alarmInfo.Add(ac);
            }
            query = remoteData.Where(alarm => alarm.level.Equals("Warning"));
            if (query.Count() > 0)
            {
                foreach (Alarm a in query)
                {
                    a.Uid = Server.EnumStatus.Warning;
                    NmsInfo.GetInstance().alarmInfo.Add(a);
                }
            }
            else
            {
                NmsInfo.GetInstance().alarmInfo.Add(aw);
            }
            query = remoteData.Where(alarm => alarm.level.Equals("Information"));
            if (query.Count() > 0)
            {
                foreach (Alarm a in query)
                {
                    a.Uid = Server.EnumStatus.Information;
                    NmsInfo.GetInstance().alarmInfo.Add(a);
                }
            }
            else
            {
                NmsInfo.GetInstance().alarmInfo.Add(ai);
            }
        }

        private void DragNDropSetting()
        {
            this.dragMgr = new ListViewDragDropManager<Server>(ServerListItem);
            this.dragMgr.ListView = ServerListItem;
            this.dragMgr.ShowDragAdorner = true;
            this.dragMgr.DragAdornerOpacity = 0.5;
            this.ServerListItem.DragEnter += OnListViewDragEnter;
            this.ServerListItem.Drop += OnListViewDrop;
        }

        // Handles the DragEnter event for both ListViews.
        private void OnListViewDragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;
        }

        // Handles the Drop event for both ListViews.
        private void OnListViewDrop(object sender, DragEventArgs e)
        {
            if (e.Effects == DragDropEffects.None)
                return;

            Server server = e.Data.GetData(typeof(Server)) as Server;
            if (sender == this.ServerListItem)
            {
                if (this.dragMgr.IsDragInProgress)
                    return;

                // An item was dragged from the bottom ListView into the top ListView
                // so remove that item from the bottom ListView.
                // (this.listView2.ItemsSource as ObservableCollection<Task>).Remove(task);
            }
        }

        private void LoadMibFiles()
        {
            String path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MIB");
            String[] files = Directory.GetFiles(path);
        }

        private void LogInit()
        {
            // 최초 실행시에 최근로그를 한번만 가져옴
            List<LogItem> currentLog = LogItem.GetLog();
            NmsInfo.GetInstance().activeLog = new ObservableCollection<LogItem>(currentLog);

            LvActiveLog.ItemsSource = NmsInfo.GetInstance().activeLog;

            NmsInfo.GetInstance().historyLog = new NmsInfo.LimitedSizeObservableCollection<LogItem>(LogItem.GetLog(true), 1000);

            LvHistory.ItemsSource = NmsInfo.GetInstance().historyLog;
        }

        private void LodingisDone()
        {
            if (PbMainLoading.Visibility == Visibility.Visible)
            {
                PbMainLoading.Visibility = Visibility.Collapsed;
                ServerListItem.Visibility = Visibility.Visible;

                BtnGroupAdd.IsEnabled = true;
                BtnServerAdd.IsEnabled = true;
                BtnServerInfo.IsEnabled = true;
                BtnScreenLock.IsEnabled = true;
                BtnRevert.IsEnabled = true;
                BtnSoundOff.IsEnabled = true;
            }
        }

        private void ServerDispatcherTimer()
        {
            /*
            _snmpGetTimer = new DispatcherTimer(DispatcherPriority.ContextIdle);
            _snmpGetTimer.Tick += new EventHandler(SnmpGetService);
            _snmpGetTimer.Interval = TimeSpan.FromSeconds(5);
            */
            //_snmpGetThreadingTimer = new System.Threading.Timer(SnmpGetService, null, 0, 5000);
            //SnmpGetTimer.Interval = new TimeSpan(0, 0, 5);

            //Task.Factory.StartNew(new Action<object>(SnmpGetService), 5);

            ObservableCollection<Server> ocs = new ObservableCollection<Server>(Server.GetServerList());

            NmsInfo.GetInstance().serverList = ocs;

            NmsInfo.GetInstance().serverList = new ObservableCollection<Server>();
            NmsInfo.GetInstance().serverListStack = new Stack<ObservableCollection<Server>>(10);

            int currentIdx = 0;
            if (ocs.Count > 0)
            {
                for (int i = 0; i < MAX_SERVER; i++)
                {
                    Server s;
                    int currentLocation = ocs[currentIdx].Location;
                    if (i == currentLocation)
                    {
                        s = ocs[currentIdx];

                        MenuItem miEdit = new MenuItem();
                        miEdit.Header = "장비 수정";
                        MenuItem miDelete = new MenuItem();
                        miDelete.Header = "장비 삭제";
                        List<MenuItem> menus = new List<MenuItem>();
                        menus.Add(miEdit);
                        menus.Add(miDelete);
                        miEdit.Click += new System.Windows.RoutedEventHandler(this.MenuServerEdit_Click);
                        miDelete.Click += new System.Windows.RoutedEventHandler(this.MenuServerDel_Click);

                        s.MenuItems = menus;

                        if (ocs.Count - 1 > currentIdx)
                        {
                            currentIdx++;
                        }
                    }
                    else
                    {
                        s = new Server();
                    }
                    NmsInfo.GetInstance().serverList.Add(s);
                }
            }

            NmsInfo.GetInstance().groupList = new ObservableCollection<Group>(Group.GetGroupList());

            foreach (Server s in NmsInfo.GetInstance().serverList)
            {
                s.Groups = NmsInfo.GetInstance().groupList;
            }

            ServerListItem.ItemsSource = NmsInfo.GetInstance().serverList;
            TreeGroup.ItemsSource = NmsInfo.GetInstance().groupList;

            Task.Run(() => GetInformation(5));

            LodingisDone();
        }

        private async Task GetInformation(double sleepSecond)
        {
            TimeSpan ts = TimeSpan.FromSeconds((double)sleepSecond);

            while (!_shouldStop)
            {
                await Task.Delay(ts);
                Board.Getinstance().GetBoard();

                try
                {
                    if (Board.Getinstance().server != null)
                    {
                        foreach (Server s in Board.Getinstance().server)
                        {
                            IEnumerable<Server> results = NmsInfo.GetInstance().serverList;
                            Server c = (Server)results.Where(x => x.Id == s.Id).FirstOrDefault();
                            if (c != null)
                            {
                                c.PutInfo(s);
                            }
                            /*
                            else
                            {
                                ServerListItem.Dispatcher.Invoke(() => { NmsInfo.GetInstance().serverList.Add(s); });
                            }
                            */
                        }
                        /*
                        Server temp_server = null;

                        foreach (Server s in Board.Getinstance().server)
                        {
                            Server c = (Server)NmsInfo.GetInstance().serverList.Where(x => x.Id == s.Id && x.Id != null).FirstOrDefault();
                            if (c == null)
                            {
                                //s는 새로운 장비이다
                                temp_server = s;
                                break;
                            }
                        }

                        if (temp_server != null)
                        {
                            temp_server.GetNewLocation();
                            if (string.IsNullOrEmpty(temp_server.Id))
                            {
                                foreach (Group g in NmsInfo.GetInstance().groupList)
                                {
                                    if (g.Id == temp_server.Gid)
                                    {
                                        temp_server.Grp_name = g.Name;
                                        g.Server.Add(temp_server);
                                        break;
                                    }
                                }

                                // 하나의 함수로 변경해야 함
                                MenuItem miEdit = new MenuItem();
                                miEdit.Header = "장비 수정";
                                MenuItem miDelete = new MenuItem();
                                miDelete.Header = "장비 삭제";
                                List<MenuItem> menus = new List<MenuItem>();
                                menus.Add(miEdit);
                                menus.Add(miDelete);
                                miEdit.Click += new System.Windows.RoutedEventHandler(this.MenuServerEdit_Click);
                                miDelete.Click += new System.Windows.RoutedEventHandler(this.MenuServerDel_Click);
                                temp_server.MenuItems = menus;

                                temp_server.Color = "Green";

                                NmsInfo.GetInstance().serverList.Insert(temp_server.Location, temp_server);

                                //ServerListItem.ItemsSource = Server.GetServerList();
                                //TreeGroup.ItemsSource = Group.GetGroupList();
                            }
                        }
                        */
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e.ToString());
                }

                List<LogItem> currentLog = LogItem.GetLog();

                try
                {
                    List<LogItem> temp = new List<LogItem>();
                    foreach (LogItem item in NmsInfo.GetInstance().activeLog)
                    {
                        IEnumerable<LogItem> result = currentLog;
                        LogItem log = (LogItem)result.Where(x => x.Oid == item.Oid && x.Ip == item.Ip).FirstOrDefault();
                        if (log == null)
                        {
                            temp.Add(item);
                        }
                    }

                    foreach (LogItem item in temp)
                    {
                        LvActiveLog.Dispatcher.Invoke(() => { NmsInfo.GetInstance().activeLog.Remove(item); });
                        logger.Info(string.Format($"({item.Ip}) {item.Value} log removed"));
                    }

                    foreach (LogItem item in currentLog)
                    {
                        IEnumerable<LogItem> result = NmsInfo.GetInstance().activeLog;
                        LogItem log = (LogItem)result.Where(x => x.Value == item.Value && x.Ip == item.Ip).FirstOrDefault();
                        if (log == null)
                        {
                            SoundPlay("Critical");
                            LvActiveLog.Dispatcher.Invoke(() => { NmsInfo.GetInstance().activeLog.Insert(0, item); });
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e.ToString());
                }
                Debug.WriteLine("alive ...");
            }
        }

        private LogItem FindConnectionFailItem(ObservableCollection<LogItem> ocl, string Ip)
        {
            /*
            foreach (LogItem item in ocl)
            {
                if (item.Ip == Ip && item.IsConnection == false)
                {
                    return item;
                }
            }
            return null;
            */
            IEnumerable<LogItem> items =
                from x in ocl
                where x.Ip == Ip && x.IsConnection == false

                select x;
            IEnumerable<LogItem> item = items.OrderByDescending(x => x.LevelPriority).Take(1);
            if (item.Count() > 0)
            {
                return (LogItem)item.ElementAt(0);
            }
            else
            {
                return null;
            }
        }

        private List<LogItem> FindAllActiveAlarm(ObservableCollection<LogItem> ocl, string Ip)
        {
            IEnumerable<LogItem> items =
                from x in ocl
                where x.Ip == Ip
                select x;
            return items.ToList();
        }

        private LogItem FindCurrentStatusItem(ObservableCollection<LogItem> ocl, string Ip)
        {
            IEnumerable<LogItem> items =
                from x in ocl
                where x.Ip == Ip
                select x;
            IEnumerable<LogItem> item = items.OrderByDescending(x => x.LevelPriority).Take(1);
            if (item.Count() > 0)
            {
                return (LogItem)item.ElementAt(0);
            }
            else
            {
                return null;
            }
        }

        private List<LogItem> FindItemFromOid(ObservableCollection<LogItem> ocl, Server s, string oid)
        {
            IEnumerable<LogItem> items =
                from x in ocl
                where x.Oid == oid && x.Ip == s.Ip && string.IsNullOrEmpty(x.EndAt)
                select x;
            return items.ToList();
        }

        private List<LogItem> FindItemFromValue(ObservableCollection<LogItem> ocl, Server s, string value)
        {
            IEnumerable<LogItem> items =
                from x in ocl
                where x.Value == value && x.Ip == s.Ip
                select x;
            return items.ToList();
        }

        private List<LogItem> FindItemDuplicateTrap(ObservableCollection<LogItem> ocl, Server s, string oid)
        {
            IEnumerable<LogItem> items =
                from x in ocl
                where x.Oid == oid && x.Ip == s.Ip && x.TypeValue == "begin"
                select x;
            return items.ToList();
        }

        private List<LogItem> FindItemDuplicateTitanTrap(ObservableCollection<LogItem> ocl, Server s, string value)
        {
            IEnumerable<LogItem> items =
                from x in ocl
                where x.Value == value && x.Ip == s.Ip && x.TypeValue == "begin"
                select x;
            return items.ToList();
        }

        private void ServerList_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            //MessageBox.Show("Get");
            //Service.SnmpService.GetNext();
            //logger.Info(sender);
            MenuItem menuItem = (MenuItem)e.Source;
            ContextMenu menu = (ContextMenu)menuItem.Parent;
            ListView lv = (ListView)menu.PlacementTarget;
            lv.Focus();
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

            var selectedObject = menu.PlacementTarget;
            TreeView tv = GetParentTreeview(selectedObject as DependencyObject);

            //TreeViewItem item = (TreeViewItem)(tv.ItemContainerGenerator.ContainerFromItem(tv.SelectedItem));

            if (tv.SelectedItem == null)
            {
                MessageBox.Show("그룹을 선택해 주세요", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                this.IsEnabled = false;
                var group = (Group)tv.SelectedItem;
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
                    group.AddGroup();
                    group.Server = new ObservableCollection<Server>();
                    NmsInfo.GetInstance().groupList.Add(group);

                    //deprecated 2020-10-28 by wheo
                    //TreeGroup.ItemsSource = Group.GetGroupList();
                    //TreeGroup.ItemsSource = _groupList; // 필요없을것 같다 확인할것 // _grouplist가 observation Collection 이라서 변화 반영됨
                }
                else
                {
                    if (group.EditGroup() < 1)
                    {
                        MessageBox.Show("그룹명 변경 에러가 발생하였습니다");
                    }
                }
            }
        }

        private void MenuGroupDel_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("삭제 하시겠습니까?", "알림", MessageBoxButton.YesNo, MessageBoxImage.Information);
            if (result == MessageBoxResult.Yes)
            {
                MenuItem menuItem = (MenuItem)e.Source;
                Group group = (Group)menuItem.DataContext;

                if (group.DeleteGroup() > 0) // 데이터베이스에 서버는 gid cascade 라서 그룹이 지워지면 서버도 지워짐
                {
                    if (group.Server != null)
                    {
                        foreach (Server s in group.Server)
                        {
                            int location = s.Location;
                            NmsInfo.GetInstance().serverList.Remove(s);
                            Server emptyServer = new Server();
                            NmsInfo.GetInstance().serverList.Insert(location, emptyServer);
                        }
                    }
                    NmsInfo.GetInstance().groupList.Remove(group);
                    //ServerListItem.ItemsSource = Server.GetServerList();
                    //서비스 스레드 종료 꼭 해야함 (서비스 스레드는 1개로 운영)
                }
                else
                {
                    MessageBox.Show("그룹 삭제 실패");
                }
            }
        }

        private async void MenuServerAdd_Click(object sender, RoutedEventArgs e)
        {
            //clear 먼저
            //DialogServer.IsOpen = true;

            if (!ServerListItem.IsEnabled)
            {
                MessageBox.Show("화면 잠금일 때에는 장비를 추가할 수 없습니다", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                this.IsEnabled = false;

                Server server = new Server();
                server.Groups = NmsInfo.GetInstance().groupList;
                var result = await DialogHost.Show(server, "DialogServer");
            }
        }

        private async void MenuServerEdit_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)e.Source;
            Server server = (Server)menuItem.DataContext;

            if (server == null)
            {
                MessageBox.Show("서버를 선택해 주세요", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            server.Undo = server.ShallowCopy();
            //_snmpGetTimer.Stop();

            this.IsEnabled = false;
            var result = await DialogHost.Show(server, "DialogServer");
        }

        private void MenuServerDel_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("삭제 하시겠습니까?", "알림", MessageBoxButton.YesNo, MessageBoxImage.Information);
            if (result == MessageBoxResult.Yes)
            {
                MenuItem menuItem = (MenuItem)e.Source;
                Server server = (Server)menuItem.DataContext;

                if (server == null)
                {
                    MessageBox.Show("장비를 선택해 주세요", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    //logger.Debug(server.Id);
                    if (Server.DeleteServer(server) > 0)
                    {
                        logger.Info(String.Format("[{0}/{1} deleted]", server.Ip, server.Status));
                        foreach (Group g in NmsInfo.GetInstance().groupList)
                        {
                            foreach (Server s in g.Server)
                            {
                                if (s.Id == server.Id)
                                {
                                    g.Server.Remove(s);
                                    break;
                                }
                            }
                        }
                        int location = server.Location;
                        NmsInfo.GetInstance().serverList.Remove(server);
                        //액티브 알람 지움
                        List<LogItem> activeAlarmList = FindAllActiveAlarm(NmsInfo.GetInstance().activeLog, server.Ip);
                        foreach (LogItem item in activeAlarmList)
                        {
                            //NmsInfo.GetInstance().activeLog.Remove(item);
                            NmsInfo.GetInstance().activeLogRemove(item);
                        }

                        Server emptyServer = new Server();
                        NmsInfo.GetInstance().serverList.Insert(location, emptyServer);
                        //인스턴스를 삭제하지 않고 초기화 시킴
                        //server.Clear();
                    }
                    else
                    {
                        MessageBox.Show("삭제 실패");
                    }
                }
            }
        }

        private void ClosingServerDialog(object sender, MaterialDesignThemes.Wpf.DialogClosingEventArgs eventArgs)
        {
            if (this.IsEnabled == false)
            {
                this.IsEnabled = true;
            }

            Server temp = (Server)eventArgs.Session.Content;
            Server server = temp.ShallowCopy();

            if ((bool)eventArgs.Parameter == false)
            {
                //cancel button
                if (server.Undo != null)
                {
                    server.UnitName = server.Undo.UnitName;
                    server.Ip = server.Undo.Ip;
                    server.Grp_name = server.Undo.Grp_name;
                    server.Gid = server.Undo.Gid;
                }
            }
            else if ((bool)eventArgs.Parameter == true)
            {
                if (Utils.Util.IpValidCheck(server.Ip))
                {
                    //IEnumerable<Server> results = NmsInfo.GetInstance().serverList.Where(s => s.Ip == server.Ip);
                    //IEnumerable<Server> results = Server.GetServerList().Where(s => s.Ip == server.Ip);
                    IEnumerable<Server> results = Server.GetServerList();
                    results = results.Where(s => s.Ip == server.Ip);

                    //if (results.Count() > 0 && (server.Ip != server.Undo.Ip))
                    if (string.IsNullOrEmpty(server.Id))
                    {
                        if (results.Count() > 0)
                        {
                            MessageBox.Show(string.Format($"{server.Ip}는 이미 등록 되었습니다", "경고", MessageBoxImage.Warning, MessageBoxButton.OK));
                            eventArgs.Cancel();
                            return;
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(server.Ip))
                {
                    MessageBox.Show(string.Format($"{server.Ip}는 IP 형식에 맞지 않습니다", "경고", MessageBoxImage.Warning, MessageBoxButton.OK));
                    eventArgs.Cancel();
                    return;
                }

                try
                {
                    if (string.IsNullOrEmpty(server.Id))
                    {
                        if (string.IsNullOrEmpty(server.Ip))
                        {
                            MessageBox.Show(string.Format($"IP를 입력해 주세요", "경고", MessageBoxImage.Warning, MessageBoxButton.OK));
                            eventArgs.Cancel();
                        }
                        else if (!string.IsNullOrEmpty(server.Gid))
                        {
                            foreach (Group g in NmsInfo.GetInstance().groupList)
                            {
                                if (g.Id == server.Gid)
                                {
                                    server.Grp_name = g.Name;
                                    g.Server.Add(server);
                                    break;
                                }
                            }
                            server.Id = server.AddServer();
                            //NmsInfo.GetInstance().serverList.Add(server);

                            // 하나의 함수로 변경해야 함
                            MenuItem miEdit = new MenuItem();
                            miEdit.Header = "장비 수정";
                            MenuItem miDelete = new MenuItem();
                            miDelete.Header = "장비 삭제";
                            List<MenuItem> menus = new List<MenuItem>();
                            menus.Add(miEdit);
                            menus.Add(miDelete);
                            miEdit.Click += new System.Windows.RoutedEventHandler(this.MenuServerEdit_Click);
                            miDelete.Click += new System.Windows.RoutedEventHandler(this.MenuServerDel_Click);
                            server.MenuItems = menus;

                            server.Color = "#00FF7F";

                            server.GetNewLocation();

                            NmsInfo.GetInstance().serverList.Insert(server.Location, server);

                            //ServerListItem.ItemsSource = Server.GetServerList();
                            //TreeGroup.ItemsSource = Group.GetGroupList();
                        }
                        else
                        {
                            MessageBox.Show("그룹을 선택해 주세요");
                            eventArgs.Cancel();
                        }
                    }
                    else
                    {
                        if (temp.Ip != server.Ip)
                        {
                            LogItem.Flush(temp);
                        }
                        server.EditServer();
                        foreach (Group g in NmsInfo.GetInstance().groupList)
                        {
                            foreach (Server s in g.Server)
                            {
                                if (s.Id == server.Id)
                                {
                                    g.Server.Remove(s);
                                    break;
                                }
                            }
                            if (g.Id == server.Gid)
                            {
                                g.Server.Add(server);
                            }
                        }

                        //바인딩이 지저분해짐 한번에 할 수 있는것을 연구해야함
                        //TreeGroup.ItemsSource = _groupList;
                        //ServerListItem.ItemsSource = Server.GetServerList();
                        SnmpService.GetModelName(server);
                        SnmpService.Set(server);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex.ToString());
                }
            }

            //_snmpGetTimer.Start();
        }

        private void LoggingDisplay(LogItem log)
        {
            if (LvActiveLog.Dispatcher.CheckAccess())
            {
                NmsInfo.GetInstance().activeLog.Insert(0, log);
            }
            else
            {
                LvActiveLog.Dispatcher.Invoke(() => { NmsInfo.GetInstance().activeLog.Insert(0, log); });
            }
        }

        private void LoggingDisplay(List<LogItem> activeLog)
        {
            foreach (var log in activeLog)
            {
                if (LvActiveLog.Dispatcher.CheckAccess())
                {
                    NmsInfo.GetInstance().activeLogRemove(log);
                }
                else
                {
                    LvActiveLog.Dispatcher.Invoke(() => { NmsInfo.GetInstance().activeLogRemove(log); });
                }
            }
        }

        private void SoundPlay(string levelString)
        {
            if (NmsInfo.GetInstance().alarmInfo != null)
            {
                foreach (Alarm alarm in NmsInfo.GetInstance().alarmInfo)
                {
                    if (alarm.level.Equals(levelString))
                    {
                        if (File.Exists(alarm.path))
                        {
                            Task.Run(() => SoundPlayAsync(alarm.path));
                        }
                        else
                        {
                            Task.Run(() => SoundPlayAsync());
                        }
                        break;
                    }
                }
            }
            else
            {
                Task.Run(() => SoundPlayAsync());
            }
        }

        private async Task SoundPlayAsync()
        {
            if (_soundPlayer == null)
            {
                _soundPlayer = new SoundPlayer(@"Sound\alarm.wav");
                _soundPlayer.PlayLooping();
                await Task.Delay(5000);
                if (_soundPlayer != null) _soundPlayer.Stop();
                if (_soundPlayer != null) _soundPlayer = null;
            }
        }

        private async Task SoundPlayAsync(string path)
        {
            if (_soundPlayer == null)
            {
                _soundPlayer = new SoundPlayer(path);
                _soundPlayer.PlayLooping();
                await Task.Delay(5000);
                if (_soundPlayer != null) _soundPlayer.Stop();
                if (_soundPlayer != null) _soundPlayer = null;
            }
        }

        private async Task SoundPlayAsync(string path, bool preview)
        {
            if (_soundPlayer != null)
            {
                _soundPlayer.Stop();
                _soundPlayer = null;
            }
            _soundPlayer = new SoundPlayer(path);
            _soundPlayer.PlayLooping();
            logger.Info("play sound : " + path);
        }

        private void SoundStop()
        {
            if (_soundPlayer != null)
            {
                _soundPlayer.Stop();
                _soundPlayer = null;
            }
        }

        private void ServerList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string uri = "http://";
            ListView lv = sender as ListView;
            Server server = (Server)lv.SelectedItem;
            if (!string.IsNullOrEmpty(server.Ip))
            {
                System.Diagnostics.Process.Start(String.Format($"{uri}{server.Ip}"));
            }
        }

        #region ItemAccess Method

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

        private TreeView GetParentTreeview(DependencyObject source)
        {
            while (source != null && !(source is TreeView))
            {
                source = VisualTreeHelper.GetParent(source);
            }

            return source as TreeView;
        }

        #endregion ItemAccess Method

        private async void BtnMenuSetting_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;

            GlobalSettings settings = new GlobalSettings();
            settings.SnmpCM5000Settings = (List<SnmpSetting>)Snmp.GetTrapAlarmList("CM5000");
            settings.SnmpDR5000Settings = (List<SnmpSetting>)Snmp.GetTrapAlarmList("DR5000");
            settings.AlarmSettings = NmsInfo.GetInstance().alarmInfo;
            settings.SnmpPort = settings.GetSnmpPort();
            settings.PollingSec = settings.GetPollingSec(); ;
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

                //TreeGroup.ItemsSource = Group.GetGroupList();
                //ServerListItem.ItemsSource = Server.GetServerList();
            }
            if (_soundPlayer != null)
            {
                _soundPlayer.Stop();
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

            NmsInfo.GetInstance().logSearch = new ObservableCollection<LogItem>(LogItem.GetLog());
            var result = await DialogHost.Show(NmsInfo.GetInstance().logSearch, "DialogLogViewInfo");
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

            try
            {
                NmsInfo.GetInstance().logSearch = new ObservableCollection<LogItem>(LogItem.GetLog(DpDayFrom.Text, DpDayTo.Text, true));
                ListView lvDialog = (ListView)FindElemetByName(e, "lvLogSearch");
                lvDialog.ItemsSource = NmsInfo.GetInstance().logSearch;
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
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
            //csvString = LogItem.MakeCsvFile(LogItem.GetLog());
            csvString = LogItem.MakeCsvFile(NmsInfo.GetInstance().logSearch);

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
                //NmsInfo.GetInstance().activeLog.Remove(_currentSelectedItem);
                NmsInfo.GetInstance().activeLogRemove(_currentSelectedItem);
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

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string tabItem = ((sender as TabControl).SelectedItem as TabItem).Header as string;

            switch (tabItem)
            {
                case "히스토리 알람":
                    NmsInfo.GetInstance().historyLog = new NmsInfo.LimitedSizeObservableCollection<LogItem>(LogItem.GetLog(true), 1000);
                    LvHistory.ItemsSource = NmsInfo.GetInstance().historyLog;
                    break;
            }
        }

        private void BtnAlarmFileUpload_Click(object sender, RoutedEventArgs e)
        {
            string levelString = ((Button)e.Source).Uid;

            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Sound (*.WAV;*.MP3)|*.WAV;*.MP3";
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                string filename = dlg.FileName;
                string subpath = "alarmSound";

                logger.Info(string.Format($"Upload Alarm File Ready ({filename})"));
                if (File.Exists(filename))
                {
                    if (!Directory.Exists(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, subpath)))
                    {
                        Directory.CreateDirectory(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, subpath));
                    }
                    string destfilepath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, subpath, Path.GetFileName(filename));
                    File.Copy(filename, destfilepath, true);
                    if (Path.GetExtension(destfilepath).ToLower() == ".mp3")
                    {
                        Utils.Util.ConvertMp3ToWav(destfilepath);
                    }
                    var query = NmsInfo.GetInstance().alarmInfo.Where<Alarm>(a => a.level.Equals(levelString)).Take(1);
                    Alarm alarm = (Alarm)query.ElementAt(0);
                    alarm.path = Path.Combine(subpath, Path.ChangeExtension(Path.GetFileName(filename), "WAV"));
                    MessageBox.Show("업로드 완료", "정보", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void ListViewItem_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string uri = "http://";
            var obj = sender as ListViewItem;
            LogItem item = (LogItem)obj.DataContext;
            if (!string.IsNullOrEmpty(item.Ip))
            {
                System.Diagnostics.Process.Start(String.Format($"{uri}{item.Ip}"));
            }
        }

        private async void ServerInfo_Button_Click(object sender, RoutedEventArgs e)
        {
            foreach (Server s in NmsInfo.GetInstance().serverList)
            {
                if (SnmpService.GetInfomation(s))
                {
                    s.UpdateServerStatus();
                }
            }
            //open dialog
            this.IsEnabled = false;

            ObservableCollection<Server> servers = NmsInfo.GetInstance().RealServerList();
            var result = await DialogHost.Show(servers, "DialogServerInfo");
        }

        private void ClosingServerInfoDialogEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            if (this.IsEnabled == false)
            {
                this.IsEnabled = true;
            }
            /*
            if ((bool)eventArgs.Parameter == true)
            {
                //GlobalSettings settings = (GlobalSettings)eventArgs.Session.Content;
                //Snmp.UpdateSnmpMessgeUseage(settings);

                //TreeGroup.ItemsSource = Group.GetGroupList();
                //ServerListItem.ItemsSource = Server.GetServerList();
            }
            */
        }

        private void ServerInfoSetting_Button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            Server s = (Server)btn.DataContext;
            if (SnmpService.Set(s))
            {
                MessageBox.Show("성공", "정보", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("실패", "정보", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnInformationImport_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("import");
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "json files (*.json)|*.json";
                if (ofd.ShowDialog() == true)
                {
                    string importJson = File.ReadAllText(ofd.FileName);
                    /*
                    var jObj = JObject.Parse(importJson);

                    ObservableCollection<Server> servers = jObj["serverList"].ToObject<ObservableCollection<Server>>();
                    ObservableCollection<Group> groups = jObj["groupList"].ToObject<ObservableCollection<Group>>();
                    //Group.ImportGroup(groups);
                    //Server.ImportServer(servers);

                    //_snmpGetTimer.Stop();
                    MessageBox.Show("새로운 장비 정보를 등록했습니다", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                    ServerDispatcherTimer();
                    */
                    DialogHost.Close("DialogServerInfo");

                    if (PbMainLoading.Visibility == Visibility.Hidden)
                    {
                        PbMainLoading.Visibility = Visibility.Visible;
                        ServerListItem.Visibility = Visibility.Hidden;
                        BtnServerInfo.IsEnabled = false;
                    }
                }
                /*
                ObservableCollection<Server> servers = new ObservableCollection<Server>();
                servers = JsonConvert.DeserializeObject<ObservableCollection<Server>>()
                NmsInfo.GetInstance().serverList
                */
            }
            catch (Exception)
            {
            }
        }

        private void BtnInformationExport_Click(object sender, RoutedEventArgs e)
        {
            string jsonExport = JsonConvert.SerializeObject(NmsInfo.GetInstance(), Formatting.Indented);

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "json file (*.json)|*.json";
            string downloadsPath = KnownFolders.GetPath(KnownFolder.Downloads);
            saveFileDialog.InitialDirectory = downloadsPath;
            saveFileDialog.FileName = "export";
            if (String.IsNullOrEmpty(jsonExport))
            {
                MessageBox.Show("저장할 항목이 없습니다", "정보", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(saveFileDialog.FileName, jsonExport);
                MessageBox.Show("저장 되었습니다", "정보", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void TreeItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                string uri = "http://";

                var obj = sender as StackPanel;
                string Ip = obj.Uid;

                if (!string.IsNullOrEmpty(Ip))
                {
                    System.Diagnostics.Process.Start(String.Format($"{uri}{Ip}"));
                }
            }
        }

        private void ServerSettings_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string uri = "http://";
            try
            {
                ListView lv = sender as ListView;
                Server server = (Server)lv.SelectedItem;
                if (!string.IsNullOrEmpty(server.Ip))
                {
                    System.Diagnostics.Process.Start(String.Format($"{uri}{server.Ip}"));
                }
            }
            catch
            {
            }
        }

        private void BtnScreenLock_Click(object sender, RoutedEventArgs e)
        {
            if (ServerListItem.IsEnabled)
            {
                MessageBox.Show("잠금 되었습니다", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("활성화 되었습니다", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            ServerListItem.IsEnabled = !ServerListItem.IsEnabled;
        }

        private void BtnRevert_Click(object sender, RoutedEventArgs e)
        {
            if (NmsInfo.GetInstance().serverListStack.Count > 0)
            {
                NmsInfo.GetInstance().serverList = NmsInfo.GetInstance().serverListStack.Pop();
                ServerListItem.ItemsSource = null;
                ServerListItem.ItemsSource = NmsInfo.GetInstance().serverList;
            }
            else
            {
                MessageBox.Show("더 이상 되돌릴 수 없습니다", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Thumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            UIElement thumb = e.Source as UIElement;

            Canvas.SetLeft(thumb, Canvas.GetLeft(thumb) + e.HorizontalChange);
            Canvas.SetTop(thumb, Canvas.GetTop(thumb) + e.VerticalChange);
        }

        private void BtnAddPanel_Click(object sender, RoutedEventArgs e)
        {
            string inputRead = new InputBox("패널이름").ShowDialog();
            if (!string.IsNullOrEmpty(inputRead))
            {
                //패널 추가
                MessageBox.Show(inputRead);
                MakePanel(inputRead);
            }
        }

        private void MakePanel(string panelName)
        {
            /*
            Thumb thumb = new Thumb();
            thumb.DragDelta += Thumb_DragDelta;
            thumb.MouseDoubleClick += thumb_MouseDoubleClick;
            thumb.Cursor = Cursors.Hand;
            thumb.Style = this.Resources["ThumbStyle"] as Style;

            Label lb = (Label)thumb.Template.FindName("lbPanel", thumb);

            Border bd = new Border();
            Label lb = new Label();
            lb.Content = panelName;
            bd.Child = lb;
            bd.Style = this.Resources["BorderVisibleOnMouse"] as Style;
            Canvas.SetLeft(thumb, 50);
            Canvas.SetTop(thumb, 50);
            cnvPanel.Children.Add(thumb);
            */
        }

        private void thumb_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("더블클릭");
        }

        private GridViewColumnHeader _lastHeaderClicked = null;
        private ListSortDirection _lastDirection = ListSortDirection.Ascending;

        private void Sort(string sortBy, ListSortDirection direction, object sender)
        {
            ListView target = sender as ListView;
            //ICollectionView dataView = CollectionViewSource.GetDefaultView(LvHistory.ItemsSource);
            ICollectionView dataView = CollectionViewSource.GetDefaultView(target.ItemsSource);

            dataView.SortDescriptions.Clear();
            SortDescription sd = new SortDescription(sortBy, direction);
            dataView.SortDescriptions.Add(sd);
            dataView.Refresh();
        }

        private void LvSort_Click(object sender, RoutedEventArgs e)
        {
            var headerClicked = e.OriginalSource as GridViewColumnHeader;
            ListSortDirection direction;

            if (headerClicked != null)
            {
                if (headerClicked.Role != GridViewColumnHeaderRole.Padding)
                {
                    if (headerClicked != _lastHeaderClicked)
                    {
                        direction = ListSortDirection.Ascending;
                    }
                    else
                    {
                        if (_lastDirection == ListSortDirection.Ascending)
                        {
                            direction = ListSortDirection.Descending;
                        }
                        else
                        {
                            direction = ListSortDirection.Ascending;
                        }
                    }

                    var columnBinding = headerClicked.Column.DisplayMemberBinding as Binding;
                    var sortBy = columnBinding?.Path.Path ?? headerClicked.Column.Header as string;

                    Sort(sortBy, direction, sender);

                    if (direction == ListSortDirection.Ascending)
                    {
                        headerClicked.Column.HeaderTemplate =
                          Resources["HeaderTemplateArrowUp"] as DataTemplate;
                    }
                    else
                    {
                        headerClicked.Column.HeaderTemplate =
                          Resources["HeaderTemplateArrowDown"] as DataTemplate;
                    }

                    // Remove arrow from previously sorted header
                    if (_lastHeaderClicked != null && _lastHeaderClicked != headerClicked)
                    {
                        _lastHeaderClicked.Column.HeaderTemplate = null;
                    }

                    _lastHeaderClicked = headerClicked;
                    _lastDirection = direction;
                }
            }
        }

        private void Button_AlarmSoundClick(object sender, RoutedEventArgs e)
        {
            Button b = (Button)e.Source as Button;
            string path = b.Uid;

            try
            {
                if (File.Exists(path))
                {
                    Task.Run(() => SoundPlayAsync(path, true));
                }
                else
                {
                    MessageBox.Show("재생할 파일이 없습니다");
                }
            }
            catch (Exception ex)
            {
                logger.Error(path);
                logger.Error(ex.ToString());
            }
        }

        private void btn_Port_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender as Button;
            GlobalSettings gs = (GlobalSettings)btn.DataContext;
            gs.SetSnmpPort(gs.SnmpPort);
        }

        private void btn_Polling_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender as Button;
            GlobalSettings gs = (GlobalSettings)btn.DataContext;
            gs.SetPollingSec(gs.PollingSec);
        }
    }
}