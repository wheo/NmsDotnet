﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Diagnostics;
using System.Net.Sockets;
using log4net;

using SnmpSharpNet;

using NmsDotNet.Service;
using NmsDotNet.vo;
using NmsDotNet.Utils;
using NmsDotNet.Database.vo;
using System.Windows.Controls.Primitives;
using System.Net;
using System.Runtime.CompilerServices;

using System.Runtime.Remoting.Contexts;
using System.Media;
using System.Windows.Media;
using System.Windows.Data;
using System.Collections.ObjectModel;

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

        private SoundPlayer _simpleSound = null;

        //CancellationTokenSource source;
        public NmsMainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadMibFiles();
            GetGroupList();
            ServerDispatcherTimer();
            GetLog();

            Task.Run(() => TrapListener());
        }

        private void LoadMibFiles()
        {
            String path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MIB");
            String[] files = Directory.GetFiles(path);
        }

        private void GetGroupList()
        {
            TreeGroup.ItemsSource = Group.GetInstance().GetGroupList();
        }

        private int GetLog()
        {
            LvLog.ItemsSource = null;
            LvLog.ItemsSource = LogItem.GetInstance().GetLog();
            return LogItem.GetInstance().LogItemCount;
        }

        private void ServerDispatcherTimer()
        {
            DispatcherTimer SnmpGetTimer = new DispatcherTimer(DispatcherPriority.Render);
            SnmpGetTimer.Interval = TimeSpan.FromSeconds(5);
            SnmpGetTimer.Tick += new EventHandler(SService);
            _serverList = new ObservableCollection<Server>(Server.GetServerList());
            ServerListItem.ItemsSource = _serverList;
            SnmpGetTimer.Start();
        }

        private void SService(object sender, EventArgs e)
        {
            var Servers = _serverList;
            bool drawItem = false;
            foreach (Server server in Servers)
            {
                string old_status = null;
                if (SnmpService.Get(server.Ip))
                {
                    //logger.Debug(String.Format("[{0}/{1}] ServerService current status", server.Ip, server.Status));
                    if (server.Status == "idle" || server.Status == "error")
                    {
                        server.Status = "normal";
                        //server.IsChange = true; INotify 인터페이스로 대체
                        logger.Info(String.Format($"[{server.Ip}] ServerService ({server.Status}) status changed"));
                        drawItem = true;
                    }
                }
                else
                {
                    if (server.Status == "normal")
                    {
                        server.Status = "error";
                        //server.IsChange = true;
                        logger.Info(String.Format($"[{server.Ip}] ServerService ({server.Status}) status changed"));
                        drawItem = true;
                    }
                    if (server.Status == "idle")
                    {
                        //최초 등록시 snmp get 응답 없을땐 idle 상태로 계속 유지
                    }
                }
            }

            if (drawItem)
            {
                if (ServerListItem.Dispatcher.CheckAccess())
                {
                    ServerListItem.ItemsSource = null;
                    ServerListItem.ItemsSource = Server.GetServerList();
                }
                else
                {
                    ServerListItem.Dispatcher.Invoke(() =>
                    {
                        ServerListItem.ItemsSource = null;
                        ServerListItem.ItemsSource = Server.GetServerList();
                    });
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

        private void MenuGroupAdd_Click(object sender, RoutedEventArgs e)
        {
            DialogGroup.IsOpen = true;
        }

        private void MenuGroupEdit_Click(object sender, RoutedEventArgs e)
        {
            DialogGroup.IsOpen = true;

            /*
            MenuItem menuItem = (MenuItem)e.Source;
            ContextMenu menu = (ContextMenu)menuItem.Parent;
            TreeView tv = (TreeView)menu.PlacementTarget;
            if ((Group)tv.SelectedItem == null)
            {
                MessageBox.Show("선택된 그룹이 없습니다.");
            }
            else
            {
                var group = (Group)tv.SelectedItem;
                Debug.WriteLine(group.Id);
                if (Group.GetInstance().EditGroup(group) > 0)
                {
                }
            }
            */
        }

        private void MenuGroupDel_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)e.Source;
            ContextMenu menu = (ContextMenu)menuItem.Parent;
            TreeView tv = (TreeView)menu.PlacementTarget;
            if ((Group)tv.SelectedItem == null)
            {
                MessageBox.Show("선택된 그룹이 없습니다.");
            }
            else
            {
                var group = (Group)tv.SelectedItem;
                if (Group.GetInstance().DeleteGroup(group.Id) > 0)
                {
                    TreeGroup.ItemsSource = Group.GetInstance().GetGroupList();
                    ServerListItem.ItemsSource = Server.GetServerList();
                    //서비스 스레드 종료 꼭 해야함
                }
            }
        }

        private void MenuServerAdd_Click(object sender, RoutedEventArgs e)
        {
            //clear 먼저
            DialogServer.IsOpen = true;
        }

        private void MenuServerEdit_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)e.Source;
            ContextMenu menu = (ContextMenu)menuItem.Parent;
            ListView lv = (ListView)menu.PlacementTarget;
            Server info = (Server)lv.SelectedItem;
            if (info == null)
            {
                MessageBox.Show("선택된 그룹이 없습니다.");
            }
            logger.Debug(info.GroupName);
            logger.Debug(info.Gid);
            TbServerIp.Text = info.Ip;
            TbServerName.Text = info.Name;
            CbGroup.Text = info.GroupName;
            DialogServer.IsOpen = true;
        }

        private void MenuServerDel_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)e.Source;
            ContextMenu menu = (ContextMenu)menuItem.Parent;
            ListView lv = (ListView)menu.PlacementTarget;
            if ((Server)lv.SelectedItem == null)
            {
                MessageBox.Show("선택된 그룹이 없습니다.");
            }
            else
            {
                var server = (Server)lv.SelectedItem;
                logger.Debug(server.Id);
                if (Server.DeleteServer(server) > 0)
                {
                    logger.Debug(String.Format("[{0}/{1} deleted]", server.Ip, server.Status));

                    TreeGroup.ItemsSource = Group.GetInstance().GetGroupList();
                    ServerListItem.ItemsSource = Server.GetServerList();
                }
            }
        }

        private void BtnGroupAdd_Click(object sender, RoutedEventArgs e)
        {
            // group Add
            if (Group.GetInstance().AddGroup(TbGroupName.Text) > 0)
            {
                TreeGroup.ItemsSource = Group.GetInstance().GetGroupList();
            }
        }

        private void BtnServerAdd_Click(object sender, RoutedEventArgs e)
        {
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
        }

        private void DialogServer_DialogOpened(object sender, MaterialDesignThemes.Wpf.DialogOpenedEventArgs eventArgs)
        {
            CbGroup.ItemsSource = Group.GetInstance().GetGroupList();
        }

        private void InitServerPopup()
        {
            TbServerIp.Text = null;
            TbServerName.Text = null;
        }

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
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 0);

            int inlen = -1;
            while (!_shouldStop)
            {
                byte[] indata = new byte[16 * 1024];
                // 16KB receive buffer int inlen = 0;
                IPEndPoint peer = new IPEndPoint(IPAddress.Any, 0);
                EndPoint inep = (EndPoint)peer;
                try
                {
                    Debug.WriteLine("Waiting for snmp trap");
                    inlen = socket.ReceiveFrom(indata, ref inep);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception {0}", ex.Message);
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
                        Debug.WriteLine("** SNMP Version 2 TRAP received from {0}:", inep.ToString());
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
                            int logCount = 0;
                            if (!String.IsNullOrEmpty(snmp.LevelString))
                            {
                                snmp.TrapString = snmp.MakeTrapLogString();
                                LogItem.GetInstance().LoggingDatabase(snmp);
                                if (LvLog.Dispatcher.CheckAccess())
                                {
                                    logCount = GetLog();
                                }
                                else
                                {
                                    LvLog.Dispatcher.Invoke(() => { logCount = GetLog(); });
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
                            if (logCount > 0)
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
            TreeGroup.ItemsSource = Group.GetInstance().GetGroupList();
            //Task.Run(() => ServerService(server)); // 스레드 돌리지 않음
            logger.Info(String.Format("{0} New Service is created", server.Ip));
        }

        private void TreeGroup_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            logger.Info(sender);
        }

        private void lvBtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            LogItem logItem = GetLvItem(e);
            LogItem.ChangeConfirmStatus(logItem.idx);
            logger.Info(String.Format($"{logItem.Ip}, {logItem.Value}, {logItem.idx}"));
            GetLog();
        }

        private LogItem GetLvItem(RoutedEventArgs e)
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
            item.Background = Brushes.Transparent;
            LogItem content = (LogItem)item.Content;
            return content;
        }
    }
}