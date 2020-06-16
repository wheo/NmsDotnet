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

using NmsDotNet.Database.vo;

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

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadMibFiles();
            GetGroupList();
            GetServerList();
            GetLog();

            await Task.Run(() => TrapListener());
            Debug.WriteLine("TrapListener Completed");
        }

        private void LoadMibFiles()
        {
            String path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MIB");
            String[] files = Directory.GetFiles(path);
        }

        private void GetGroupList()
        {
            var groups = Database.vo.Group.GetInstance().GetGroupList();
            if (groups.Count > 0)
            {
                TreeGroup.ItemsSource = groups;
            }
        }

        private void GetServerList()
        {
            var servers = Database.vo.Server.GetInstance().GetServerList();
            if (servers.Count > 0)
            {
                ListViewServer.ItemsSource = servers;
            }
        }

        private void GetLog()
        {
            DgLog.ItemsSource = LogItem.GetInstance().GetLog().DefaultView;
        }

        private void TrapListener()
        {
            Debug.WriteLine("hello await task");

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
                                Database.vo.Snmp snmp = new Database.vo.Snmp()
                                {
                                    Id = v.Oid.ToString()
                                    ,
                                    IP = inep.ToString().Split(':')[0]
                                    ,
                                    Port = inep.ToString().Split(':')[1]
                                    ,
                                    Community = pkt.Community.ToString()
                                    ,
                                    Syntax = SnmpConstants.GetTypeName(v.Value.Type)
                                    ,
                                    Value = v.Value.ToString()
                                    ,
                                    type = "trap"
                                };
                                Database.vo.Snmp.GetInstance().RegisterSnmpInfo(snmp);
                                LogItem.GetInstance().LoggingDatabase(snmp);

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

        private void ListViewProducts_MouseMove(object sender, MouseEventArgs e)
        {
            var item = sender as ItemsControl;
        }

        private void ListViewProducts_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("right down");
            //Snmp.Get();
            Service.SnmpService.GetNext();
        }

        private void ListViewProducts_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            /*
            MessageBox.Show("left down");
            Service.Snmp.GetBulk();
            */
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Debug.WriteLine("NMS main windows closing");
            _shouldStop = true;
        }

        private void MenuGroupAdd_Click(object sender, RoutedEventArgs e)
        {
            DialogGroup.IsOpen = true;
        }

        private void MenuGroupEdit_Click(object sender, RoutedEventArgs e)
        {
        }

        private void MenuGroupDel_Click(object sender, RoutedEventArgs e)
        {
        }

        private void MenuMachineAdd_Click(object sender, RoutedEventArgs e)
        {
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
            Group.GetInstance().AddGroup(TbGroupName.Text);
        }
    }
}