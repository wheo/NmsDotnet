using NnmDotnet.vo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NmsDotnet
{
    /// <summary>
    /// NmsMainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class NmsMainWindow : Window
    {
        public NmsMainWindow()
        {
            InitializeComponent();
            var products = GetProducts();
            if (products.Count > 0 )
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
                new Product("Product 1", 250.46, "/Asset/item/1.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/2.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/1.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/2.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/1.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/2.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/1.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/2.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/1.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/2.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/1.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/2.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/1.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/2.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/1.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/2.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/1.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/2.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/1.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/2.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/1.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/2.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/1.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/2.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/1.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/2.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/1.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/2.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/1.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/2.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/1.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/2.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/1.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/2.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/1.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/2.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/1.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/2.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/1.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/2.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/1.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/2.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/1.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/2.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/1.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/2.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/1.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/2.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/1.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/2.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/1.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/2.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/1.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/2.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/1.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/2.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/1.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/2.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/1.jpg"),
                new Product("Product 1", 250.46, "/Asset/item/2.jpg"),
            };                        
        }
    }
}
