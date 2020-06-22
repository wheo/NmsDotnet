using System;
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

namespace NmsDotNet
{
    /// <summary>
    /// NmsMainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class NmsMainWindow : Window
    {
        private static readonly ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static bool _shouldStop = false;

        //CancellationTokenSource source;
        public NmsMainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadMibFiles();
            GetGroupList();
            GetServerList();
            GetLog();

            Task.Run(() => TrapListener());
            Debug.WriteLine("TrapListener Completed");
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

        private void GetServerList()
        {
            ServerList.ItemsSource = Server.GetInstance().GetServerList();

            foreach (Server item in ServerList.ItemsSource)
            {
                Task.Run(() => ServerService(item));
            }
        }

        private void GetLog()
        {
            DgLog.ItemsSource = LogItem.GetInstance().GetLog().DefaultView;
        }

        private void ServerService(Server server)
        {
            logger.Info(String.Format("{0} ServerService is starting", server.Ip));
            if (ServerList.Dispatcher.CheckAccess())
            {
                ServerList.ItemsSource = Server.GetInstance().GetServerList();
            }
            else
            {
                ServerList.Dispatcher.Invoke(() => { ServerList.ItemsSource = Server.GetInstance().GetServerList(); });
            }

            while (!_shouldStop)
            {
                if (SnmpService.Get(server.Ip))
                {
                }
                else
                {
                    server.Status = "error";
                    Server.GetInstance().UpdateServerStatus(server);                    
                }
                Thread.Sleep(1000); // 나중에 변수로
            }
            logger.Info(String.Format("{0} ServerService is done", server.Ip));
        }

        private void ServerList_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            //MessageBox.Show("Get");
            //Service.SnmpService.GetNext();
        }

        private void ServerList_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
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
                Debug.WriteLine(group.Id);
                if (Group.GetInstance().DelGroup(group.Id) > 0)
                {
                    TreeGroup.ItemsSource = Group.GetInstance().GetGroupList();
                    ServerList.ItemsSource = Server.GetInstance().GetServerList();
                    //서비스 스레드 종료 꼭 해야함
                }
            }
        }

        private void MenuMachineAdd_Click(object sender, RoutedEventArgs e)
        {
            DialogServer.IsOpen = true;
        }

        private void MenuMachineEdit_Click(object sender, RoutedEventArgs e)
        {
        }

        private void MenuMachineDel_Click(object sender, RoutedEventArgs e)
        {
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
                return;
            }
            else
            {
                var group = (Group)CbGroup.SelectedItem;
                Server.GetInstance().SetServerInfo(TbServerName.Text, TbServerIp.Text, group.Id);
                Server.GetInstance().AddServer();
                ServerList.ItemsSource = Server.GetInstance().GetServerList();
                TreeGroup.ItemsSource = Group.GetInstance().GetGroupList();
                Task.Run(() => ServerService(Server.GetInstance()));
                logger.Info(String.Format("{0} New Service is created", Server.GetInstance().Ip));
            }
        }

        private void DialogServer_DialogOpened(object sender, MaterialDesignThemes.Wpf.DialogOpenedEventArgs eventArgs)
        {
            CbGroup.ItemsSource = Group.GetInstance().GetGroupList();
        }

        private void TrapListener()
        {
            Debug.WriteLine("hello task");

            // Construct a socket and bind it to the trap manager port 162
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.ReceiveTimeout = 1000;
            socket.SendTimeout = 1000;
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
                    Debug.WriteLine("Waiting for snmp");
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
                        Debug.WriteLine("** SNMP Version 1 TRAP received from {0}:", inep.ToString());
                        Debug.WriteLine("*** Trap generic: {0}", pkt.Pdu.Generic);
                        Debug.WriteLine("*** Trap specific: {0}", pkt.Pdu.Specific);
                        Debug.WriteLine("*** Agent address: {0}", pkt.Pdu.AgentAddress.ToString());
                        Debug.WriteLine("*** Timestamp: {0}", pkt.Pdu.TimeStamp.ToString());
                        Debug.WriteLine("*** VarBind count: {0}", pkt.Pdu.VbList.Count);
                        Debug.WriteLine("*** VarBind content:");
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
                            foreach (Vb v in pkt.Pdu.VbList)
                            {
                                Snmp.GetInstance().Id = v.Oid.ToString();
                                Snmp.GetInstance().IP = inep.ToString().Split(':')[0];
                                Snmp.GetInstance().Port = inep.ToString().Split(':')[1];
                                Snmp.GetInstance().Community = pkt.Community.ToString();
                                Snmp.GetInstance().Syntax = SnmpConstants.GetTypeName(v.Value.Type);
                                Snmp.GetInstance().Value = v.Value.ToString();
                                Snmp.GetInstance().type = "trap";

                                Snmp.GetInstance().RegisterSnmpInfo(Snmp.GetInstance());
                                LogItem.GetInstance().LoggingDatabase(Snmp.GetInstance());

                                if (DgLog.Dispatcher.CheckAccess())
                                {
                                    DgLog.ItemsSource = LogItem.GetInstance().GetLog().DefaultView;
                                }
                                else
                                {
                                    DgLog.Dispatcher.Invoke(() => { DgLog.ItemsSource = LogItem.GetInstance().GetLog().DefaultView; });
                                }

                                if (DialogNotification.Dispatcher.CheckAccess())
                                {
                                    DialogNotification.IsOpen = true;
                                }
                                else
                                {
                                    DialogNotification.Dispatcher.Invoke(() => { DialogNotification.IsOpen = true; });
                                }

                                Debug.WriteLine("**** {0} {1}: {2}",
                                    v.Oid.ToString(), SnmpConstants.GetTypeName(v.Value.Type), v.Value.ToString());
                                logger.Info(String.Format("[{0}] Trap : {1} {2}: {3}", inep.ToString().Split(':')[0], v.Oid.ToString(), SnmpConstants.GetTypeName(v.Value.Type), v.Value.ToString()));
                            }
                            Debug.WriteLine("** End of SNMP Version 2 TRAP data.");
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
            string group_id = "12d4b03e-b055-11ea-ab0a-0242ac130002"; //고정
            Server.GetInstance().SetServerInfo("테스트서버", "127.0.0.1", group_id);
            Server.GetInstance().AddServer();
            ServerList.ItemsSource = Server.GetInstance().GetServerList();
            TreeGroup.ItemsSource = Group.GetInstance().GetGroupList();
            Task.Run(() => ServerService(Server.GetInstance()));
            logger.Info(String.Format("{0} New Service is created", Server.GetInstance().Ip));
        }
    }
}