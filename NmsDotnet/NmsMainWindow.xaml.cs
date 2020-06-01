
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;
using log4net;

using SnmpSharpNet;

using NmsDotNet.lib;
using NmsDotNet.vo;
using NmsDotNet.Utils;
using NmsDotNet.Database.vo;

namespace NmsDotNet
{
    /// <summary>
    /// NmsMainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class NmsMainWindow : Window
    {
        private static readonly ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        CancellationTokenSource source;        
        public NmsMainWindow()
        {
            InitializeComponent();
            LoadMibFiles();
            GetServerList();
        }

        private void GetServerList()
        {
            List<Server> server = Server.GetInstance().GetServerList();
        }

        private void LoadMibFiles()
        {
            String path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MIB");
            String[] files = Directory.GetFiles(path);
        }

        private void InitializeItems()
        {
            var products = GetProducts();
            if (products.Count > 0)
            {
                ListViewProducts.ItemsSource = products;
            }

            LogItem.GetInstance().Add(new LogItem() { Server = "서버1", Info = "정보1", Desc = "설명1" });
            LogItem.GetInstance().Add(new LogItem() { Server = "서버1", Info = "정보1", Desc = "설명1" });
            LogItem.GetInstance().Add(new LogItem() { Server = "서버1", Info = "정보1", Desc = "설명1" });
            LogItem.GetInstance().Add(new LogItem() { Server = "서버1", Info = "정보1", Desc = "설명1" });
            LogItem.GetInstance().Add(new LogItem() { Server = "서버1", Info = "정보1", Desc = "설명1" });
            LogItem.GetInstance().Add(new LogItem() { Server = "서버1", Info = "정보1", Desc = "설명1" });
            LogItem.GetInstance().Add(new LogItem() { Server = "서버1", Info = "정보1", Desc = "설명1" });
            LogItem.GetInstance().Add(new LogItem() { Server = "서버1", Info = "정보1", Desc = "설명1" });
            LogItem.GetInstance().Add(new LogItem() { Server = "서버1", Info = "정보1", Desc = "설명1" });
            LogItem.GetInstance().Add(new LogItem() { Server = "서버1", Info = "정보1", Desc = "설명1" });
            LogItem.GetInstance().Add(new LogItem() { Server = "서버1", Info = "정보1", Desc = "설명1" });
            LogItem.GetInstance().Add(new LogItem() { Server = "서버1", Info = "정보1", Desc = "설명1" });
            LogItem.GetInstance().Add(new LogItem() { Server = "서버1", Info = "정보1", Desc = "설명1" });
            LogItem.GetInstance().Add(new LogItem() { Server = "서버1", Info = "정보1", Desc = "설명1" });
            LogItem.GetInstance().Add(new LogItem() { Server = "서버1", Info = "정보1", Desc = "설명1" });
            LogItem.GetInstance().Add(new LogItem() { Server = "서버1", Info = "정보1", Desc = "설명1" });
            LogItem.GetInstance().Add(new LogItem() { Server = "서버1", Info = "정보1", Desc = "설명1" });

            LvLog.ItemsSource = LogItem.GetInstance();
        }

        private List<Product> GetProducts()
        {
            return new List<Product>()
            {
                new Product("CM5000", "정상", "/Asset/item/server.png"),
                new Product("CM5000", "정상", "/Asset/item/server.png"),
                new Product("CM5000", "정상", "/Asset/item/server.png"),
                new Product("CM5000", "정상", "/Asset/item/server.png"),
                new Product("CM5000", "정상", "/Asset/item/server.png"),
                new Product("CM5000", "정상", "/Asset/item/server.png"),
                new Product("CM5000", "정상", "/Asset/item/server.png"),
                new Product("CM5000", "정상", "/Asset/item/server.png"),
                new Product("CM5000", "정상", "/Asset/item/server.png"),
                new Product("CM5000", "정상", "/Asset/item/server.png"),
                new Product("CM5000", "정상", "/Asset/item/server.png"),
                new Product("CM5000", "정상", "/Asset/item/server.png"),
                new Product("CM5000", "정상", "/Asset/item/server.png"),
                new Product("CM5000", "정상", "/Asset/item/server.png"),
                new Product("CM5000", "정상", "/Asset/item/server.png"),
                new Product("CM5000", "정상", "/Asset/item/server.png"),
                new Product("CM5000", "정상", "/Asset/item/server.png"),
                new Product("CM5000", "정상", "/Asset/item/server.png"),
                new Product("CM5000", "정상", "/Asset/item/server.png"),
                new Product("CM5000", "정상", "/Asset/item/server.png"),
                new Product("CM5000", "정상", "/Asset/item/server.png"),
                new Product("CM5000", "정상", "/Asset/item/server.png"),
                new Product("CM5000", "정상", "/Asset/item/server.png"),
                new Product("CM5000", "정상", "/Asset/item/server.png"),
                new Product("CM5000", "정상", "/Asset/item/server.png"),
                new Product("CM5000", "정상", "/Asset/item/server.png"),
                new Product("CM5000", "정상", "/Asset/item/server.png"),
                new Product("CM5000", "정상", "/Asset/item/server.png"),
                new Product("CM5000", "정상", "/Asset/item/server.png"),
                new Product("CM5000", "정상", "/Asset/item/server.png"),
                new Product("CM5000", "정상", "/Asset/item/server.png"),
                new Product("CM5000", "정상", "/Asset/item/server.png"),
                new Product("CM5000", "정상", "/Asset/item/server.png"),
                new Product("CM5000", "정상", "/Asset/item/server.png"),
                new Product("CM5000", "정상", "/Asset/item/server.png"),
                new Product("CM5000", "정상", "/Asset/item/server.png"),
                new Product("CM5000", "정상", "/Asset/item/server.png"),
                new Product("CM5000", "정상", "/Asset/item/server.png"),
                new Product("CM5000", "정상", "/Asset/item/server.png"),
                new Product("CM5000", "정상", "/Asset/item/server.png"),
                new Product("CM5000", "정상", "/Asset/item/server.png"),
                new Product("CM5000", "정상", "/Asset/item/server.png"),
                new Product("CM5000", "정상", "/Asset/item/server.png"),
                new Product("CM5000", "정상", "/Asset/item/server.png"),
                new Product("CM5000", "정상", "/Asset/item/server.png"),
                new Product("CM5000", "정상", "/Asset/item/server.png"),
                new Product("CM5000", "정상", "/Asset/item/server.png"),
                new Product("CM5000", "정상", "/Asset/item/server.png"),
                new Product("CM5000", "정상", "/Asset/item/server.png"),
                new Product("CM5000", "정상", "/Asset/item/server.png"),
                new Product("CM5000", "정상", "/Asset/item/server.png"),
                new Product("CM5000", "정상", "/Asset/item/server.png"),
                new Product("CM5000", "정상", "/Asset/item/server.png"),
                new Product("CM5000", "정상", "/Asset/item/server.png"),
                new Product("CM5000", "정상", "/Asset/item/server.png"),
                new Product("CM5000", "정상", "/Asset/item/server.png"),
            };
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeItems();
            LoadMIBs();
            TrapListener();
        }

        private void TrapListener()
        {
            var TaskTrapListener = Task.Run(() => Snmp.TrapListener());
            //bool result = await Snmp.TrapListener();
            Debug.WriteLine("hello await task");
        }

        private void LoadMIBs()
        {

        }

        private void ListViewProducts_MouseMove(object sender, MouseEventArgs e)
        {
            var item = sender as ItemsControl;
        }

        private void ListViewProducts_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("right down");
            Snmp.Get();
        }

        private void ListViewProducts_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("left down");

            TrapAgent agent = new TrapAgent();
            // Variable Binding collection to send with the trap
            VbCollection col = new VbCollection();
            col.Add(new Oid("1.3.6.1.2.1.1.1.0"), new SnmpSharpNet.OctetString("Test string"));
            col.Add(new Oid("1.3.6.1.2.1.1.2.0"), new Oid("1.3.6.1.9.1.1.0"));
            col.Add(new Oid("1.3.6.1.2.1.1.3.0"), new SnmpSharpNet.TimeTicks(2324));
            col.Add(new Oid("1.3.6.1.2.1.1.4.0"), new SnmpSharpNet.OctetString("Milan"));
            // Send the trap to the localhost port 162
            agent.SendV1Trap(new IpAddress("192.168.2.81"), 162, "public",
                             new Oid("1.3.6.1.2.1.1.1.0"), new IpAddress(Util.GetLocalIpAddress()),
                             SnmpConstants.LinkUp, 0, 13432, col);            


            /*
            Messenger.SendTrapV1(new IPEndPoint(IPAddress.Parse("192.168.2.81"), 162),
                    IPAddress.Parse("192.168.2.77"),
                    new OctetString("public"),
                    new ObjectIdentifier("1.3.6.1.2.1.1"),
                    GenericCode.ColdStart,
                    0,
                    0,
                    new List<Variable>());
                    */
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Debug.WriteLine("NMS main windows closing");
            Snmp._shouldStop = true;
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
    }        
}
